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
/// project file (F-6, D-88). Git reads each rule, so the tests see what git does, not a copy of a rule.
/// </summary>
public sealed class GameFilesTests
{
    private const string ProjectFile = "Game/IronAbsolution.uproject";
    private const string TestMap = "Game/Content/Maps/L_Test.umap";

    /// <summary>Each file type that Git LFS stores (D-86). A new rule needs a new line here.</summary>
    private static readonly string[] LfsTypes =
    [
        "uasset", "umap", "ubulk", "uexp", "uptnl",
        "fbx", "obj", "glb", "blend", "abc",
        "png", "jpg", "jpeg", "tga", "tif", "tiff", "exr", "hdr", "psd", "dds",
        "wav", "ogg", "mp3", "flac",
    ];

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
    public void EachTextFileOfTheProjectStaysOutOfLfs(string path)
    {
        IReadOnlyDictionary<string, string> attributes = CheckAttributes(path);

        Assert.Equal("unspecified", attributes["filter"]);
        Assert.Equal("auto", attributes["text"]);
    }

    [Fact]
    public void TheCommittedTestMapIsAnLfsPointer()
    {
        // The index holds the pointer that the LFS filter wrote. A map that bypassed LFS holds the binary itself.
        ProcessResult result = ExternalProcess.Run("git", ["cat-file", "-p", $":{TestMap}"], RepositoryRoot.Find(), []);

        string pointer = result.RequireSuccess();
        Assert.StartsWith("version https://git-lfs.github.com/spec/v1\n", pointer, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Game/Binaries/Mac/UnrealEditor-IronAbsolution.dylib")]
    [InlineData("Game/Intermediate/Build/Mac/UnrealEditor/Inc/IronAbsolution/UHT/Timestamp")]
    [InlineData("Game/Saved/Logs/IronAbsolution.log")]
    [InlineData("Game/Saved/Automation/editor-test/index.json")]
    [InlineData("Game/DerivedDataCache/Compressed.ddp")]
    [InlineData("Game/Plugins/Sample/Binaries/Mac/UnrealEditor-Sample.dylib")]
    [InlineData("Game/Plugins/Sample/Intermediate/Build/Mac/Sample.o")]
    [InlineData("Game/IronAbsolution (Mac).xcworkspace/contents.xcworkspacedata")]
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
    [InlineData(TestMap)]
    [InlineData("Game/Plugins/Sample/Sample.uplugin")]
    [InlineData("Game/Plugins/Sample/Content/Sample.uasset")]
    public void GitKeepsEachSourceFileOfTheProject(string path)
    {
        Assert.False(IsIgnored(path), $"'{path}' must not be ignored.");
    }

    [Fact]
    public void TheProjectFileTurnsOnEnhancedInputAndTheModelingToolsAndTurnsOffTheAndroidFileServer()
    {
        using JsonDocument project = JsonDocument.Parse(File.ReadAllText(RepositoryRoot.PathTo(ProjectFile)));
        JsonElement root = project.RootElement;

        List<(string Name, bool Enabled)> plugins = root.GetProperty("Plugins").EnumerateArray()
            .Select(plugin => (plugin.GetProperty("Name").GetString() ?? string.Empty, plugin.GetProperty("Enabled").GetBoolean()))
            .ToList();

        // The list is explicit, so a new plugin needs a new line here (T-1). The Android File Server
        // writes a security token into the config, and Android is not a target (D-32, D-88).
        Assert.Equal([("AndroidFileServer", false), ("EnhancedInput", true), ("ModelingToolsEditorMode", true)], plugins);
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
