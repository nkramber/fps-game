using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronAbsolution.Tools.CodexReview;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The sections of `.editorconfig` (D-40). A section with a plain file name, with no glob
/// character, holds the rules of the files of that name. Such a section stays after its file goes,
/// and it then holds the rules of a file that the repository does not have. The `Makefile` section
/// stayed after PR-14 removed the Makefile (D-97, D-99).
/// </summary>
public sealed class EditorConfigTests
{
    /// <summary>The characters of an editorconfig glob. A section name with none of them is a plain file name.</summary>
    private static readonly char[] GlobCharacters = ['*', '?', '[', ']', '{', '}', '!'];

    [Fact]
    public void EachSectionWithAPlainFileNameNamesATrackedFile()
    {
        List<string> trackedPaths = TrackedPaths();

        List<string> absent = PlainFileNameSections()
            .Where(name => !trackedPaths.Any(path => NamesFile(name, path)))
            .ToList();

        Assert.True(
            absent.Count == 0,
            $"`.editorconfig` has a section for a file that git does not track: {string.Join(", ", absent)}. Remove the section, or add the file.");
    }

    /// <summary>
    /// Gives the name of each section that has no glob character. The file must have at least one
    /// section, so an empty result does not come from a file that the test did not read.
    /// </summary>
    private static List<string> PlainFileNameSections()
    {
        string path = RepositoryRoot.PathTo(".editorconfig");
        List<string> sections = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => line.StartsWith('[') && line.EndsWith(']'))
            .Select(line => line[1..^1])
            .ToList();
        Assert.True(sections.Count > 0, $"'{path}' has no section.");

        return sections.Where(name => name.IndexOfAny(GlobCharacters) < 0).ToList();
    }

    /// <summary>Gives the path of each file that git tracks, with forward slashes, from the root of the checkout.</summary>
    private static List<string> TrackedPaths()
    {
        ProcessResult result = ExternalProcess.Run("git", ["ls-files"], RepositoryRoot.Find(), []);
        return result.RequireSuccess().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Tells whether a section name matches a tracked path. A name with a slash matches the path
    /// from the root. A name with no slash matches the file name in each folder.
    /// </summary>
    private static bool NamesFile(string sectionName, string trackedPath)
    {
        if (sectionName.Contains('/', StringComparison.Ordinal))
        {
            return trackedPath == sectionName.TrimStart('/');
        }

        return trackedPath.Split('/')[^1] == sectionName;
    }
}
