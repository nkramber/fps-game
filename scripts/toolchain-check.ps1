# The toolchain check of the Windows PC (D-72, D-74). It matches `make toolchain-check` of the Mac.
#
# Run it from the root of the checkout, in PowerShell:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\toolchain-check.ps1
# It prints one line for each pin, with the expected value and the found value. It gives the
# exit code 1 when a pin fails (T-2). Post the full output in the PR (D-33).
#
# The Visual Studio, MSVC, and Windows SDK values come from the Visual Studio page of Epic for
# Unreal Engine 5.8, read on 2026-09-27 (D-74):
# https://dev.epicgames.com/documentation/unreal-engine/setting-up-visual-studio-development-environment-for-cplusplus-projects-in-unreal-engine
# The engine pin and the variable match IronAbsolution.Tools/ToolchainCheck/ToolchainPins.cs (D-28, D-79).

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$EngineVariable = 'IRON_ABSOLUTION_ENGINE_DIR'
$EnginePin = '5.8.3'
$VisualStudioMajorPin = 18
$MsvcPin = '14.50'
$WindowsSdkMinimum = [version]'10.0.22621.0'

$script:Failed = 0
$script:Total = 0

function Write-Pin {
    param([string]$Tool, [bool]$Holds, [string]$Expected, [string]$Found, [string]$Advice = '')
    $script:Total += 1
    $state = 'pass'
    if (-not $Holds) {
        $state = 'fail'
        $script:Failed += 1
    }
    $line = "toolchain-check: ${Tool}: $state. Expected $Expected, found $Found."
    if ($Advice) {
        $line = "$line $Advice"
    }
    Write-Output $line
}

# The Windows version is information for the evidence, not a pin.
$os = [System.Environment]::OSVersion.Version
Write-Output "toolchain-check: Windows: $os (information, not a pin)."

# Visual Studio and MSVC (D-74). vswhere ships with the Visual Studio Installer.
$vsExpected = "Visual Studio 2026, major version $VisualStudioMajorPin (D-74)"
$msvcExpected = "the default MSVC toolset $MsvcPin.x (D-74)"
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    Write-Pin 'Visual Studio' $false $vsExpected "no file '$vswhere'" 'Install Visual Studio 2026 from docs/runbooks/engine-setup.md.'
    Write-Pin 'MSVC' $false $msvcExpected 'no Visual Studio'
}
else {
    $instances = @(& $vswhere -latest -products '*' -requires 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' -format json | ConvertFrom-Json)
    if ($instances.Count -eq 0) {
        Write-Pin 'Visual Studio' $false $vsExpected 'no install with the C++ tools' 'Add the workload "Game development with C++".'
        Write-Pin 'MSVC' $false $msvcExpected 'no Visual Studio with the C++ tools'
    }
    else {
        $instance = $instances[0]
        $vsVersion = [version]$instance.installationVersion
        Write-Pin 'Visual Studio' ($vsVersion.Major -eq $VisualStudioMajorPin) $vsExpected "$($instance.displayName) $($instance.installationVersion)"

        $defaultFile = Join-Path $instance.installationPath 'VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt'
        $toolsetFolder = Join-Path $instance.installationPath 'VC\Tools\MSVC'
        $installed = 'none'
        if (Test-Path -LiteralPath $toolsetFolder) {
            $installed = (Get-ChildItem -LiteralPath $toolsetFolder -Directory | ForEach-Object { $_.Name }) -join ', '
        }
        if (-not (Test-Path -LiteralPath $defaultFile)) {
            Write-Pin 'MSVC' $false $msvcExpected "no file '$defaultFile'. Installed toolsets: $installed"
        }
        else {
            $msvc = (Get-Content -LiteralPath $defaultFile -Raw).Trim()
            Write-Pin 'MSVC' $msvc.StartsWith("$MsvcPin.") $msvcExpected "$msvc. Installed toolsets: $installed"
        }
    }
}

# The Windows SDK: the minimum of the Epic page.
$sdkExpected = "$WindowsSdkMinimum or later (the minimum of Epic, D-74)"
$sdkFolder = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\Include'
if (-not (Test-Path -LiteralPath $sdkFolder)) {
    Write-Pin 'Windows SDK' $false $sdkExpected "no folder '$sdkFolder'"
}
else {
    $sdkVersions = @(Get-ChildItem -LiteralPath $sdkFolder -Directory | ForEach-Object { $_.Name } | Where-Object { $_ -match '^\d+\.\d+\.\d+\.\d+$' } | ForEach-Object { [version]$_ } | Sort-Object)
    if ($sdkVersions.Count -eq 0) {
        Write-Pin 'Windows SDK' $false $sdkExpected "no version folder in '$sdkFolder'"
    }
    else {
        $newest = $sdkVersions[-1]
        Write-Pin 'Windows SDK' ($newest -ge $WindowsSdkMinimum) $sdkExpected "$newest. Installed: $($sdkVersions -join ', ')"
    }
}

# Unreal Engine (D-28). The variable names the folder that holds Engine (D-79).
$engineExpected = "$EnginePin (D-28)"
$engineFolder = [System.Environment]::GetEnvironmentVariable($EngineVariable)
if (-not $engineFolder) {
    Write-Pin 'Unreal Engine' $false $engineExpected "no engine: the variable $EngineVariable is not set (D-79)"
}
else {
    $buildVersion = Join-Path $engineFolder 'Engine\Build\Build.version'
    if (-not (Test-Path -LiteralPath $buildVersion)) {
        Write-Pin 'Unreal Engine' $false $engineExpected "no engine: no file '$buildVersion'. $EngineVariable names the folder that holds 'Engine'"
    }
    else {
        $build = Get-Content -LiteralPath $buildVersion -Raw | ConvertFrom-Json
        $engine = "$($build.MajorVersion).$($build.MinorVersion).$($build.PatchVersion)"
        $advice = ''
        if ($engine -ne $EnginePin) {
            $advice = 'Each hotfix upgrade is its own PR with build evidence (D-28).'
        }
        Write-Pin 'Unreal Engine' ($engine -eq $EnginePin) $engineExpected $engine $advice
    }
}

# Git LFS (D-30). No decision pins its version.
$lfsExpected = 'an install of any version (D-30)'
$lfs = $null
try {
    $lfs = (& git lfs version 2>&1 | Out-String).Trim()
    $lfsExit = $LASTEXITCODE
}
catch {
    $lfsExit = -1
    $lfs = $_.Exception.Message
}
if ($lfsExit -eq 0 -and $lfs.StartsWith('git-lfs/')) {
    Write-Pin 'Git LFS' $true $lfsExpected ($lfs -split ' ')[0].Substring('git-lfs/'.Length)
}
else {
    Write-Pin 'Git LFS' $false $lfsExpected "no Git LFS. 'git lfs version' failed: $lfs" 'Install Git for Windows with Git LFS, then run `git lfs install`.'
}

if ($script:Failed -eq 0) {
    Write-Output "toolchain-check: each of the $script:Total pins holds."
    exit 0
}

Write-Output "toolchain-check: $script:Failed of the $script:Total pins fail."
exit 1
