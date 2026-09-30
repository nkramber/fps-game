using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace IronAbsolution.TestStub;

/// <summary>
/// Stands in for a program of the engine in a test (D-103). A test writes the plan file beside
/// this program, and the test names the copy of the host after the program that it stands in for,
/// such as `UnrealEditor-Cmd.exe`. The program reads the plan, does each step in the order below,
/// and gives the exit code of the plan.
/// </summary>
public static class Program
{
    /// <summary>The name of the plan file, in the folder of this program.</summary>
    public const string PlanFile = "stub-plan.txt";

    /// <summary>The exit code of a stub that cannot read or run its plan. It is not an exit code of a plan.</summary>
    private const int FaultExitCode = 70;

    /// <summary>Reads the plan beside this program, and acts on it.</summary>
    /// <param name="args">The arguments that the program under test gave.</param>
    /// <returns>The exit code of the plan, or 70 when the plan is absent or wrong.</returns>
    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        string planPath = Path.Combine(AppContext.BaseDirectory, PlanFile);
        if (!File.Exists(planPath))
        {
            Console.Error.WriteLine($"stub: no plan file '{planPath}'. The test writes it beside this program.");
            return FaultExitCode;
        }

        try
        {
            return Act(File.ReadAllLines(planPath), args, planPath);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or FormatException)
        {
            Console.Error.WriteLine($"stub: the plan '{planPath}' did not run: {fault.Message}");
            return FaultExitCode;
        }
    }

    private static int Act(IReadOnlyList<string> plan, IReadOnlyList<string> args, string planPath)
    {
        int exitCode = 0;
        int waitSeconds = 0;
        foreach (string line in plan)
        {
            if (line.Length == 0)
            {
                continue;
            }

            int separator = line.IndexOf('=', StringComparison.Ordinal);
            if (separator < 0)
            {
                throw new FormatException($"the line '{line}' has no '=' between the key and the value");
            }

            string key = line[..separator];
            string value = Unescape(line[(separator + 1)..]);
            switch (key)
            {
                case "arguments":
                    File.AppendAllLines(value, args);
                    break;
                case "stdout":
                    Console.Out.WriteLine(value);
                    break;
                case "report":
                    WriteReport(ValueOfOption(args, "-ReportExportPath="), value);
                    break;
                case "abslog":
                    File.AppendAllText(ValueOfOption(args, "-abslog="), value + Environment.NewLine);
                    break;
                case "csv":
                    File.WriteAllText(ValueOfOption(args, "-FrameTimeCsv="), value);
                    break;
                case "seconds":
                    waitSeconds = Number(key, value);
                    break;
                case "exit":
                    exitCode = Number(key, value);
                    break;
                default:
                    throw new FormatException($"the plan '{planPath}' has the unknown key '{key}'");
            }
        }

        // The wait comes after each write, so a test of a time limit still finds the output.
        Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
        return exitCode;
    }

    /// <summary>Writes the test report where the automation controller of the editor writes it.</summary>
    private static void WriteReport(string reportFolder, string report)
    {
        Directory.CreateDirectory(reportFolder);
        // The editor of 5.8.3 writes the report with a UTF-8 byte order mark, so the stub does too.
        File.WriteAllText(Path.Combine(reportFolder, "index.json"), report, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    /// <summary>Gives the value of the one argument that starts with a prefix.</summary>
    private static string ValueOfOption(IReadOnlyList<string> args, string prefix)
    {
        foreach (string arg in args)
        {
            if (arg.StartsWith(prefix, StringComparison.Ordinal))
            {
                return arg[prefix.Length..];
            }
        }

        throw new FormatException($"the plan needs the argument '{prefix}', and the program under test gave none of the {args.Count} arguments with it");
    }

    private static int Number(string key, string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
        {
            throw new FormatException($"the key '{key}' has the value '{value}', which is no whole number");
        }

        return number;
    }

    /// <summary>
    /// Reads the one escape of the plan format. A value holds one line, so a test that needs a
    /// line end in a value writes `\n`, and a value that needs a backslash writes `\\`.
    /// </summary>
    private static string Unescape(string value)
    {
        if (!value.Contains('\\', StringComparison.Ordinal))
        {
            return value;
        }

        StringBuilder text = new StringBuilder(value.Length);
        for (int index = 0; index < value.Length; index++)
        {
            if (value[index] != '\\' || index + 1 == value.Length)
            {
                text.Append(value[index]);
                continue;
            }

            index++;
            text.Append(value[index] switch
            {
                'n' => '\n',
                '\\' => '\\',
                _ => throw new FormatException($"the value '{value}' holds the unknown escape '\\{value[index]}'"),
            });
        }

        return text.ToString();
    }
}
