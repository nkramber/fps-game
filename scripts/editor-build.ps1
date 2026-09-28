# The editor build of the Windows PC (D-72). It matches `make editor-build` of the Mac.
#
# Run it from any folder of the checkout, in PowerShell:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\editor-build.ps1
# It builds the target IronAbsolutionEditor for Win64 Development with Build.bat of the engine.
# The engine folder comes from IRON_ABSOLUTION_ENGINE_DIR (D-79). The build tool writes its log
# to Game\Saved\Logs\editor-build.log. Post that log in the PR (D-33). The exit code is the exit
# code of Build.bat, so a failed build gives a code that is not 0 (T-2).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$EngineVariable = 'IRON_ABSOLUTION_ENGINE_DIR'

# The root of the checkout is the parent of the folder of this script.
$root = Split-Path -Parent $PSScriptRoot
$engine = [System.Environment]::GetEnvironmentVariable($EngineVariable)
if (-not $engine) {
    Write-Output "editor-build: set $EngineVariable to the folder that holds 'Engine' (D-79)."
    exit 1
}

# Path.Combine keeps the separator of the platform, so the hosted tests run the script on Linux.
$buildBatch = [System.IO.Path]::Combine($engine, 'Engine', 'Build', 'BatchFiles', 'Build.bat')
if (-not (Test-Path -LiteralPath $buildBatch)) {
    Write-Output "editor-build: no file '$buildBatch'. $EngineVariable names the folder that holds 'Engine' (D-79)."
    exit 1
}

$project = [System.IO.Path]::Combine($root, 'Game', 'IronAbsolution.uproject')
$log = [System.IO.Path]::Combine($root, 'Game', 'Saved', 'Logs', 'editor-build.log')
Write-Output "editor-build: build IronAbsolutionEditor Win64 Development. The log goes to '$log'."
& $buildBatch IronAbsolutionEditor Win64 Development "-Project=$project" -WaitMutex "-Log=$log"
$exitCode = $LASTEXITCODE
Write-Output "editor-build: Build.bat gave the exit code $exitCode."
exit $exitCode
