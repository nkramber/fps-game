# The entry of the development commands and the engine commands of the repository (D-99). It
# keeps the target names of D-41. The project supports Windows alone (D-91).
#
# Run it from any folder of the checkout, in PowerShell:
#   .\run.ps1                    the `verify` target
#   .\run.ps1 test
#   .\run.ps1 codex-review -PR 13
#
# The script gives the exit code of the command that it starts. So `codex-review` gives 0 for an
# approval, 10 for changes, 11 for the three-strike stop, 3 for a refused start, and 1 for a fault
# (D-14).
#
# The engine targets read the engine folder from IRON_ABSOLUTION_ENGINE_DIR, so no commit holds a
# path of one machine (D-9, D-79). `verify` runs no engine target, because the hosted runners have
# no engine (D-31). `package-run` opens a game window, so a session asks the owner first (D-96).
# `content-build` runs the editor with no window.

[CmdletBinding()]
param(
    # The target to run. `verify` runs the build, the tests, the format check, and ste-check.
    [Parameter(Position = 0)]
    [string] $Target = 'verify',

    # The GitHub number of the pull request. The `codex-review` target needs it.
    [int] $PR = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The root of the checkout is the folder of this script, so the target runs from any folder.
$Root = $PSScriptRoot
$Solution = Join-Path $Root 'IronAbsolution.slnx'
$ToolsProject = Join-Path $Root 'IronAbsolution.Tools/IronAbsolution.Tools.csproj'

# Each target, with the one line of help that `help` prints.
$Targets = [ordered]@{
    'verify'          = 'build, test, format, and ste-check. Run it before each push.'
    'build'           = 'build every project of the solution.'
    'test'            = 'run every test of the solution. Run the `build` target first.'
    'format'          = 'fail when a file needs a format change (D-40).'
    'ste-check'       = 'the STE checker, the reference check, the session number check, and the size check (D-17).'
    'handoff-rotate'  = 'move each handoff entry after the tenth to the archive (D-58, D-59).'
    'codex-review'    = 'the cross-provider review of one PR. Needs -PR <number> (D-14, D-47).'
    'hooks'           = 'install the pre-commit hook in this checkout, one time (D-43).'
    'where'           = 'the branch, the tree, and the PR state.'
    'clean'           = 'remove the build output of every project.'
    'toolchain-check' = 'the pins of the Windows toolchain: Visual Studio, MSVC, the Windows SDK, the engine, and Git LFS (D-28, D-74).'
    'editor-build'    = 'build the editor target of the Unreal project. The log is Game\Saved\Logs\editor-build.log (D-72).'
    'editor-test'     = 'run each automation test of the project headless. Run `editor-build` first (D-71).'
    'content-build'   = 'make the scripted content with a headless editor. Run `editor-build` first. The log is Game\Saved\Logs\content-build.log (D-134).'
    'package-build'   = 'make a packaged Development build of the game. The log is Game\Saved\Logs\package-build.log (D-72).'
    'package-run'     = 'start the package for a timed run of the test map. It opens a game window (D-89, D-96).'
    'help'            = 'print this list.'
}

function Write-Fault {
    param([string] $Message)

    # Every fault names the target and what to do, so no failure is silent (T-2).
    Write-Output "run.ps1: $Message"
}

function Assert-Program {
    param([string] $Name, [string] $Reason)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Write-Fault "${Target}: '$Name' is not on the PATH. $Reason"
        exit 1
    }
}

function Invoke-Step {
    param([string] $Name, [string] $Program, [string[]] $Arguments)

    & $Program @Arguments
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        Write-Fault "$Name failed with the exit code $code."
        exit $code
    }
}

function Get-EngineFile {
    param([string[]] $Parts)

    # The engine folder comes from the variable, so no commit holds its path (D-79).
    $engine = [System.Environment]::GetEnvironmentVariable('IRON_ABSOLUTION_ENGINE_DIR')
    if (-not $engine) {
        Write-Fault "${Target}: set IRON_ABSOLUTION_ENGINE_DIR to the folder that holds 'Engine' (D-79)."
        exit 1
    }

    # Path.Combine keeps the separator of the platform, so the hosted tests run the target on Linux.
    $file = [System.IO.Path]::Combine([string[]] (@($engine, 'Engine') + $Parts))
    if (-not (Test-Path -LiteralPath $file)) {
        Write-Fault "${Target}: no file '$file'. IRON_ABSOLUTION_ENGINE_DIR names the folder that holds 'Engine' (D-79)."
        exit 1
    }

    return $file
}

