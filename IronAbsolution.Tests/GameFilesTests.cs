using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The text files of the Unreal project that the hosted runners can read (D-31): the Git LFS
/// rules (D-30, D-86), the ignore rules of the Unreal folders (D-9), and the plugin list of the
/// project file (F-6, D-88, D-134), and the map list of the packaging settings (D-89). Git reads each rule,
/// so the tests see what git does, not a copy of a rule.
/// </summary>
public sealed class GameFilesTests
{
    private const string ProjectFile = "Game/IronAbsolution.uproject";
    private const string TestMap = "Game/Content/Maps/L_Test.umap";
    private const string GymMap = "Game/Content/Maps/L_Gym.umap";
    private const string ContentScript = "Game/Scripts/build_content.py";

    /// <summary>Each file type that Git LFS stores (D-86). A new rule needs a new line here.</summary>
    private static readonly string[] LfsTypes =
    [
        "uasset", "umap", "ubulk", "uexp", "uptnl",
        "fbx", "obj", "glb", "blend", "abc",
        "png", "jpg", "jpeg", "tga", "tif", "tiff", "exr", "hdr", "psd", "dds",
        "wav", "ogg", "mp3", "flac",
    ];

    /// <summary>The tokens of a setting of another platform, with their case. A Windows setting holds none of them.</summary>
    private static readonly string[] OtherPlatformTokens = ["Xcode", "BundleIdentifier", "SF_METAL", "SF_VULKAN"];

    public static TheoryData<string> LfsTypeData => new TheoryData<string>(LfsTypes);

    [Theory]
    [MemberData(nameof(LfsTypeData))]
    public void GitLfsStoresEachBinaryType(string type)
    {
        string path = $"Game/Content/Sample.{type}";

        IReadOnlyDictionary<string, string> attributes = CheckAttributes(path);

        Assert.Equal("lfs", attributes["filter"]);
        Assert.Equal("lfs", attributes["diff"]);
        Assert.Equal("lfs", attributes["merge"]);
        Assert.Equal("unset", attributes["text"]);
    }

