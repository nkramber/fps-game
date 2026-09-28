# The headless test run of the Windows PC (D-71, D-72). It matches `make editor-test` of the Mac,
# and it applies the pass rule of IronAbsolution.Tools/EditorTest/EditorTestRules.cs.
#
# Run it from any folder of the checkout, in PowerShell, after scripts\editor-build.ps1:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\editor-test.ps1
# It starts UnrealEditor-Cmd.exe with no window and runs each automation test of the project. A
# pass needs three facts: the exit code 0, a test report with at least one passed test and no
# other test, and the success line of the log. An exit code of 0 alone is not a pass. It prints
# one line for each check, and it gives the exit code 1 when a check fails (T-2). Post the output
# and the log Game\Saved\Logs\editor-test.log in the PR (D-33).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$EngineVariable = 'IRON_ABSOLUTION_ENGINE_DIR'
$TestFilter = 'IronAbsolution'
$SuccessLine = '**** TEST COMPLETE. EXIT CODE: 0 ****'
$PassedState = 'Success'
$LimitMinutes = 30

# The root of the checkout is the parent of the folder of this script.
$root = Split-Path -Parent $PSScriptRoot
$engine = [System.Environment]::GetEnvironmentVariable($EngineVariable)
if (-not $engine) {
    Write-Output "editor-test: set $EngineVariable to the folder that holds 'Engine' (D-79)."
    exit 1
}

# Path.Combine keeps the separator of the platform, so the hosted tests run the script on Linux.
$editor = [System.IO.Path]::Combine($engine, 'Engine', 'Binaries', 'Win64', 'UnrealEditor-Cmd.exe')
$project = [System.IO.Path]::Combine($root, 'Game', 'IronAbsolution.uproject')
foreach ($path in @($editor, $project)) {
    if (-not (Test-Path -LiteralPath $path)) {
        Write-Output "editor-test: no file '$path'. $EngineVariable names the engine folder, and the script is in the checkout."
        exit 1
    }
}

$reportFolder = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Automation', 'editor-test')
$reportFile = [System.IO.Path]::Combine($reportFolder, 'index.json')
$log = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Logs', 'editor-test.log')
$errorLog = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Logs', 'editor-test.stderr.log')

# Remove the report and the logs of the last run, so a run that writes no report cannot pass on
# an old one (T-2). A file that does not go away stops the script with its error.
if (Test-Path -LiteralPath $reportFolder) {
    Remove-Item -LiteralPath $reportFolder -Recurse -Force
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $log) | Out-Null
foreach ($path in @($log, $errorLog)) {
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
}

# The editor reads the raw command line, so each value with a space keeps its quotes after the
# equals sign. `Quit` inside the automation command makes the controller write the success line
# and set the exit code from the results. `-nullrhi` starts no renderer.
$arguments = "`"$project`" -ExecCmds=`"Automation RunTests $TestFilter;Quit`" -ReportExportPath=`"$reportFolder`" -unattended -nullrhi -nosplash -nopause -nosound -stdout -FullStdOutLogOutput"
Write-Output "editor-test: run the tests '$TestFilter' with no window, with a limit of $LimitMinutes minutes. The log goes to '$log'."
$process = Start-Process -FilePath $editor -ArgumentList $arguments -RedirectStandardOutput $log -RedirectStandardError $errorLog -NoNewWindow -PassThru
# The handle keeps the exit code of the process after the wait with a time limit.
$null = $process.Handle
$timedOut = -not $process.WaitForExit($LimitMinutes * 60 * 1000)
if ($timedOut) {
    # The editor starts shader workers, so the stop takes the whole tree on Windows.
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
    Add-Check 'Editor exit' $false "The time limit of $LimitMinutes minutes stopped the editor"
}
else {
    Add-Check 'Editor exit' ($process.ExitCode -eq 0) "The editor gave the exit code $($process.ExitCode)"
}

# A whole number in one field of the report, or a throw that names the file and the field.
function Get-ReportNumber {
    param($Report, [string]$Field)
    $value = $null
    if ($Report.PSObject.Properties.Name -contains $Field) {
        $value = $Report.$Field
    }
    if (-not ($value -is [int] -or $value -is [long])) {
        throw "The report '$reportFile' has no whole number in the field '$Field'"
    }
    return $value
}

# A text in one field of one test, or a throw that names the file and the field.
function Get-TestText {
    param($Test, [string]$Field)
    $value = $null
    if ($Test -is [pscustomobject] -and ($Test.PSObject.Properties.Name -contains $Field)) {
        $value = $Test.$Field
    }
    if (-not ($value -is [string]) -or $value.Length -eq 0) {
        throw "The report '$reportFile' has a test with no text in the field '$Field'"
    }
    return $value
}

# The test report.
if (-not (Test-Path -LiteralPath $reportFile)) {
    Add-Check 'Test report' $false "No test report: no file '$reportFile'"
}
else {
    try {
        try {
            $report = Get-Content -LiteralPath $reportFile -Raw | ConvertFrom-Json
        }
        catch {
            throw "The report '$reportFile' is not JSON: $($_.Exception.Message)"
        }
        if (-not ($report -is [pscustomobject])) {
            throw "The report '$reportFile' is not a JSON object"
        }
        $counts = [ordered]@{}
        foreach ($field in @('succeeded', 'succeededWithWarnings', 'failed', 'notRun', 'inProcess')) {
            $counts[$field] = Get-ReportNumber $report $field
        }
        if (-not ($report.PSObject.Properties.Name -contains 'tests') -or -not ($report.tests -is [array])) {
            throw "The report '$reportFile' has no array in the field 'tests'"
        }
        $tests = @()
        foreach ($test in $report.tests) {
            $tests += [pscustomobject]@{ Path = (Get-TestText $test 'fullTestPath'); State = (Get-TestText $test 'state') }
        }

        $faults = @()
        if ($tests.Count -eq 0) {
            $faults += 'it names no test, so the test filter matched none'
        }
        foreach ($field in @('failed', 'notRun', 'inProcess')) {
            if ($counts[$field] -ne 0) {
                $faults += "'$field' is $($counts[$field])"
            }
        }
        $notPassed = @($tests | Where-Object { $_.State -ne $PassedState } | ForEach-Object { "$($_.Path) ($($_.State))" })
        if ($notPassed.Count -gt 0) {
            $faults += "these tests did not pass: $($notPassed -join ', ')"
        }

        if ($faults.Count -gt 0) {
            Add-Check 'Test report' $false "The report '$reportFile' shows a fault: $($faults -join '; and ')"
        }
        else {
            $names = ($tests | ForEach-Object { $_.Path }) -join ', '
            Add-Check 'Test report' $true "The report '$reportFile' shows $($tests.Count) passed tests: $names"
        }
    }
    catch {
        Add-Check 'Test report' $false $_.Exception.Message
    }
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
    Write-Output "editor-test: $($result.Check): $state. $($result.Detail.TrimEnd('.'))."
}

if ($failed -eq 0) {
    Write-Output "editor-test: pass. Each of the $($results.Count) checks passes."
    exit 0
}

Write-Output "editor-test: fail. $failed of the $($results.Count) checks fail."
exit 1
