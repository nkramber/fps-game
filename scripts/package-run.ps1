# The start command of the Windows package (D-72, D-89). It matches `make package-run` of the
# Mac, and it applies the pass rule of IronAbsolution.Tools/PackageRun/PackageRunRules.cs.
#
# Run it from any folder of the checkout, in PowerShell, after scripts\package-build.ps1:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\package-run.ps1
# It starts the package with the timed-run option. The package runs the test map for 10 seconds,
# writes the success line in its log, and stops. A pass needs two facts: the exit code 0, and the
# success line in the log. The script stops a package that runs for 5 minutes. It prints one line
# for each check, and it gives the exit code 1 when a check fails. Each failure names the log
# (T-2). Post the output and the log Game\Saved\Logs\package-run.log in the PR (D-33).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RunSeconds = 10
$LimitMinutes = 5
$SuccessLine = "Timed run: pass. The map /Game/Maps/L_Test ran for $RunSeconds seconds."

# The root of the checkout is the parent of the folder of this script.
$root = Split-Path -Parent $PSScriptRoot

# Path.Combine keeps the separator of the platform, so the hosted tests run the script on Linux.
# The program under Binaries is the game itself. The program at the top of the package only
# starts it, so its exit code does not prove the run.
$game = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Packages', 'Windows', 'IronAbsolution', 'Binaries', 'Win64', 'IronAbsolution.exe')
if (-not (Test-Path -LiteralPath $game)) {
    Write-Output "package-run: no file '$game'. Run scripts\package-build.ps1 first."
    exit 1
}

# Remove the log of the last run, so a run that writes no log cannot pass on an old one (T-2).
$log = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Logs', 'package-run.log')
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $log) | Out-Null
if (Test-Path -LiteralPath $log) {
    Remove-Item -LiteralPath $log -Force
}

# The game writes its log to the path of `-abslog`. The Windows package has no sandbox, so it
# can write there (F-27). A window keeps the desktop free during the run.
$arguments = "-TimedRunSeconds=$RunSeconds -unattended -windowed -ResX=1280 -ResY=720 -abslog=`"$log`""
Write-Output "package-run: run the package for $RunSeconds seconds on the test map, with a limit of $LimitMinutes minutes. The log goes to '$log'."
$process = Start-Process -FilePath $game -ArgumentList $arguments -NoNewWindow -PassThru
# The handle keeps the exit code of the process after the wait with a time limit.
$null = $process.Handle
$timedOut = -not $process.WaitForExit($LimitMinutes * 60 * 1000)
if ($timedOut) {
    # The game can start helper processes, so the stop takes the whole tree on Windows.
    if ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT) {
        & taskkill.exe /T /F /PID $process.Id | Out-Null
    }
    else {
        $process.Kill()
    }
}
$process.WaitForExit()

$results = @()

function Add-Check {
    param([string]$Check, [bool]$Holds, [string]$Detail)
    $script:results += [pscustomobject]@{ Check = $Check; Holds = $Holds; Detail = $Detail }
}

# The exit code.
if ($timedOut) {
    Add-Check 'Package exit' $false "The time limit of $LimitMinutes minutes stopped the package. Read the log '$log'"
}
elseif ($process.ExitCode -ne 0) {
    Add-Check 'Package exit' $false "The package gave the exit code $($process.ExitCode). Read the log '$log'"
}
else {
    Add-Check 'Package exit' $true 'The package gave the exit code 0'
}

# The success line of the log.
$logText = $null
if (Test-Path -LiteralPath $log) {
    # An empty file gives no text, not an empty text.
    $logText = [string](Get-Content -LiteralPath $log -Raw)
}
if ($null -eq $logText) {
    Add-Check 'Success line' $false "No log: no file '$log'"
}
elseif ($logText.Contains($SuccessLine)) {
    Add-Check 'Success line' $true "The log '$log' holds the line '$SuccessLine'"
}
else {
    Add-Check 'Success line' $false "The log '$log' has no line '$SuccessLine'. An exit code of 0 alone is not a pass"
}

$failed = 0
foreach ($result in $results) {
    $state = 'pass'
    if (-not $result.Holds) {
        $state = 'fail'
        $failed += 1
    }
    Write-Output "package-run: $($result.Check): $state. $($result.Detail.TrimEnd('.'))."
}

if ($failed -eq 0) {
    Write-Output "package-run: pass. Each of the $($results.Count) checks passes."
    exit 0
}

Write-Output "package-run: fail. $failed of the $($results.Count) checks fail."
exit 1