    [Fact]
    public void TheLfsRulesNameExactlyTheListedTypes()
    {
        // A rule with no line in LfsTypes has no test, and a line with no rule fails above.
        List<string> ruleTypes = File.ReadAllLines(RepositoryRoot.PathTo(".gitattributes"))
            .Where(line => line.Contains("filter=lfs", StringComparison.Ordinal))
            .Select(line => line.Split(' ')[0])
            .ToList();

        Assert.Equal(LfsTypes.Select(type => $"*.{type}").Order(StringComparer.Ordinal), ruleTypes.Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(ProjectFile)]
    [InlineData("Game/Config/DefaultEngine.ini")]
    [InlineData("Game/Source/IronAbsolution/IronAbsolution.Build.cs")]
    [InlineData("Game/Source/IronAbsolution/IronAbsolution.cpp")]
    [InlineData("Game/Source/IronAbsolution/IronAbsolution.h")]
    [InlineData(ContentScript)]
    public void EachTextFileOfTheProjectStaysOutOfLfs(string path)
    {
        IReadOnlyDictionary<string, string> attributes = CheckAttributes(path);

        Assert.Equal("unspecified", attributes["filter"]);
        Assert.Equal("auto", attributes["text"]);
    }

    [Theory]
    [InlineData(TestMap)]
    [InlineData(GymMap)]
    [InlineData("Game/Content/Input/IMC_KeyboardMouse.uasset")]
    [InlineData("Game/Content/Player/BP_PlayerCharacter.uasset")]
    [InlineData("Game/Content/Player/DA_PlayerMovement.uasset")]
    public void EachCommittedBinaryOfTheProjectIsAnLfsPointer(string path)
    {
        // The index holds the pointer that the LFS filter wrote. A file that bypassed LFS holds the binary itself.
        ProcessResult result = ExternalProcess.Run("git", ["cat-file", "-p", $":{path}"], RepositoryRoot.Find(), []);

        string pointer = result.RequireSuccess();
        Assert.StartsWith("version https://git-lfs.github.com/spec/v1\n", pointer, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Game/Binaries/Win64/UnrealEditor-IronAbsolution.dll")]
    [InlineData("Game/Intermediate/Build/Win64/UnrealEditor/Inc/IronAbsolution/UHT/Timestamp")]
    [InlineData("Game/Saved/Logs/IronAbsolution.log")]
    [InlineData("Game/Saved/Automation/editor-test/index.json")]
    [InlineData("Game/Saved/Packages/Windows/IronAbsolution/Binaries/Win64/IronAbsolution.exe")]
    [InlineData("Game/Saved/StagedBuilds/Windows/IronAbsolution/Binaries/Win64/IronAbsolution.exe")]
    [InlineData("Game/Saved/Cooked/Windows/IronAbsolution/Content/Maps/L_Test.umap")]
    [InlineData("Game/Build/Windows/FileOpenOrder/CookerOpenOrder.log")]
    [InlineData("Game/DerivedDataCache/Compressed.ddp")]
    [InlineData("Game/Plugins/Sample/Binaries/Win64/UnrealEditor-Sample.dll")]
    [InlineData("Game/Plugins/Sample/Intermediate/Build/Win64/Sample.obj")]
    [InlineData("Game/IronAbsolution.sln")]
    public void GitIgnoresEachGeneratedFolderOfUnreal(string path)
    {
        Assert.True(IsIgnored(path), $"'{path}' must be ignored (D-9).");
    }

    [Theory]
    [InlineData(ProjectFile)]
    [InlineData("Game/Config/DefaultEngine.ini")]
    [InlineData("Game/Source/IronAbsolution/IronAbsolution.Build.cs")]
    [InlineData("Game/Source/IronAbsolution/Private/Tests/ProjectSettingsTest.cpp")]
    [InlineData("Game/Source/IronAbsolution/Public/TimedRunSubsystem.h")]
    [InlineData("Game/Source/IronAbsolution/Private/TimedRunSubsystem.cpp")]
    [InlineData("Game/Config/DefaultGame.ini")]
    [InlineData(TestMap)]
    [InlineData(GymMap)]
    [InlineData(ContentScript)]
    [InlineData("Game/Plugins/Sample/Sample.uplugin")]
    [InlineData("Game/Plugins/Sample/Content/Sample.uasset")]
    public void GitKeepsEachSourceFileOfTheProject(string path)
    {
        Assert.False(IsIgnored(path), $"'{path}' must not be ignored.");
    }

    [Fact]
    public void TheProjectFileTurnsOnEnhancedInputTheModelingToolsAndPythonAndTurnsOffTheAndroidFileServer()
    {
        using JsonDocument project = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(ProjectFile)));
        JsonElement root = project.RootElement;

        List<(string Name, bool Enabled)> plugins = root.GetProperty("Plugins").EnumerateArray()
            .Select(plugin => (plugin.GetProperty("Name").GetString() ?? string.Empty, plugin.GetProperty("Enabled").GetBoolean()))
            .ToList();

        // The list is explicit, so a new plugin needs a new line here (T-1). The Android File Server
        // writes a security token into the config, and Android is not a target (D-32, D-88). The
        // content script runs in the editor alone through the Python plugin (D-134).
        Assert.Equal([("AndroidFileServer", false), ("EnhancedInput", true), ("ModelingToolsEditorMode", true), ("PythonScriptPlugin", true)], plugins);
    }

    [Theory]
    [InlineData("ModelingToolsEditorMode")]
    [InlineData("PythonScriptPlugin")]
    public void AnEditorPluginLoadsInTheEditorTargetAlone(string name)
    {
        // Python never runs in a package (D-134), so the game target must not load the plugin.
        using JsonDocument project = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(ProjectFile)));
        JsonElement plugin = project.RootElement.GetProperty("Plugins").EnumerateArray()
            .Single(entry => entry.GetProperty("Name").GetString() == name);

