using System;
using System.Globalization;

namespace IronAbsolution.Tools.CodexReview;

/// <summary>
/// The version that `codex --version` prints, such as `codex-cli 0.156.1` or
/// `codex-cli 0.155.0-alpha.9.2`. A prerelease of a version is older than the version itself,
/// as in semantic versioning, so a prerelease of the minimum does not pass (D-14).
/// </summary>
/// <param name="Major">The first number.</param>
/// <param name="Minor">The second number.</param>
/// <param name="Patch">The third number.</param>
/// <param name="Prerelease">The text after the first hyphen, or the empty text for a release.</param>
public sealed record CodexVersion(int Major, int Minor, int Patch, string Prerelease)
{
    /// <summary>The text before the version on the first line of the output.</summary>
    public const string OutputPrefix = "codex-cli ";

    /// <summary>Reads the first line of the output of `codex --version`.</summary>
    /// <param name="versionOutput">The stdout of `codex --version`.</param>
    /// <returns>The version.</returns>
    /// <exception cref="FormatException">The line has another form. The message holds the text (T-2).</exception>
    public static CodexVersion Parse(string versionOutput)
    {
        ArgumentNullException.ThrowIfNull(versionOutput);

        string line = versionOutput.Split('\n')[0].Trim();
        if (!line.StartsWith(OutputPrefix, StringComparison.Ordinal))
        {
            throw new FormatException($"The Codex version output '{line}' does not start with '{OutputPrefix}'.");
        }

        string version = line[OutputPrefix.Length..].Trim();
        int dash = version.IndexOf('-', StringComparison.Ordinal);
        string core = dash < 0 ? version : version[..dash];
        string prerelease = dash < 0 ? string.Empty : version[(dash + 1)..];
        string[] parts = core.Split('.');
        if (parts.Length != 3)
        {
            throw new FormatException($"The Codex version '{version}' does not have the form <major>.<minor>.<patch>.");
        }

        return new CodexVersion(ParsePart(parts[0], version), ParsePart(parts[1], version), ParsePart(parts[2], version), prerelease);
    }

    /// <summary>Tells whether this version is the minimum or newer. The method does not compare the prerelease text.</summary>
    /// <param name="minimum">The oldest version that passes.</param>
    /// <returns>True when this version passes.</returns>
    public bool IsAtLeast(CodexVersion minimum)
    {
        ArgumentNullException.ThrowIfNull(minimum);

        int byNumber = this.CompareNumbers(minimum);
        if (byNumber != 0)
        {
            return byNumber > 0;
        }

        bool thisIsRelease = this.Prerelease.Length == 0;
        bool minimumIsRelease = minimum.Prerelease.Length == 0;
        return thisIsRelease || !minimumIsRelease;
    }

    /// <summary>Gives the version as `major.minor.patch`, with `-prerelease` when it has one.</summary>
    /// <returns>The text of the version.</returns>
    public override string ToString()
    {
        string core = $"{this.Major}.{this.Minor}.{this.Patch}";
        return this.Prerelease.Length == 0 ? core : $"{core}-{this.Prerelease}";
    }

    private int CompareNumbers(CodexVersion other)
    {
        if (this.Major != other.Major)
        {
            return this.Major.CompareTo(other.Major);
        }

        if (this.Minor != other.Minor)
        {
            return this.Minor.CompareTo(other.Minor);
        }

        return this.Patch.CompareTo(other.Patch);
    }

    private static int ParsePart(string part, string version)
    {
        if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
        {
            throw new FormatException($"The part '{part}' of the Codex version '{version}' is not a whole number.");
        }

        return value;
    }
}
