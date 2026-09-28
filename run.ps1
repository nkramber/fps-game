# The entry of the development commands of the repository (D-99). It replaces the Makefile of the
# Mac for that work, and it keeps the target names of D-41.
#
# Run it from any folder of the checkout, in PowerShell:
#   .\run.ps1                    the `verify` target
#   .\run.ps1 test
#   .\run.ps1 codex-review -PR 13
#
# The script gives the exit code of the command that it starts. So `codex-review` gives 0 for an
# approval, 10 for changes, 11 for the three-strike stop, 3 for a refused start, and 1 for a fault
# (D-14). Make gave 2 for each code other than 0, and this script does not (D-99).
#
# The engine targets stay in the Makefile until PR-14 moves them (section 7.6 of the phase 1 file).
# `scripts/` holds the engine scripts of the Windows PC (D-72).

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
    'verify'         = 'build, test, format, and ste-check. Run it before each push.'
    'build'          = 'build every project of the solution.'
    'test'           = 'run every test of the solution. Run the `build` target first.'
    'format'         = 'fail when a file needs a format change (D-40).'
    'ste-check'      = 'the STE checker, the reference check, the session number check, and the size check (D-17).'
    'handoff-rotate' = 'move each handoff entry after the tenth to the archive (D-58, D-59).'
    'codex-review'   = 'the cross-provider review of one PR. Needs -PR <number> (D-14, D-47).'
    'hooks'          = 'install the pre-commit hook in this checkout, one time (D-43).'
    'where'          = 'the branch, the tree, and the PR state.'
    'clean'          = 'remove the build output of every project.'
    'help'           = 'print this list.'
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
        Write-Output ("  {0,-14} {1}" -f $name, $Targets[$name])
    }

    exit 0
}

# Each target below `where` needs the .NET SDK of global.json (D-40).
if ($Target -notin @('where', 'hooks')) {
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
}
