# Runbook: the engine setup of the Windows PC

Status: procedure, written 2026-09-27 in PR-8. Written in ASD-STE100 (D-17). The owner did the Windows steps on 2026-09-27 (D-78).

Correction of 2026-09-28, PR-14: the project supports Windows alone (D-91). PR-14 removed the Mac steps: the volume, Xcode, the Metal Toolchain, the cache path, and the start of the Mac editor (D-97). Git history keeps them.

This runbook installs the toolchain of the engine work on the Windows PC. The pins come from D-28 and D-74. Each step that needs the owner has the mark **Owner**, because it changes the computer. `docs/runbooks/session-context.md` gives the tools of a session: the .NET SDK, PowerShell 7, the Codex CLI, and the GitHub CLI.

The evidence of the pins, read on 2026-09-27:

- Unreal Engine 5.8.3 is the newest hotfix of 5.8 ([Epic forum](https://forums.unrealengine.com/t/5-8-3-hotfix-released/2833315)).
- The Visual Studio page of Epic for 5.8 gives Visual Studio 2026 18.0 or later for general work. Visual Studio 2022 17.14 or later is the other choice. The page gives MSVC 14.38 as the minimum and 14.50 as the recommendation. It gives the Windows SDK 10.0.22621.0 as the minimum and 10.0.26100 or later as the recommendation ([Epic](https://dev.epicgames.com/documentation/unreal-engine/setting-up-visual-studio-development-environment-for-cplusplus-projects-in-unreal-engine)).
- The file `Engine/Config/Windows/Windows_SDK.json` of the engine 5.8.3 gives the MSVC range that UnrealBuildTool prefers: 14.50.35717 to 14.50.99999. It bans 14.50.0 to 14.50.35722. The range reads the name of the toolset folder, and the ban reads the product version of `cl.exe` in that folder. A servicing update of `cl.exe` keeps the folder name. It suggests the component `Microsoft.VisualStudio.Component.VC.14.50.18.0.x86.x64` and its ATL component.
- MSVC 14.51 is the "Latest" toolset from Visual Studio 2026 18.6 on ([Microsoft](https://devblogs.microsoft.com/cppblog/msvc-version-1451-available/)). The engine does not prefer it, so the Windows PC needs 14.50 next to it.

## The pins

| Item | Pin | Decision |
|---|---|---|
| Unreal Engine | 5.8.3 | D-28 |
| Visual Studio | Visual Studio 2026, major version 18 | D-74 |
| MSVC | a 14.50 toolset with a `cl.exe` of 14.50.35723 or later, installed | D-74 |
| Windows SDK | 10.0.22621.0 or later | D-74 |
| Git LFS | any version | D-30 |
| Engine folder | `IRON_ABSOLUTION_ENGINE_DIR` | D-79 |

The variable names the folder that holds the `Engine` folder. No commit holds that path (D-9).

`run.ps1 toolchain-check` tests each pin (D-104). It prints one line for each pin, with the expected value and the found value. It gives the exit code 1 when a pin fails.

## Visual Studio 2026

1. **Owner.** Download Visual Studio 2026 from the [Visual Studio page](https://visualstudio.microsoft.com/downloads/).
2. **Owner.** In the installer, select these workloads: ".NET desktop development", "Desktop development with C++", ".NET Multi-platform App UI development", and "Game development with C++".
3. **Owner.** Under "Game development with C++", select "C++ profiling tools", "C++ AddressSanitizer", "Unreal Engine installer", and a Windows SDK of 10.0.26100 or later.
4. **Owner.** Under Individual components, search for `14.50`. Select "MSVC Build Tools v14.50 for x64/x86" and the ATL component of v14.50.
5. **Owner.** Keep the "Latest" toolset. The check reads each installed toolset, not the default.

## Git and Git LFS

1. **Owner.** Install Git for Windows from the [Git page](https://git-scm.com/download/win). Its installer includes Git LFS.
2. **Owner.** In PowerShell, add the hooks of Git LFS.

   ```
   git lfs install
   git lfs version
   ```

## Unreal Engine 5.8.3

1. **Owner.** Download the Epic Games Launcher from the [Epic download page](https://store.epicgames.com/download), and install it.
2. **Owner.** In the launcher, open Unreal Engine, then Library, then the plus sign next to Engine Versions.
3. **Owner.** Select `5.8.0` in the list. The launcher lists the minor versions and installs the newest hotfix.
4. **Owner.** Keep the default folder, or select another folder. Note the folder that holds `Engine`.
5. **Owner.** Start the install. The option "Editor symbols for debugging" is optional. The symbols give readable call stacks after a crash, and they use more disk.

CAUTION: The launcher installs each new hotfix without a question. Each hotfix upgrade is its own PR with build evidence (D-28). `run.ps1 toolchain-check` fails when the hotfix changes.

## The engine cache

The engine keeps its cache in the default place of the engine (D-100). No step of this runbook moves it, and no commit holds its path.

## The checkout

The build output of Unreal has deep folders, and a long root path can pass the path limit of 260 characters of Windows. So the checkout goes to a short path, such as a folder directly under a drive. The owner picks the path (D-100).

1. **Owner.** In PowerShell, clone the repository to a short path. Put the path in the command.

   ```
   git clone https://github.com/nkramber/iron-absolution.git <short path>
   cd <short path>
   .\run.ps1 hooks
   ```

2. For the work of a PR, get its branch and the LFS files of that branch.

   ```
   git fetch origin
   git switch <branch>
   git lfs pull
   ```

## The engine variable and the check

1. **Owner.** In PowerShell, set the variable for the user. Put the folder of the engine in the command. The launcher uses `C:\Program Files\Epic Games\UE_5.8` by default.

   ```
   [Environment]::SetEnvironmentVariable('IRON_ABSOLUTION_ENGINE_DIR', '<engine folder>', 'User')
   ```

2. Open a new PowerShell window, and run the check from the checkout.

   ```
   .\run.ps1 toolchain-check
   ```

3. Make sure that the last line says `each of the 5 pins holds`.

## The project build and the tests

Each engine target of `run.ps1` reads `IRON_ABSOLUTION_ENGINE_DIR` and finds the checkout from the folder of the script (D-99). No engine target runs in `verify`, because the hosted runners have no engine (D-31).

1. Build the editor target.

   ```
   .\run.ps1 editor-build
   ```

2. Run each automation test headless. The editor starts with no window.

   ```
   .\run.ps1 editor-test
   ```

3. Attach `Game\Saved\Logs\editor-build.log` and `Game\Saved\Logs\editor-test.log` to the evidence form of the PR (D-31).

The test target gives the exit code 1 when a check fails. A pass needs the exit code 0 of the editor, a test report with no failed test, and the success line of the log.

## The first start of the editor

The first start compiles the shaders of the engine, so it takes a long time. The editor opens a window, so a session asks the owner first (D-96).

1. Start the editor with the project.

   ```
   & "$env:IRON_ABSOLUTION_ENGINE_DIR\Engine\Binaries\Win64\UnrealEditor.exe" "$PWD\Game\IronAbsolution.uproject"
   ```

2. Open Edit, then Plugins, and find Enhanced Input. Make sure that the plugin is on, and note its label (F-6).
3. Close the editor.

## The package

The package is a Development build of the game, with the test map (D-89).

1. Make the package.

   ```
   .\run.ps1 package-build
   ```

2. Start the package for its timed run. The package opens a game window, so a session asks the owner first (D-96).

   ```
   .\run.ps1 package-run
   ```

3. Attach `Game\Saved\Logs\package-build.log` and `Game\Saved\Logs\package-run.log` to the evidence form of the PR (D-31).

The package runs the test map for 10 seconds, and stops. The start command gives the exit code 1 when a check fails. A pass needs the exit code 0 of the package and the success line of the timed run in `Game\Saved\Logs\package-run.log`.

## The frame-time capture

The capture measures M-3 in the package of the section above (D-137).

1. Close each other game and each heavy program. A game in the background makes the capture slower.
2. Start the capture. The package opens a borderless fullscreen window, so a session asks the owner first (D-96).

   ```
   .\run.ps1 frame-capture
   ```

3. Attach `Game\Saved\Logs\frame-capture.log` and `Game\Saved\Logs\frame-capture.csv` to the evidence form of the PR (D-31).

The package shows each view of the gym, and the CSV profiler records each frame. A pass needs the success line and the settings of D-137. It also needs a mean and a 99th percentile of the frame time at 8.33 ms or less (D-32).

## Traps

- The name of an MSVC toolset folder does not change after a servicing update. The folder 14.50.35717 can hold `cl.exe` 14.50.35739. The check reads `cl.exe`, as UnrealBuildTool does (F-22).
- `Build.bat` of the engine gives 0 for the result "up to date" of the build tool, so `run.ps1 editor-build` gives 0 when no file changed.
- The game is a program of the Windows subsystem, so its stdout is not its log. `run.ps1 package-run` gives the game the log path with `-abslog`.
- A PowerShell window that was open before the variable changed does not read the new value. Open a new window after step 1 of "The engine variable and the check".
