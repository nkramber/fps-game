using System;
using System.IO;
using IronAbsolution.Tools.FrameCapture;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// The reader of the CSV file of the CSV profiler (D-137). Each file that does not have the form of
/// a finished capture fails with the path, the line, and the cause (T-2).
/// </summary>
public sealed class FrameTimeCsvTests
{
    private const string Source = "/checkout/Game/Saved/Logs/frame-capture.csv";

    [Fact]
    public void AFinishedCaptureGivesEachFrameTimeAndTheMetadata()
    {
        string text = FrameCaptureFixtures.Capture([4.25, 5.5, 3.0]);

        FrameTimeCsv capture = FrameTimeCsv.Read(Source, text);

        Assert.Equal([4.25, 5.5, 3.0], capture.FrameTimes);
        Assert.Equal("Development", capture.Metadata["config"]);
        Assert.Equal("WindowedFullscreen", capture.Metadata["IronAbsolution.WindowMode"]);
    }

    [Fact]
    public void TheCommandLineKeepsItsCommas()
    {
        FrameTimeCsv capture = FrameTimeCsv.Read(Source, FrameCaptureFixtures.Capture([4.0]));

        Assert.Equal("\" /Game/Maps/L_Gym -FrameTimeCsv=C:/x.csv,-unattended\"", capture.Metadata["Commandline"]);
    }

    [Fact]
    public void WindowsLineEndsRead()
    {
        string text = FrameCaptureFixtures.Capture([4.0, 6.0]).Replace("\n", "\r\n", StringComparison.Ordinal);

        Assert.Equal([4.0, 6.0], FrameTimeCsv.Read(Source, text).FrameTimes);
    }

    [Fact]
    public void AHeaderWithNoFrameTimeColumnFailsWithTheLine()
    {
        string text = FrameCaptureFixtures.Capture([4.0]).Replace("FrameTime", "FrameCost", StringComparison.Ordinal);

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has no column 'FrameTime' in the header on line 1", fault.Message);
    }

    [Fact]
    public void ARowWithAnotherNumberOfFieldsFailsWithTheLine()
    {
        string text = FrameCaptureFixtures.Capture([4.0, 5.0]).Replace(",5.0000,", ",5.0000,extra,", StringComparison.Ordinal);

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has 6 fields on line 3, and a row has from 5 to 5", fault.Message);
    }

    [Fact]
    public void AnEarlyRowWithFewerFieldsThanTheClosingHeaderReads()
    {
        // The profiler adds a column when a stat first appears, so the closing header holds more.
        string text = "EVENTS,FrameTime,GPUTime\n,4.0000,3.0000\n,5.0000,3.0000,0.5000\nEVENTS,FrameTime,GPUTime,NewStat\n[HasHeaderRowAtEnd],1\n";

        Assert.Equal([4.0, 5.0], FrameTimeCsv.Read(Source, text).FrameTimes);
    }

    [Fact]
    public void ARowWithFewerFieldsThanTheFirstHeaderFailsWithTheLine()
    {
        string text = "EVENTS,FrameTime,GPUTime\n,4.0000\nEVENTS,FrameTime,GPUTime,NewStat\n[HasHeaderRowAtEnd],1\n";

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has 2 fields on line 2, and a row has from 3 to 4", fault.Message);
    }

    [Fact]
    public void ARealCaptureOfTheEngineReads()
    {
        // Nine rows of a capture of the gym on 2026-09-29, from the package of Unreal Engine 5.8.3.
        // The rows have 288, 290, and 291 fields, and FrameTime is column 185 of 291. The order of
        // the columns changes from one capture to the next, so the reader finds the column by name.
        // The machine id of the key `loginid` holds zeros.
        string path = RepositoryRoot.PathTo("IronAbsolution.Tests/Fixtures/frame-capture-5.8.3.csv");

        FrameTimeCsv capture = FrameTimeCsv.Read(path, File.ReadAllText(path));

        Assert.Equal([6.3862, 1.8890, 2.1603, 3.3411, 3.1251, 3.6295, 3.6489, 3.1873, 3.1509], capture.FrameTimes);
        Assert.Equal("Development", capture.Metadata["config"]);
        Assert.Equal("WindowedFullscreen", capture.Metadata["IronAbsolution.WindowMode"]);
        Assert.Equal("2560", capture.Metadata["IronAbsolution.ViewportWidth"]);
        Assert.Equal("0", capture.Metadata["IronAbsolution.MaxFps"]);
        Assert.StartsWith("\" /Game/Maps/L_Gym -FrameTimeCsv=", capture.Metadata["Commandline"], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1.0000")]
    [InlineData("NaN")]
    [InlineData("")]
    public void AFrameTimeThatIsNotANumberOfZeroOrMoreFailsWithTheLine(string field)
    {
        string text = FrameCaptureFixtures.Capture([4.0, 7.0]).Replace(",7.0000,", $",{field},", StringComparison.Ordinal);

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has the frame time '{field}' on line 3, and a frame time is a number of milliseconds of 0 or more", fault.Message);
    }

    [Fact]
    public void AFileWithNoClosingHeaderAndNoMetadataFailsBecauseTheCaptureDidNotEnd()
    {
        // The profiler writes the header again and the metadata when it finishes the file.
        string text = $"{FrameCaptureFixtures.Header}\n,4.0000,2.0000,1.5000,3.0000\n";

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has no metadata on its last line, line 2. The capture did not end", fault.Message);
    }

    [Fact]
    public void MetadataWithNoClosingHeaderBeforeItFails()
    {
        string text = $"{FrameCaptureFixtures.Header}\n,4.0000,2.0000,1.5000,3.0000\n[HasHeaderRowAtEnd],1\n";

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has no header row before the metadata, on line 2. The capture did not end", fault.Message);
    }

    [Fact]
    public void AFileWithNoMetadataFailsBecauseTheCaptureDidNotEnd()
    {
        string header = FrameCaptureFixtures.Header;
        string text = $"{header}\n,4.0000,2.0000,1.5000,3.0000\n{header}\n";

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has no metadata on its last line, line 3. The capture did not end", fault.Message);
    }

    [Fact]
    public void MetadataWithNoKeyOfTheFinishedFileFails()
    {
        string text = FrameCaptureFixtures.Capture([4.0], [("config", "Development")]);

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has no metadata key '[HasHeaderRowAtEnd]' on line 4. The capture did not end", fault.Message);
    }

    [Fact]
    public void AMetadataKeyWithNoBracketsFailsWithTheLine()
    {
        string header = FrameCaptureFixtures.Header;
        string text = $"{header}\n,4.0000,2.0000,1.5000,3.0000\n{header}\n[HasHeaderRowAtEnd],1,config,Development\n";

        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, text));

        Assert.Equal($"the file '{Source}' has the metadata key 'config' on line 4, and a key has the form '[name]'", fault.Message);
    }

    [Fact]
    public void AnEmptyFileFailsWithNoHeader()
    {
        FormatException fault = Assert.Throws<FormatException>(() => FrameTimeCsv.Read(Source, string.Empty));

        Assert.Equal($"the file '{Source}' has no header on line 1", fault.Message);
    }
}
