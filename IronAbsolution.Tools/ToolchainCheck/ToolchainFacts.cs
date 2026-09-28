using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IronAbsolution.Tools.CodexReview;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The text that one source of the toolchain gave, or the reason that it gave none. Exactly one
/// of <see cref="Text"/> and <see cref="Absence"/> is set.
/// </summary>
/// <param name="Source">The command or the file that the text came from, for each message (T-2).</param>
/// <param name="Text">The text of the source, or null when the source is absent.</param>
/// <param name="Absence">The reason that the source gave no text, or null when it gave text.</param>
public sealed record ToolOutput(string Source, string? Text, string? Absence)
{
    /// <summary>Makes the output of a source that gave text.</summary>
    /// <param name="source">The command or the file.</param>
    /// <param name="text">The text that it gave.</param>
    /// <returns>The output.</returns>
    public static ToolOutput Found(string source, string text)
    {
        return new ToolOutput(source, text, null);
    }

    /// <summary>Makes the output of a source that gave no text.</summary>
    /// <param name="source">The command or the file.</param>
    /// <param name="absence">The reason, with its context (T-2).</param>
    /// <returns>The output.</returns>
    public static ToolOutput Absent(string source, string absence)
    {
        return new ToolOutput(source, null, absence);
    }
}

/// <summary>
/// The items that one folder gave, or the reason that it gave none. Exactly one of
/// <see cref="Items"/> and <see cref="Absence"/> is set.
/// </summary>
/// <typeparam name="T">The type of one item.</typeparam>
/// <param name="Source">The folder that the items came from, for each message (T-2).</param>
/// <param name="Items">The items of the folder, or null when the folder is absent.</param>
/// <param name="Absence">The reason that the folder gave no items, or null when it gave items.</param>
public sealed record FolderListing<T>(string Source, IReadOnlyList<T>? Items, string? Absence)
{
    /// <summary>Makes the listing of a folder that gave items.</summary>
    /// <param name="source">The folder.</param>
    /// <param name="items">The items that it gave.</param>
    /// <returns>The listing.</returns>
    public static FolderListing<T> Found(string source, IReadOnlyList<T> items)
    {
        return new FolderListing<T>(source, items, null);
    }

    /// <summary>Makes the listing of a folder that gave no items.</summary>
    /// <param name="source">The folder.</param>
    /// <param name="absence">The reason, with its context (T-2).</param>
    /// <returns>The listing.</returns>
    public static FolderListing<T> Absent(string source, string absence)
    {
        return new FolderListing<T>(source, null, absence);
    }
}

/// <summary>One MSVC toolset: the name of its folder, and the product version of its x64 cl.exe.</summary>
/// <param name="Folder">The name of the toolset folder, such as `14.50.35717`.</param>
/// <param name="Compiler">The product version of cl.exe, or null when the folder has no x64 cl.exe.</param>
public sealed record MsvcToolset(string Folder, Version? Compiler)
{
    /// <summary>Gives the toolset for a message, such as `14.50.35717 (cl.exe 14.50.35725)`.</summary>
    /// <returns>The text of the toolset.</returns>
    public string Describe()
    {
        return this.Compiler is null ? $"{this.Folder} (no x64 cl.exe)" : $"{this.Folder} (cl.exe {this.Compiler})";
    }
}

