using System;
using System.IO;
using Xunit;

namespace IronAbsolution.Tests;

/// <summary>
/// Runs `scripts/package-build.ps1` and `scripts/package-run.ps1` under PowerShell with stub
/// programs (D-72, D-89). Each script takes the checkout from its own folder, so each test copies
/// the script into a temporary checkout. Each stub is a POSIX shell script at the path of the
/// Windows program, and the hosted Linux runner starts it (D-55). <see cref="PowerShellScript"/>
/// runs each script, and CI must run these tests.
/// </summary>
public sealed class PackageScriptTests : IDisposable
{
    private const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";
    private const string SuccessLine = "LogTimedRun: Display: Timed run: pass. The map /Game/Maps/L_Test ran for 10 seconds.";

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"package-script-{Guid.NewGuid():N}");
    private readonly string root;
    private readonly string engine;
    private readonly string logPath;

    public PackageScriptTests()
    {
        this.root = Path.Combine(this.folder, "checkout");
        this.engine = Path.Combine(this.folder, "engine");
        this.logPath = Path.Combine(this.root, "Game", "Saved", "Logs", "package-run.log");
        Directory.CreateDirectory(Path.Combine(this.root, "scripts"));
        foreach (string script in new[] { "package-build.ps1", "package-run.ps1" })
        {
            File.Copy(RepositoryRoot.PathTo($"scripts/{script}"), Path.Combine(this.root, "scripts", script));
        }
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void TheRunScriptPassesAStubRunThatWritesTheSuccessLineToItsLog()
    {
        // Exit test 3 of PR-10 on the Windows script.
        this.WriteGame($"echo '{SuccessLine}' > \"$log\"\nexit 0\n");

        (int exitCode, string output) = this.RunScript("package-run.ps1", engineFolder: null);

        Assert.Equal(0, exitCode);
        Assert.Contains("package-run: Package exit: pass. The package gave the exit code 0.", output, StringComparison.Ordinal);
        Assert.Contains($"package-run: Success line: pass. The log '{this.logPath}' holds the line", output, StringComparison.Ordinal);
        Assert.Contains("package-run: pass. Each of the 2 checks passes.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunScriptFailsARunWithNoSuccessLineAndNamesTheLogEvenWithAnOldLog()
    {
        // Exit test 4 of PR-10 on the Windows script (T-2).
        Directory.CreateDirectory(Path.GetDirectoryName(this.logPath)!);
        File.WriteAllText(this.logPath, SuccessLine);
        this.WriteGame("echo 'LogInit: Display: Engine is initialized.' > \"$log\"\nexit 0\n");

        (int exitCode, string output) = this.RunScript("package-run.ps1", engineFolder: null);

        Assert.Equal(1, exitCode);
        Assert.Contains($"package-run: Success line: fail. The log '{this.logPath}' has no line", output, StringComparison.Ordinal);
        Assert.Contains("An exit code of 0 alone is not a pass.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunScriptFailsAnAbsentLogAndACrashWithThePath()
    {
        this.WriteGame("exit 3\n");

        (int exitCode, string output) = this.RunScript("package-run.ps1", engineFolder: null);

        Assert.Equal(1, exitCode);
        Assert.Contains($"package-run: Package exit: fail. The package gave the exit code 3. Read the log '{this.logPath}'.", output, StringComparison.Ordinal);
        Assert.Contains($"package-run: Success line: fail. No log: no file '{this.logPath}'.", output, StringComparison.Ordinal);
        Assert.Contains("package-run: fail. 2 of the 2 checks fail.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRunScriptGivesTheGameTheTimedRunOptionAndTheLogPath()
    {
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        this.WriteGame($"for arg in \"$@\"; do echo \"$arg\" >> '{argumentsFile}'; done\nexit 0\n");

        this.RunScript("package-run.ps1", engineFolder: null);

        Assert.Equal(
            ["-TimedRunSeconds=10", "-unattended", "-windowed", "-ResX=1280", "-ResY=720", $"-abslog={this.logPath}"],
            File.ReadAllLines(argumentsFile));
    }

    [Fact]
    public void TheRunScriptFailsWithThePathWhenThePackageIsAbsent()
    {
        (int exitCode, string output) = this.RunScript("package-run.ps1", engineFolder: null);

        Assert.Equal(1, exitCode);
        string game = Path.Combine(this.root, "Game", "Saved", "Packages", "Windows", "IronAbsolution", "Binaries", "Win64", "IronAbsolution.exe");
        Assert.Equal($"package-run: no file '{game}'. Run scripts\\package-build.ps1 first.", output.Trim());
    }

    [Fact]
    public void TheBuildScriptFailsAndNamesTheVariableWhenItIsNotSet()
    {
        (int exitCode, string output) = this.RunScript("package-build.ps1", engineFolder: null);

        Assert.Equal(1, exitCode);
        Assert.Equal("package-build: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds 'Engine' (D-79).", output.Trim());
    }

    [Fact]
    public void TheBuildScriptFailsWithThePathWhenRunUatIsAbsent()
    {
        (int exitCode, string output) = this.RunScript("package-build.ps1", this.engine);

        Assert.Equal(1, exitCode);
        string runUat = Path.Combine(this.engine, "Engine", "Build", "BatchFiles", "RunUAT.bat");
        Assert.Contains($"package-build: no file '{runUat}'.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBuildScriptRemovesTheOldPackageAndGivesTheExitCodeTheArgumentsAndTheLog()
    {
        string oldPackage = Path.Combine(this.root, "Game", "Saved", "Packages", "Windows");
        Directory.CreateDirectory(oldPackage);
        string argumentsFile = Path.Combine(this.folder, "arguments.txt");
        string runUat = Path.Combine(this.engine, "Engine", "Build", "BatchFiles", "RunUAT.bat");
        WriteStub(runUat, $"for arg in \"$@\"; do echo \"$arg\" >> '{argumentsFile}'; done\necho 'BUILD FAILED'\nexit 7\n");

        (int exitCode, string output) = this.RunScript("package-build.ps1", this.engine);

        Assert.Equal(7, exitCode);
        Assert.Contains("package-build: RunUAT.bat gave the exit code 7.", output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(oldPackage), "The script left the old package.");
        string log = Path.Combine(this.root, "Game", "Saved", "Logs", "package-build.log");
        Assert.Contains("BUILD FAILED", File.ReadAllText(log), StringComparison.Ordinal);
        string archive = Path.Combine(this.root, "Game", "Saved", "Packages");
        Assert.Equal(
            [
                "BuildCookRun",
                $"-project={Path.Combine(this.root, "Game", "IronAbsolution.uproject")}",
                "-platform=Win64",
                "-clientconfig=Development",
                "-build",
                "-cook",
                "-stage",
                "-package",
                "-pak",
                "-archive",
                $"-archivedirectory={archive}",
                "-unattended",
                "-utf8output",
                "-nop4",
            ],
            File.ReadAllLines(argumentsFile));
    }

    private static void WriteStub(string path, string body)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string script = "#!/bin/sh\n"
            + "for arg in \"$@\"; do\n"
            + "  case \"$arg\" in -abslog=*) log=\"${arg#-abslog=}\" ;; esac\n"
            + "done\n"
            + body;
        File.WriteAllText(path, script);
        if (OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Each stub is a POSIX shell script, and the development tools run on the Mac alone (D-55).");
        }

        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    }

    private void WriteGame(string body)
    {
        WriteStub(Path.Combine(this.root, "Game", "Saved", "Packages", "Windows", "IronAbsolution", "Binaries", "Win64", "IronAbsolution.exe"), body);
    }

    private (int ExitCode, string Output) RunScript(string script, string? engineFolder)
    {
        return PowerShellScript.Run(Path.Combine(this.root, "scripts", script), this.folder, EngineVariable, engineFolder);
    }
}
