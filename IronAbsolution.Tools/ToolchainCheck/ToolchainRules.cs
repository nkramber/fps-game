using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>The result of one pin: the tool, the expected value, the found value, and the next step on a failure.</summary>
/// <param name="Tool">The name of the tool, such as `Xcode`.</param>
/// <param name="Holds">True when the found value meets the pin.</param>
/// <param name="Expected">The pin, with the decision that sets it.</param>
/// <param name="Found">The value on this machine, or the reason that no value exists (T-2).</param>
/// <param name="Advice">The next step on a failure, or the empty text.</param>
public sealed record PinResult(string Tool, bool Holds, string Expected, string Found, string Advice)
{
    /// <summary>Gives the one report line of the pin.</summary>
    /// <returns>The line, such as `Xcode: pass. Expected 26.1.1 (D-28), found 26.1.1.`</returns>
    public string Line()
    {
        string state = this.Holds ? "pass" : "fail";
        string advice = this.Advice.Length == 0 ? string.Empty : $" {this.Advice}";

        // A found value can end in the period of a quoted stderr, and the line adds its own.
        return $"{this.Tool}: {state}. Expected {this.Expected}, found {this.Found.TrimEnd('.')}.{advice}";
    }
}

/// <summary>
/// The rules of the toolchain check (D-28, D-30). The facts go in, and one result for each pin
/// comes out. The rules do no I/O.
/// </summary>
public static class ToolchainRules
{
    /// <summary>The text before the version on the first line of `xcodebuild -version`.</summary>
    public const string XcodePrefix = "Xcode ";

    /// <summary>The text before the version in the output of `git lfs version`.</summary>
    public const string GitLfsPrefix = "git-lfs/";

    /// <summary>Applies each pin to the facts.</summary>
    /// <param name="facts">The facts of the Mac.</param>
    /// <returns>One result for Xcode, one for the engine, and one for Git LFS, in that order.</returns>
    public static IReadOnlyList<PinResult> Evaluate(ToolchainFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        return [CheckXcode(facts.Xcode), CheckEngine(facts.Engine), CheckGitLfs(facts.GitLfs)];
    }

    /// <summary>Xcode holds when `xcodebuild -version` names the one pinned version (D-28).</summary>
    private static PinResult CheckXcode(ToolOutput xcode)
    {
        const string tool = "Xcode";
        string expected = $"{ToolchainPins.Xcode} (D-28)";
        if (xcode.Text is null)
        {
            return new PinResult(
                tool,
                false,
                expected,
                $"no Xcode app. `{xcode.Source}` failed: {xcode.Absence}",
                "Select the Xcode app with `sudo xcode-select -s <path of Xcode-26.1.1.app>`.");
        }

        string firstLine = xcode.Text.Split('\n')[0].Trim();
        ToolVersion? version = firstLine.StartsWith(XcodePrefix, StringComparison.Ordinal)
            ? ToolVersion.TryParse(firstLine[XcodePrefix.Length..])
            : null;
        if (version is null)
        {
            return new PinResult(tool, false, expected, $"the line '{firstLine}' from `{xcode.Source}`, with no '{XcodePrefix}<version>' form", string.Empty);
        }

        if (version == ToolchainPins.Xcode)
        {
            return new PinResult(tool, true, expected, version.ToString(), string.Empty);
        }

        return new PinResult(tool, false, expected, version.ToString(), XcodeReason(version));
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

        ToolVersion version;
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
            return new PinResult(tool, false, expected, $"no Git LFS. `{gitLfs.Source}` failed: {gitLfs.Absence}", "Install it with `brew install git-lfs`, then run `git lfs install`.");
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

    private static string XcodeReason(ToolVersion version)
    {
        if (version.CompareTo(ToolchainPins.XcodeFirstRefused) >= 0)
        {
            return $"Xcode {ToolchainPins.XcodeFirstRefused} and later do not work with Unreal Engine 5.8 (F-3).";
        }

        if (version.CompareTo(ToolchainPins.XcodeMinimum) < 0)
        {
            return $"Unreal Engine 5.8 needs Xcode {ToolchainPins.XcodeMinimum} or later (F-3).";
        }

        return $"D-28 pins one Xcode version, {ToolchainPins.Xcode}.";
    }

    /// <summary>Reads the three version numbers of the engine version file.</summary>
    /// <exception cref="InvalidOperationException">The file is not JSON, or it has no whole number in a field. The message names the file and the field.</exception>
    private static ToolVersion ReadEngineVersion(string text, string source)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            JsonElement root = document.RootElement;
            return new ToolVersion(
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
}
