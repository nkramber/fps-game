using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IronAbsolution.Tools.FrameCapture;

/// <summary>
/// The frame times and the metadata of one CSV file of the CSV profiler of the engine (D-137).
/// </summary>
/// <param name="FrameTimes">The time of each frame of the capture, in milliseconds, in the order of the frames.</param>
/// <param name="Metadata">Each metadata value, by its key. The profiler writes each key in lower case, so the keys match with no regard to case.</param>
public sealed record FrameTimeCsv(IReadOnlyList<double> FrameTimes, IReadOnlyDictionary<string, string> Metadata)
{
    /// <summary>The column of the frame time in the header of the file.</summary>
    public const string FrameTimeColumn = "FrameTime";

    /// <summary>
    /// The metadata key that the profiler writes when it finishes the file. A file with no such
    /// key comes from a capture that did not end.
    /// </summary>
    public const string CompleteFileKey = "HasHeaderRowAtEnd";

    /// <summary>The metadata key that the profiler writes last. Its value can hold commas.</summary>
    private const string CommandLineKey = "Commandline";

    /// <summary>
    /// Reads the text of a CSV file. The file has a header row, one row for each frame, the closing
    /// header row with each column of the capture, and one line of metadata pairs, such as
    /// `[config],Development`.
    /// </summary>
    /// <param name="source">The path of the file, for each error message.</param>
    /// <param name="text">The whole text of the file.</param>
    /// <returns>The frame times and the metadata.</returns>
    /// <exception cref="FormatException">The text does not have this form. The message names the file, the line, and the cause (T-2).</exception>
    public static FrameTimeCsv Read(string source, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(text);

        string[] lines = text.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        string firstHeader = lines[0];
        if (firstHeader.Length == 0)
        {
            throw new FormatException($"the file '{source}' has no header on line 1");
        }

        // The metadata is the last line of a finished file, and the closing header comes before it.
        int metadataIndex = lines.Length - 1;
        while (metadataIndex > 0 && lines[metadataIndex].Length == 0)
        {
            metadataIndex--;
        }

        if (metadataIndex < 2 || !lines[metadataIndex].StartsWith('['))
        {
            throw new FormatException($"the file '{source}' has no metadata on its last line, line {metadataIndex + 1}. The capture did not end");
        }

        // The profiler adds a column when a stat first appears during the capture, so an early row
        // has fewer fields. The closing header holds each column, and it starts with the first one.
        int closingIndex = metadataIndex - 1;
        string closingHeader = lines[closingIndex];
        if (closingHeader != firstHeader && !closingHeader.StartsWith(firstHeader + ",", StringComparison.Ordinal))
        {
            throw new FormatException($"the file '{source}' has no header row before the metadata, on line {closingIndex + 1}. The capture did not end");
        }

        string[] columns = closingHeader.Split(',');
        int firstColumnCount = firstHeader.Split(',').Length;
        int frameTimeIndex = Array.IndexOf(columns, FrameTimeColumn);
        if (frameTimeIndex < 0 || frameTimeIndex >= firstColumnCount)
        {
            throw new FormatException($"the file '{source}' has no column '{FrameTimeColumn}' in the header on line 1");
        }

        List<double> frameTimes = new List<double>();
        for (int index = 1; index < closingIndex; index++)
        {
            frameTimes.Add(ReadFrameTime(source, index + 1, lines[index], firstColumnCount, columns.Length, frameTimeIndex));
        }

        Dictionary<string, string> metadata = ReadMetadata(source, metadataIndex + 1, lines[metadataIndex]);
        if (!metadata.ContainsKey(CompleteFileKey))
        {
            throw new FormatException($"the file '{source}' has no metadata key '[{CompleteFileKey}]' on line {metadataIndex + 1}. The capture did not end");
        }

        return new FrameTimeCsv(frameTimes, metadata);
    }

    private static double ReadFrameTime(string source, int lineNumber, string line, int firstColumnCount, int columnCount, int frameTimeIndex)
    {
        string[] fields = line.Split(',');
        if (fields.Length < firstColumnCount || fields.Length > columnCount)
        {
            throw new FormatException($"the file '{source}' has {fields.Length} fields on line {lineNumber}, and a row has from {firstColumnCount} to {columnCount}");
        }

        string field = fields[frameTimeIndex];
        if (!double.TryParse(field, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) || !double.IsFinite(value) || value < 0.0)
        {
            throw new FormatException($"the file '{source}' has the frame time '{field}' on line {lineNumber}, and a frame time is a number of milliseconds of 0 or more");
        }

        return value;
    }

    private static Dictionary<string, string> ReadMetadata(string source, int lineNumber, string line)
    {
        Dictionary<string, string> metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] fields = line.Split(',');
        for (int index = 0; index < fields.Length; index += 2)
        {
            string key = fields[index];
            if (key.Length < 3 || key[0] != '[' || key[^1] != ']')
            {
                throw new FormatException($"the file '{source}' has the metadata key '{key}' on line {lineNumber}, and a key has the form '[name]'");
            }

            string name = key[1..^1];
            if (name.Equals(CommandLineKey, StringComparison.OrdinalIgnoreCase))
            {
                // The profiler writes the command line last and does not escape its commas.
                metadata[name] = string.Join(',', fields[(index + 1)..]);
                break;
            }

            if (index + 1 >= fields.Length)
            {
                throw new FormatException($"the file '{source}' has the metadata key '{key}' with no value on line {lineNumber}");
            }

            metadata[name] = fields[index + 1];
        }

        return metadata;
    }
}
