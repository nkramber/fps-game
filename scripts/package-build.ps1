# The packaged Development build of the Windows PC (D-72, D-89). It matches `make package-build`
# of the Mac.
#
# Run it from any folder of the checkout, in PowerShell:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\package-build.ps1
# RunUAT.bat of the engine builds the game target for Win64 Development, cooks the test map, and
# puts the package in Game\Saved\Packages\Windows. The engine folder comes from
# IRON_ABSOLUTION_ENGINE_DIR (D-79). The script first removes the last package, so a failed build
# leaves no old package for scripts\package-run.ps1. The output goes to
# Game\Saved\Logs\package-build.log too. Post that log in the PR (D-33). The exit code is the exit
# code of RunUAT.bat, so a failed build gives a code that is not 0 (T-2).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$EngineVariable = 'IRON_ABSOLUTION_ENGINE_DIR'

# The root of the checkout is the parent of the folder of this script.
$root = Split-Path -Parent $PSScriptRoot
$engine = [System.Environment]::GetEnvironmentVariable($EngineVariable)
if (-not $engine) {
    Write-Output "package-build: set $EngineVariable to the folder that holds 'Engine' (D-79)."
    exit 1
}

# Path.Combine keeps the separator of the platform, so the hosted tests run the script on Linux.
$runUat = [System.IO.Path]::Combine($engine, 'Engine', 'Build', 'BatchFiles', 'RunUAT.bat')
if (-not (Test-Path -LiteralPath $runUat)) {
    Write-Output "package-build: no file '$runUat'. $EngineVariable names the folder that holds 'Engine' (D-79)."
    exit 1
}

$project = [System.IO.Path]::Combine($root, 'Game', 'IronAbsolution.uproject')
$archive = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Packages')
$package = [System.IO.Path]::Combine($archive, 'Windows')
$log = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Logs', 'package-build.log')
if (Test-Path -LiteralPath $package) {
    Remove-Item -LiteralPath $package -Recurse -Force
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $log) | Out-Null

Write-Output "package-build: package IronAbsolution Win64 Development in '$package'. The log goes to '$log'."
# Tee-Object takes stdout alone. A redirect of stderr gives an error record for each line of
# stderr, and the stop preference of the script then stops at the first line.
& $runUat BuildCookRun "-project=$project" -platform=Win64 -clientconfig=Development -build -cook -stage -package -pak -archive "-archivedirectory=$archive" -unattended -utf8output -nop4 | Tee-Object -FilePath $log
$exitCode = $LASTEXITCODE
Write-Output "package-build: RunUAT.bat gave the exit code $exitCode."
exit $exitCode
