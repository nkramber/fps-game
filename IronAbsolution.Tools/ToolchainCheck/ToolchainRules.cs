using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>The result of one pin: the tool, the expected value, the found value, and the next step on a failure.</summary>
/// <param name="Tool">The name of the tool, such as `Visual Studio`.</param>
/// <param name="Holds">True when the found value meets the pin.</param>
/// <param name="Expected">The pin, with the decision that sets it.</param>
/// <param name="Found">The value on this machine, or the reason that no value exists (T-2).</param>
/// <param name="Advice">The next step on a failure, or the empty text.</param>
public sealed record PinResult(string Tool, bool Holds, string Expected, string Found, string Advice)
{
    /// <summary>Gives the one report line of the pin.</summary>
    /// <returns>The line, such as `Unreal Engine: pass. Expected 5.8.3 (D-28), found 5.8.3.`</returns>
    public string Line()
    {
        string state = this.Holds ? "pass" : "fail";
        string advice = this.Advice.Length == 0 ? string.Empty : $" {this.Advice}";

        // A found value can end in the period of a quoted stderr, and the line adds its own.
        return $"{this.Tool}: {state}. Expected {this.Expected}, found {this.Found.TrimEnd('.')}.{advice}";
    }
}

/// <summary>The newest install of Visual Studio with the C++ tools, as vswhere gives it.</summary>
/// <param name="DisplayName">The name of the product, such as `Visual Studio Community 2026`.</param>
/// <param name="InstallationVersion">The full version of the install, such as `18.0.11205.157`.</param>
/// <param name="InstallationPath">The folder of the install.</param>
public sealed record VisualStudioInstance(string DisplayName, string InstallationVersion, string InstallationPath);

/// <summary>
/// The rules of the toolchain check (D-28, D-30, D-74). The facts go in, and one result for each
/// pin comes out. The rules do no I/O.
/// </summary>
public static class ToolchainRules
{
    /// <summary>The text before the version in the output of `git lfs version`.</summary>
    public const string GitLfsPrefix = "git-lfs/";

    /// <summary>The next step when the MSVC toolset of the pin is absent (D-74).</summary>
    public const string MsvcAdvice = "Add the component \"MSVC Build Tools v14.50 for x64/x86\" in the Visual Studio Installer, then update Visual Studio.";

    /// <summary>Applies each pin to the facts.</summary>
    /// <param name="facts">The facts of the Windows PC.</param>
    /// <returns>One result for Visual Studio, MSVC, the Windows SDK, the engine, and Git LFS, in that order.</returns>
    public static IReadOnlyList<PinResult> Evaluate(ToolchainFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return
        [
            CheckVisualStudio(facts.VisualStudio),
            CheckMsvc(facts.Msvc),
            CheckWindowsSdk(facts.WindowsSdk),
            CheckEngine(facts.Engine),
            CheckGitLfs(facts.GitLfs),
        ];
    }

    /// <summary>Reads the first install in the JSON output of vswhere.</summary>
    /// <param name="visualStudio">The output of vswhere.</param>
    /// <returns>The install.</returns>
    /// <exception cref="InvalidOperationException">
    /// vswhere gave no output, no install, or an install with no text in a field. The message
    /// names the command and the field (T-2).
    /// </exception>
    public static VisualStudioInstance ReadVisualStudio(ToolOutput visualStudio)
    {
        ArgumentNullException.ThrowIfNull(visualStudio);

        if (visualStudio.Text is null)
        {
            throw new InvalidOperationException(visualStudio.Absence ?? $"`{visualStudio.Source}` gave no text");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(visualStudio.Text);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"the output of `{visualStudio.Source}` is not a JSON array");
            }

            if (root.GetArrayLength() == 0)
            {
                throw new InvalidOperationException($"`{visualStudio.Source}` gave no install with the C++ tools");
            }

