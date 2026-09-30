using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace IronAbsolution.Tests;

/// <summary>
/// Writes the text of a CSV file in the form of the CSV profiler of Unreal Engine 5.8.3: a header
/// row, one row for each frame, the header row again, and one line of metadata. The profiler writes
/// each metadata key in lower case, except the key of the finished file, and the command line comes
/// last with its commas (D-137).
/// </summary>
public static class FrameCaptureFixtures
{
    /// <summary>The header of the fixtures. The first column holds the events of each frame.</summary>
    public const string Header = "EVENTS,FrameTime,GameThreadTime,RenderThreadTime,GPUTime";

    /// <summary>Gives the metadata of a capture that holds each setting of D-137.</summary>
    /// <returns>Each key and value, in the order of the file, with no command line.</returns>
    public static List<(string Key, string Value)> GoodMetadata()
    {
        return
        [
            ("HasHeaderRowAtEnd", "1"),
            ("EventTimestamps", "0"),
            ("platform", "Windows"),
            ("config", "Development"),
            ("ironabsolution.windowmode", "WindowedFullscreen"),
            ("ironabsolution.viewportwidth", "2560"),
            ("ironabsolution.viewportheight", "1440"),
            ("ironabsolution.vsync", "0"),
            ("ironabsolution.maxfps", "0"),
            ("ironabsolution.viewcount", "4"),
        ];
    }

    /// <summary>Gives the text of a complete capture with the metadata of D-137.</summary>
    /// <param name="frameTimes">The time of each frame, in milliseconds.</param>
    /// <returns>The whole text of the file.</returns>
    public static string Capture(IEnumerable<double> frameTimes)
    {
        return Capture(frameTimes, GoodMetadata());
    }

    /// <summary>Gives the text of a complete capture.</summary>
    /// <param name="frameTimes">The time of each frame, in milliseconds.</param>
    /// <param name="metadata">Each metadata key and value, in order. The command line comes after them.</param>
    /// <returns>The whole text of the file.</returns>
    public static string Capture(IEnumerable<double> frameTimes, IEnumerable<(string Key, string Value)> metadata)
    {
        StringBuilder text = new StringBuilder();
        text.Append(Header).Append('\n');
        foreach (double frameTime in frameTimes)
        {
            string value = frameTime.ToString("F4", CultureInfo.InvariantCulture);
            text.Append(CultureInfo.InvariantCulture, $",{value},2.0000,1.5000,3.0000\n");
        }

        text.Append(Header).Append('\n');
        IEnumerable<string> pairs = metadata.Select(pair => $"[{pair.Key}],{pair.Value}");
        text.Append(string.Join(',', pairs));
        text.Append(",[commandline],\" /Game/Maps/L_Gym -FrameTimeCsv=C:/x.csv,-unattended\"\n");
        return text.ToString();
    }

    /// <summary>Gives a number of frames with the same time.</summary>
    /// <param name="count">The number of frames.</param>
    /// <param name="milliseconds">The time of each frame.</param>
    /// <returns>The frame times.</returns>
    public static IEnumerable<double> Frames(int count, double milliseconds)
    {
        return Enumerable.Repeat(milliseconds, count);
    }
}