function Get-CodexEntry {
    # npm gives the global package folder on each platform. The command starts the entry script
    # with `node`, because npm writes a `codex.cmd` shim on Windows and no `bin/codex` link (D-47).
    $packages = & npm root --global
    if ($LASTEXITCODE -ne 0) {
        Write-Fault "codex-review: 'npm root --global' failed with the exit code $LASTEXITCODE."
        exit 1
    }

    return Join-Path ($packages | Select-Object -Last 1).Trim() '@openai/codex/bin/codex.js'
}

if (-not $Targets.Contains($Target)) {
    Write-Fault "no target '$Target'. The targets are: $($Targets.Keys -join ', ')."
    exit 1
}

if ($Target -eq 'help') {
    Write-Output 'run.ps1: the development commands of the repository (D-99).'
    foreach ($name in $Targets.Keys) {
        Write-Output ("  {0,-15} {1}" -f $name, $Targets[$name])
    }

    exit 0
}

# Each target of the tools project needs the .NET SDK of global.json (D-40).
if ($Target -notin @('where', 'hooks', 'editor-build', 'content-build', 'package-build')) {
    Assert-Program 'dotnet' 'Install the .NET SDK of global.json. docs/runbooks/session-context.md gives the step.'
}

switch ($Target) {
    'verify' {
        Invoke-Step 'build' 'dotnet' @('build', $Solution)
        Invoke-Step 'test' 'dotnet' @('test', '--solution', $Solution, '--no-build')
        Invoke-Step 'format' 'dotnet' @('format', $Solution, '--verify-no-changes')
        Invoke-Step 'ste-check' 'dotnet' @('run', '--project', $ToolsProject, '--', 'ste-check', '--root', $Root)
        Write-Output 'run.ps1: verify passed. The build, the tests, the format check, and ste-check are green.'
    }
    'build' {
        Invoke-Step 'build' 'dotnet' @('build', $Solution)
    }
    'test' {
        Invoke-Step 'test' 'dotnet' @('test', '--solution', $Solution, '--no-build')
    }
    'format' {
        Invoke-Step 'format' 'dotnet' @('format', $Solution, '--verify-no-changes')
    }
    'ste-check' {
        Invoke-Step 'ste-check' 'dotnet' @('run', '--project', $ToolsProject, '--', 'ste-check', '--root', $Root)
    }
    'handoff-rotate' {
        Invoke-Step 'handoff-rotate' 'dotnet' @('run', '--project', $ToolsProject, '--', 'handoff-rotate', '--root', $Root)
    }
    'codex-review' {
        if ($PR -le 0) {
            Write-Fault 'codex-review: set -PR <number>, such as .\run.ps1 codex-review -PR 13 (T-2).'
            exit 1
        }

        Assert-Program 'npm' 'Install Node.js with npm. The target installs the Codex CLI with it (D-47).'
        Invoke-Step 'npm install' 'npm' @('install', '--global', '@openai/codex@latest')
        $codex = Get-CodexEntry
        # The command gives its own exit code the meaning of the review outcome, so no step check
        # runs here. The caller reads 0, 10, 11, 3, or 1 (D-14).
        & dotnet run --project $ToolsProject -- codex-review --root $Root --pr $PR --codex $codex
        exit $LASTEXITCODE
    }
    'hooks' {
        Assert-Program 'git' 'Install Git for Windows.'
        Invoke-Step 'git config' 'git' @('-C', $Root, 'config', 'core.hooksPath', '.githooks')
        Write-Output 'run.ps1: hooks: the hook path is .githooks.'
    }
    'where' {
        Assert-Program 'git' 'Install Git for Windows.'
        Assert-Program 'gh' 'Install the GitHub CLI, then run `gh auth login`.'
        Invoke-Step 'git status' 'git' @('-C', $Root, 'status', '--short', '--branch')
        & gh pr status
        exit $LASTEXITCODE
    }
    'clean' {
        Invoke-Step 'clean' 'dotnet' @('clean', $Solution)
    }
    'toolchain-check' {
        Invoke-Step 'toolchain-check' 'dotnet' @('run', '--project', $ToolsProject, '--', 'toolchain-check')
    }
    'editor-build' {
        # Build.bat gives 0 for the result "up to date", and so does this target.
        $buildBatch = Get-EngineFile @('Build', 'BatchFiles', 'Build.bat')
        $project = [System.IO.Path]::Combine($Root, 'Game', 'IronAbsolution.uproject')
        $log = [System.IO.Path]::Combine($Root, 'Game', 'Saved', 'Logs', 'editor-build.log')
        Write-Output "run.ps1: editor-build: build IronAbsolutionEditor Win64 Development. The log goes to '$log'."
        Invoke-Step 'editor-build' $buildBatch @('IronAbsolutionEditor', 'Win64', 'Development', "-Project=$project", '-WaitMutex', "-Log=$log")
        Write-Output 'run.ps1: editor-build passed.'
    }
    'editor-test' {
        # The command starts the editor with no window. A pass needs the exit code 0, a test report
        # with no failed test, and the success line of the log (D-71).
        Invoke-Step 'editor-test' 'dotnet' @('run', '--project', $ToolsProject, '--', 'editor-test', '--root', $Root)
    }
    'content-build' {
        # The Python commandlet runs the content script with no window (D-96, D-134). The script
        # raises on each fault, and the commandlet then gives a nonzero exit code. The editor
        # program of the console subsystem writes its log to stdout.
        $editor = Get-EngineFile @('Binaries', 'Win64', 'UnrealEditor-Cmd.exe')
        $project = [System.IO.Path]::Combine($Root, 'Game', 'IronAbsolution.uproject')
        $script = [System.IO.Path]::Combine($Root, 'Game', 'Scripts', 'build_content.py')
        $log = [System.IO.Path]::Combine($Root, 'Game', 'Saved', 'Logs', 'content-build.log')
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $log) | Out-Null

        Write-Output "run.ps1: content-build: run '$script' in a headless editor. The log goes to '$log'."
        & $editor $project -run=pythonscript "-script=$script" -unattended -nullrhi -nosplash -nosound -stdout -FullStdOutLogOutput | Out-File -FilePath $log
        $code = $LASTEXITCODE
        if ($code -ne 0) {
            Write-Fault "content-build failed with the exit code $code. Read the log '$log'."
            exit $code
        }

        Write-Output 'run.ps1: content-build passed.'
    }
    'package-build' {
        # RunUAT builds the game target, cooks the test map, and puts the package in
        # Game\Saved\Packages\Windows. The target first removes the last package, so a failed build
        # leaves no old package for `package-run` (T-2).
        $runUat = Get-EngineFile @('Build', 'BatchFiles', 'RunUAT.bat')
        $project = [System.IO.Path]::Combine($Root, 'Game', 'IronAbsolution.uproject')
        $archive = [System.IO.Path]::Combine($Root, 'Game', 'Saved', 'Packages')
        $package = [System.IO.Path]::Combine($archive, 'Windows')
        $log = [System.IO.Path]::Combine($Root, 'Game', 'Saved', 'Logs', 'package-build.log')
        if (Test-Path -LiteralPath $package) {
            Remove-Item -LiteralPath $package -Recurse -Force
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $log) | Out-Null

        Write-Output "run.ps1: package-build: package IronAbsolution Win64 Development in '$package'. The log goes to '$log'."
        # Tee-Object takes stdout alone. A redirect of stderr gives an error record for each line of
        # stderr, and the stop preference of the script then stops at the first line.
        & $runUat BuildCookRun "-project=$project" -platform=Win64 -clientconfig=Development -build -cook -stage -package -pak -archive "-archivedirectory=$archive" -unattended -utf8output -nop4 | Tee-Object -FilePath $log
        $code = $LASTEXITCODE
        if ($code -ne 0) {
            Write-Fault "package-build failed with the exit code $code. Read the log '$log'."
            exit $code
        }

        Write-Output 'run.ps1: package-build passed.'
    }
    'package-run' {
        # The package runs the test map for 10 seconds and stops. A pass needs the exit code 0 and
        # the success line of the log (D-89).
        Invoke-Step 'package-run' 'dotnet' @('run', '--project', $ToolsProject, '--', 'package-run', '--root', $Root)
    }
}