            JsonElement first = root[0];
            return new VisualStudioInstance(
                RequiredText(first, "displayName", visualStudio.Source),
                RequiredText(first, "installationVersion", visualStudio.Source),
                RequiredText(first, "installationPath", visualStudio.Source));
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException($"the output of `{visualStudio.Source}` is not JSON: {fault.Message}", fault);
        }
    }

    /// <summary>Visual Studio holds when the newest install with the C++ tools is Visual Studio 2026 (D-74).</summary>
    private static PinResult CheckVisualStudio(ToolOutput visualStudio)
    {
        const string tool = "Visual Studio";
        string expected = $"Visual Studio 2026, major version {ToolchainPins.VisualStudioMajor} (D-74)";
        VisualStudioInstance instance;
        try
        {
            instance = ReadVisualStudio(visualStudio);
        }
        catch (InvalidOperationException fault)
        {
            return new PinResult(tool, false, expected, fault.Message, "Install Visual Studio 2026 from docs/runbooks/engine-setup.md.");
        }

        string found = $"{instance.DisplayName} {instance.InstallationVersion}";
        if (!Version.TryParse(instance.InstallationVersion, out Version? version))
        {
            return new PinResult(tool, false, expected, $"the version '{instance.InstallationVersion}' from `{visualStudio.Source}`, with no version form", string.Empty);
        }

        return new PinResult(tool, version.Major == ToolchainPins.VisualStudioMajor, expected, found, string.Empty);
    }

    /// <summary>
    /// MSVC holds when one installed toolset has a cl.exe of the family 14.50 that the build tool
    /// does not ban (D-74). The default toolset can be of another family.
    /// </summary>
    private static PinResult CheckMsvc(FolderListing<MsvcToolset> msvc)
    {
        const string tool = "MSVC";
        string expected = $"an installed MSVC toolset with a cl.exe from {ToolchainPins.MsvcMinimum} to before {ToolchainPins.MsvcNextFamily} (D-74)";
        if (msvc.Items is null)
        {
            return new PinResult(tool, false, expected, $"{msvc.Absence}", MsvcAdvice);
        }

        string installed = $"installed toolsets: {string.Join(", ", msvc.Items.Select(toolset => toolset.Describe()))}";
        Version? best = msvc.Items
            .Select(toolset => toolset.Compiler)
            .OfType<Version>()
            .Where(compiler => compiler >= ToolchainPins.MsvcMinimum && compiler < ToolchainPins.MsvcNextFamily)
            .Max();
        if (best is null)
        {
            return new PinResult(tool, false, expected, installed, MsvcAdvice);
        }

        return new PinResult(tool, true, expected, $"cl.exe {best}, {installed}", string.Empty);
    }

    /// <summary>The Windows SDK holds when the newest SDK folder is the minimum of Epic or later (D-74).</summary>
    private static PinResult CheckWindowsSdk(FolderListing<string> windowsSdk)
    {
        const string tool = "Windows SDK";
        string expected = $"{ToolchainPins.WindowsSdkMinimum} or later (the minimum of Epic, D-74)";
        const string advice = "Add a Windows 11 SDK in the Visual Studio Installer.";
        if (windowsSdk.Items is null)
        {
            return new PinResult(tool, false, expected, $"{windowsSdk.Absence}", advice);
        }

        // Each SDK folder has a name of four numbers, such as `10.0.22621.0`. The folder can hold
        // other names, such as `wdf`, which name no SDK.
        List<Version> versions = [];
        foreach (string name in windowsSdk.Items)
        {
            if (Version.TryParse(name, out Version? version) && version.Revision >= 0)
            {
                versions.Add(version);
            }
        }

        if (versions.Count == 0)
        {
            return new PinResult(tool, false, expected, $"no version folder in '{windowsSdk.Source}'", advice);
        }

        versions.Sort();
        Version newest = versions[^1];
        bool holds = newest >= ToolchainPins.WindowsSdkMinimum;
        return new PinResult(tool, holds, expected, $"{newest}. Installed: {string.Join(", ", versions)}", holds ? string.Empty : advice);
    }

    /// <summary>The engine holds when the version file names the pinned hotfix (D-28).</summary>
    private static PinResult CheckEngine(ToolOutput engine)
    {
        const string tool = "Unreal Engine";
        string expected = $"{ToolchainPins.Engine} (D-28)";
        if (engine.Text is null)
        {
            return new PinResult(tool, false, expected, $"no engine: {engine.Absence}", string.Empty);
        }

        Version version;
        try
        {
            version = ReadEngineVersion(engine.Text, engine.Source);
        }
        catch (InvalidOperationException fault)
        {
            return new PinResult(tool, false, expected, fault.Message, string.Empty);
        }

        if (version == ToolchainPins.Engine)
        {
            return new PinResult(tool, true, expected, version.ToString(), string.Empty);
        }

        // The launcher installs each new hotfix by itself, and each hotfix upgrade is its own PR (D-28).
        return new PinResult(tool, false, expected, version.ToString(), "Each hotfix upgrade is its own PR with build evidence (D-28).");
    }

    /// <summary>Git LFS holds when `git lfs version` names a version. No decision pins the version (D-30).</summary>
    private static PinResult CheckGitLfs(ToolOutput gitLfs)
    {
        const string tool = "Git LFS";
        const string expected = "an install of any version (D-30)";
        if (gitLfs.Text is null)
        {
            return new PinResult(tool, false, expected, $"no Git LFS. `{gitLfs.Source}` failed: {gitLfs.Absence}", "Install Git for Windows with Git LFS, then run `git lfs install`.");
        }

        string line = gitLfs.Text.Trim();
        if (!line.StartsWith(GitLfsPrefix, StringComparison.Ordinal))
        {
            return new PinResult(tool, false, expected, $"the line '{line}' from `{gitLfs.Source}`, with no '{GitLfsPrefix}<version>' form", string.Empty);
        }

        int end = line.IndexOf(' ', StringComparison.Ordinal);
        string version = end < 0 ? line[GitLfsPrefix.Length..] : line[GitLfsPrefix.Length..end];
        return new PinResult(tool, true, expected, version, string.Empty);
    }

    /// <summary>Reads the three version numbers of the engine version file.</summary>
    /// <exception cref="InvalidOperationException">The file is not JSON, or it has no whole number in a field. The message names the file and the field.</exception>
    private static Version ReadEngineVersion(string text, string source)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            JsonElement root = document.RootElement;
            return new Version(
                RequiredNumber(root, "MajorVersion", source),
                RequiredNumber(root, "MinorVersion", source),
                RequiredNumber(root, "PatchVersion", source));
        }
        catch (JsonException fault)
        {
            throw new InvalidOperationException($"the file '{source}' is not JSON: {fault.Message}", fault);
        }
    }

    private static int RequiredNumber(JsonElement root, string field, string source)
    {
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty(field, out JsonElement value)
            || value.ValueKind != JsonValueKind.Number
            || !value.TryGetInt32(out int number))
        {
            throw new InvalidOperationException($"the file '{source}' has no whole number in the field '{field}'");
        }

        return number;
    }

    private static string RequiredText(JsonElement install, string field, string source)
    {
        if (install.ValueKind != JsonValueKind.Object
            || !install.TryGetProperty(field, out JsonElement value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrEmpty(value.GetString()))
        {
            throw new InvalidOperationException($"the output of `{source}` has no text in the field '{field}'");
        }

        return value.GetString()!;
    }
}