        List<string?> targets = plugin.GetProperty("TargetAllowList").EnumerateArray().Select(target => target.GetString()).ToList();

        Assert.Equal(["Editor"], targets);
    }

    [Fact]
    public void TheProjectFileHasOneRuntimeModuleAndThePinnedEngine()
    {
        using JsonDocument project = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(ProjectFile)));
        JsonElement root = project.RootElement;

        Assert.Equal("5.8", root.GetProperty("EngineAssociation").GetString());
        JsonElement module = Assert.Single(root.GetProperty("Modules").EnumerateArray());
        Assert.Equal("IronAbsolution", module.GetProperty("Name").GetString());
        Assert.Equal("Runtime", module.GetProperty("Type").GetString());
    }

    [Fact]
    public void ThePackagingSettingsCookTheTestMapAndTheGym()
    {
        // The package loads the test map for the timed run of PR-10 (D-89), and the gym is the
        // default map (PR-21). The list is explicit, so a new map needs a new line here (T-1).
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo("Game/Config/DefaultGame.ini"));
        int section = Array.IndexOf(lines, "[/Script/UnrealEd.ProjectPackagingSettings]");
        Assert.True(section >= 0, "DefaultGame.ini has no section of the packaging settings.");

        List<string> maps = lines
            .Skip(section + 1)
            .TakeWhile(line => !line.StartsWith('['))
            .Where(line => line.Contains("MapsToCook", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(["+MapsToCook=(FilePath=\"/Game/Maps/L_Test\")", "+MapsToCook=(FilePath=\"/Game/Maps/L_Gym\")"], maps);
    }

    [Theory]
    [InlineData("Game/Config/DefaultEngine.ini")]
    [InlineData("Game/Config/DefaultGame.ini")]
    [InlineData("Game/Config/DefaultInput.ini")]
    public void EachConfigFileHoldsTheSettingsOfWindowsAlone(string path)
    {
        // The project supports Windows alone (D-91). Exit test 6 of PR-14: no Mac section, no Linux
        // section, and no setting of Metal, Xcode, or Vulkan (D-97, D-105). The editor writes a
        // section for each platform when a platform page of Project Settings opens.
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo(path));

        List<string> platformSections = lines
            .Where(line => line.StartsWith("[/Script/", StringComparison.Ordinal) && line.Contains("TargetPlatform.", StringComparison.Ordinal))
            .Where(line => !line.StartsWith("[/Script/WindowsTargetPlatform.", StringComparison.Ordinal))
            .ToList();
        List<string> otherPlatformLines = lines
            .Where(line => OtherPlatformTokens.Any(token => line.Contains(token, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(platformSections);
        Assert.Empty(otherPlatformLines);
    }

    /// <summary>Gives the value of each LFS attribute and of `text` for one path, as git reads them.</summary>
    private static IReadOnlyDictionary<string, string> CheckAttributes(string path)
    {
        ProcessResult result = ExternalProcess.Run("git", ["check-attr", "filter", "diff", "merge", "text", "--", path], RepositoryRoot.Find(), []);
        var attributes = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string line in result.RequireSuccess().Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            // Each line has the form `<path>: <attribute>: <value>`.
            string[] parts = line[(path.Length + 2)..].Split(": ");
            attributes[parts[0]] = parts[1];
        }

        return attributes;
    }

    /// <summary>Tells whether git ignores a path, whether or not the path exists or git tracks it.</summary>
    private static bool IsIgnored(string path)
    {
        ProcessResult result = ExternalProcess.Run("git", ["check-ignore", "--no-index", "--quiet", "--", path], RepositoryRoot.Find(), []);
        return result.ExitCode switch
        {
            0 => true,
            1 => false,
            _ => throw new InvalidOperationException($"`{result.Command}` gave the exit code {result.ExitCode}: {result.StandardError.Trim()}"),
        };
    }
}