/// <summary>
/// Every fact that the rules of the toolchain check need, read one time from the Windows PC. The
/// rules do no I/O.
/// </summary>
/// <param name="VisualStudio">The JSON output of vswhere for the newest install with the C++ tools.</param>
/// <param name="Msvc">Each MSVC toolset of that install.</param>
/// <param name="WindowsSdk">The name of each folder under the headers of the Windows SDK.</param>
/// <param name="Engine">The text of the version file of the engine.</param>
/// <param name="GitLfs">The output of `git lfs version`.</param>
public sealed record ToolchainFacts(
    ToolOutput VisualStudio,
    FolderListing<MsvcToolset> Msvc,
    FolderListing<string> WindowsSdk,
    ToolOutput Engine,
    ToolOutput GitLfs)
{
    /// <summary>Runs vswhere and Git LFS, and reads the toolset folders, the SDK folders, and the version file of the engine.</summary>
    /// <param name="engineFolder">The value of the engine variable, or null when it is not set (D-79).</param>
    /// <param name="programFilesX86">The value of `ProgramFiles(x86)`, or null when it is not set.</param>
    /// <param name="git">The `git` program, as a name on the command path or a full path.</param>
    /// <param name="workingDirectory">The folder in which the two commands run.</param>
    /// <returns>The facts. A source that is absent or failed gives an absent value, not an exception.</returns>
    public static ToolchainFacts Gather(string? engineFolder, string? programFilesX86, string git, string workingDirectory)
    {
        ToolOutput visualStudio = RunVswhere(programFilesX86, workingDirectory);
        return new ToolchainFacts(
            visualStudio,
            ReadMsvcToolsets(visualStudio),
            ReadWindowsSdkFolders(programFilesX86),
            ReadBuildVersion(engineFolder),
            RunTool(git, ["lfs", "version"], workingDirectory));
    }

    private static ToolOutput RunVswhere(string? programFilesX86, string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(programFilesX86))
        {
            return ToolOutput.Absent($"${ToolchainPins.ProgramFilesX86Variable}", NoProgramFilesX86());
        }

        string vswhere = ToolPaths.UnderFolder(programFilesX86, ToolchainPins.VswherePath);
        if (!File.Exists(vswhere))
        {
            return ToolOutput.Absent(vswhere, $"no file '{vswhere}'");
        }

        return RunTool(vswhere, ["-latest", "-products", "*", "-requires", ToolchainPins.CppToolsComponent, "-format", "json"], workingDirectory);
    }

    private static ToolOutput RunTool(string fileName, IReadOnlyList<string> args, string workingDirectory)
    {
        string command = $"{fileName} {string.Join(' ', args)}";
        ProcessResult result;
        try
        {
            result = ExternalProcess.Run(fileName, args, workingDirectory, []);
        }
        catch (InvalidOperationException fault)
        {
            // An absent program is a failed pin with its reason, not a crash of the check (T-2).
            return ToolOutput.Absent(command, fault.Message);
        }

        if (result.ExitCode != 0)
        {
            return ToolOutput.Absent(command, $"the exit code {result.ExitCode}, stderr: {result.StandardError.Trim()}");
        }

        return ToolOutput.Found(command, result.StandardOutput);
    }

    /// <summary>
    /// Reads each toolset folder of the install that vswhere gave. The version comes from cl.exe,
    /// not from the folder name, as UnrealBuildTool reads it (<see cref="ToolchainPins.MsvcMinimum"/>).
    /// </summary>
    private static FolderListing<MsvcToolset> ReadMsvcToolsets(ToolOutput visualStudio)
    {
        VisualStudioInstance instance;
        try
        {
            instance = ToolchainRules.ReadVisualStudio(visualStudio);
        }
        catch (InvalidOperationException fault)
        {
            return FolderListing<MsvcToolset>.Absent(ToolchainPins.MsvcFolder, $"no Visual Studio with the C++ tools: {fault.Message}");
        }

        string folder = ToolPaths.UnderFolder(instance.InstallationPath, ToolchainPins.MsvcFolder);
        if (!Directory.Exists(folder))
        {
            return FolderListing<MsvcToolset>.Absent(folder, $"no folder '{folder}'");
        }

        try
        {
            List<MsvcToolset> toolsets = Directory.GetDirectories(folder)
                .Order(StringComparer.Ordinal)
                .Select(ReadToolset)
                .ToList();
            if (toolsets.Count == 0)
            {
                return FolderListing<MsvcToolset>.Absent(folder, $"no toolset in '{folder}'");
            }

            return FolderListing<MsvcToolset>.Found(folder, toolsets);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            return FolderListing<MsvcToolset>.Absent(folder, $"the folder '{folder}' did not read: {fault.Message}");
        }
    }

    private static MsvcToolset ReadToolset(string toolsetFolder)
    {
        string name = Path.GetFileName(toolsetFolder);
        string compiler = ToolPaths.UnderFolder(toolsetFolder, ToolchainPins.CompilerPath);
        if (!File.Exists(compiler))
        {
            return new MsvcToolset(name, null);
        }

        FileVersionInfo info = FileVersionInfo.GetVersionInfo(compiler);
        return new MsvcToolset(name, new Version(info.ProductMajorPart, info.ProductMinorPart, info.ProductBuildPart));
    }

    private static FolderListing<string> ReadWindowsSdkFolders(string? programFilesX86)
    {
        if (string.IsNullOrWhiteSpace(programFilesX86))
        {
            return FolderListing<string>.Absent($"${ToolchainPins.ProgramFilesX86Variable}", NoProgramFilesX86());
        }

        string folder = ToolPaths.UnderFolder(programFilesX86, ToolchainPins.WindowsSdkFolder);
        if (!Directory.Exists(folder))
        {
            return FolderListing<string>.Absent(folder, $"no folder '{folder}'");
        }

        try
        {
            List<string> names = Directory.GetDirectories(folder).Select(Path.GetFileName).OfType<string>().ToList();
            return FolderListing<string>.Found(folder, names);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            return FolderListing<string>.Absent(folder, $"the folder '{folder}' did not read: {fault.Message}");
        }
    }

    private static ToolOutput ReadBuildVersion(string? engineFolder)
    {
        if (string.IsNullOrWhiteSpace(engineFolder))
        {
            return ToolOutput.Absent($"${ToolchainPins.EngineVariable}", $"the variable {ToolchainPins.EngineVariable} is not set (D-79)");
        }

        string path = ToolPaths.UnderFolder(engineFolder, ToolchainPins.BuildVersionPath);
        if (!File.Exists(path))
        {
            return ToolOutput.Absent(path, $"no file '{path}'. {ToolchainPins.EngineVariable} names the folder that holds 'Engine'");
        }

        try
        {
            return ToolOutput.Found(path, File.ReadAllText(path));
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            return ToolOutput.Absent(path, $"the file '{path}' did not read: {fault.Message}");
        }
    }

    private static string NoProgramFilesX86()
    {
        return $"the variable {ToolchainPins.ProgramFilesX86Variable} is not set, so this is not a Windows PC";
    }
}
