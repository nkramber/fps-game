using System;
using System.IO;

namespace IronAbsolution.Tools;

/// <summary>
/// Joins a folder and a relative path that the tools hold with forward slashes (D-101).
/// A path constant such as `docs/session-handoff.md` reads the same on each platform, and a
/// document and a message can quote it. `Path.Combine` keeps the forward slash inside such a
/// constant, so a Windows path comes out with two separator forms, as in
/// `C:\dev\iron-absolution\docs/session-handoff.md`. A reader sees that form in an error
/// message, and a test that builds the same path with `Path.Combine` does not match it.
/// </summary>
public static class ToolPaths
{
    /// <summary>Joins a folder and a relative path, with the separator of this platform.</summary>
    /// <param name="folder">The folder that holds the relative path.</param>
    /// <param name="relativePath">The relative path, with a forward slash between the parts.</param>
    /// <returns>The joined path, with the separator of this platform alone.</returns>
    public static string UnderFolder(string folder, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(folder);
        ArgumentNullException.ThrowIfNull(relativePath);

        return Path.Combine(folder, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
