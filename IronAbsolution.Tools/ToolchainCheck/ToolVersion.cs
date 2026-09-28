using System;
using System.Globalization;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// A version of the form `major.minor` or `major.minor.patch`, such as the `26.1.1` of Xcode.
/// A version with no patch number has the patch 0, so `26.4` equals `26.4.0`.
/// </summary>
/// <param name="Major">The first number.</param>
/// <param name="Minor">The second number.</param>
/// <param name="Patch">The third number, or 0 when the text has two numbers.</param>
public sealed record ToolVersion(int Major, int Minor, int Patch) : IComparable<ToolVersion>
{
    /// <summary>Reads a version of two or three whole numbers.</summary>
    /// <param name="text">The text of the version, such as `26.1.1`.</param>
    /// <returns>The version, or null when the text has another form.</returns>
    public static ToolVersion? TryParse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        string[] parts = text.Trim().Split('.');
        if (parts.Length is < 2 or > 3)
        {
            return null;
        }

        int[] numbers = new int[3];
        for (int index = 0; index < parts.Length; index += 1)
        {
            if (!int.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out numbers[index]))
            {
                return null;
            }
        }

        return new ToolVersion(numbers[0], numbers[1], numbers[2]);
    }

    /// <summary>Compares the major, then the minor, then the patch number.</summary>
    /// <param name="other">The other version.</param>
    /// <returns>Less than 0 when this version is older, 0 when equal, more than 0 when newer.</returns>
    public int CompareTo(ToolVersion? other)
    {
        ArgumentNullException.ThrowIfNull(other);

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

    /// <summary>Gives the version as `major.minor.patch`.</summary>
    /// <returns>The text of the version.</returns>
    public override string ToString()
    {
        return $"{this.Major}.{this.Minor}.{this.Patch}";
    }
}
