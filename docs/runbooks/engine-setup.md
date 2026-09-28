# Runbook: the engine setup of the Mac and the Windows PC

Status: procedure, written 2026-09-27 in PR-8. Written in ASD-STE100 (D-17). The owner did the Mac steps on 2026-09-27 (D-78).

This runbook installs the toolchain of phase 1 on both computers. The pins come from D-28 and D-74. Each step that needs the owner has the mark **Owner**. The owner does each step of this runbook, because each one changes a computer (D-33).

The evidence of the pins, read on 2026-09-27:

- Unreal Engine 5.8.3 is the newest hotfix of 5.8 ([Epic forum](https://forums.unrealengine.com/t/5-8-3-hotfix-released/2833315)).
- The macOS page of Epic for 5.8 gives Xcode 26.0 as the minimum and 26.1.1 as the recommendation. It says "Xcode 26.4 is not compatible with Unreal Engine" ([Epic](https://dev.epicgames.com/documentation/unreal-engine/macos-development-requirements-for-unreal-engine)).
- The Visual Studio page of Epic for 5.8 gives Visual Studio 2026 18.0 or later for general work. Visual Studio 2022 17.14 or later is the other choice. The page gives MSVC 14.38 as the minimum and 14.50 as the recommendation. It gives the Windows SDK 10.0.22621.0 as the minimum and 10.0.26100 or later as the recommendation ([Epic](https://dev.epicgames.com/documentation/unreal-engine/setting-up-visual-studio-development-environment-for-cplusplus-projects-in-unreal-engine)).
- The file `Engine/Config/Windows/Windows_SDK.json` of the engine 5.8.3 gives the MSVC range that UnrealBuildTool prefers: 14.50.35717 to 14.50.99999. It bans 14.50.0 to 14.50.35722. The range reads the name of the toolset folder, and the ban reads the product version of `cl.exe` in that folder. A servicing update of `cl.exe` keeps the folder name. It suggests the component `Microsoft.VisualStudio.Component.VC.14.50.18.0.x86.x64` and its ATL component.
- MSVC 14.51 is the "Latest" toolset from Visual Studio 2026 18.6 on ([Microsoft](https://devblogs.microsoft.com/cppblog/msvc-version-1451-available/)). The engine does not prefer it, so the Windows PC needs 14.50 next to it.
- Unreal Engine does not start from a case-sensitive file system on macOS (F-21, [Epic forum](https://forums.unrealengine.com/t/help-epic-games-launcher-unreal-engine-does-not-support-running-from-case-sensitive-file-systems/2021754)).

## The pins

| Item | Mac | Windows PC | Decision |
|---|---|---|---|
| Unreal Engine | 5.8.3 | 5.8.3 | D-28 |
| Xcode | 26.1.1, never 26.4 or later | none | D-28 |
| Visual Studio | none | Visual Studio 2026, major version 18 | D-74 |
| MSVC | none | a 14.50 toolset with a `cl.exe` of 14.50.35723 or later, installed | D-74 |
| Windows SDK | none | 10.0.22621.0 or later | D-74 |
| Git LFS | any version | any version | D-30 |
| Metal Toolchain | installed | none | D-87 |
| Engine folder | `IRON_ABSOLUTION_ENGINE_DIR` | `IRON_ABSOLUTION_ENGINE_DIR` | D-79 |

The variable names the folder that holds the `Engine` folder. No commit holds that path (D-9).

`make toolchain-check` tests each pin of the Mac. `scripts/toolchain-check.ps1` tests each pin of the Windows PC (D-72). Each check prints one line for each pin, with the expected value and the found value.

## The Mac

The project SSD is case-sensitive APFS, and Unreal Engine does not start from it (F-21). A second volume in the same APFS container is case-insensitive. The engine, its cache, Xcode, and the checkout of the engine work go on that volume (D-81). The volume shares the free space of the container, and the step changes no file on the first volume.

### The volume

1. **Owner.** Find the APFS container of the SSD.

   ```
   diskutil info /Volumes/SSD-1TB | grep 'APFS Container'
   ```

2. **Owner.** Add the case-insensitive volume to that container. Put the container id in place of `disk7`.

   ```
   diskutil apfs addVolume disk7 APFS IronAbsolution
   diskutil info /Volumes/IronAbsolution | grep Personality
   ```

3. Make sure that the last line says `APFS`, with no `Case-sensitive`.

### Xcode 26.1.1

1. **Owner.** Download `Xcode_26.1.1_Apple_silicon.xip` from the [Apple download page](https://developer.apple.com/download/all/?q=Xcode%2026.1.1).
2. **Owner.** Expand the archive on the volume.

   ```
   mkdir -p /Volumes/IronAbsolution/Applications
   cd /Volumes/IronAbsolution/Applications
   xip --expand ~/Downloads/Xcode_26.1.1_Apple_silicon.xip
   find . -maxdepth 2 -name Xcode.app
   ```

3. **Owner.** Move the app that `find` gives to `Xcode-26.1.1.app`, and remove the empty folder.

   ```
   mv ./<folder>/Xcode.app Xcode-26.1.1.app
   rmdir ./<folder>
   ```

4. **Owner.** Select the app, accept the license, and add the Metal toolchain.

   ```
   sudo xcode-select -s /Volumes/IronAbsolution/Applications/Xcode-26.1.1.app
   sudo xcodebuild -license accept
   sudo xcodebuild -runFirstLaunch
   xcodebuild -downloadComponent MetalToolchain
   xcodebuild -version
   ```

5. Make sure that the last command prints `Xcode 26.1.1` and `Build version 17B100`.
6. Make sure that Xcode shows the Metal Toolchain as installed. The editor compiles no shader without it (F-24).

   ```
   xcodebuild -showComponent MetalToolchain
   ```

7. Make sure that the output has the line `Status: installed`.

CAUTION: Do not accept an update to Xcode 26.4 or later. Unreal Engine 5.8 does not work with it (F-3).

### Git LFS

1. **Owner.** Install Git LFS, and add its hooks to the Git settings of the user.

   ```
   brew install git-lfs
   git lfs install
   git lfs version
   ```

### Unreal Engine 5.8.3

1. **Owner.** Download the Epic Games Launcher from the [Epic download page](https://store.epicgames.com/download). Install it in `/Applications`.
2. **Owner.** In the launcher, open Unreal Engine, then Library, then the plus sign next to Engine Versions.
3. **Owner.** Select `5.8.0` in the list. The launcher lists the minor versions and installs the newest hotfix.
4. **Owner.** Set the install folder to `/Volumes/IronAbsolution/Epic Games`. Keep the Mac target platform.
5. **Owner.** Start the install. The option "Editor symbols for debugging" is optional. The symbols give readable call stacks after a crash, and they use more disk.

CAUTION: The launcher installs each new hotfix without a question. Each hotfix upgrade is its own PR with build evidence (D-28). `make toolchain-check` fails when the hotfix changes.

### The engine variable

1. **Owner.** Add the variable to the shell settings of the user, then load them again.

   ```
   echo 'export IRON_ABSOLUTION_ENGINE_DIR="/Volumes/IronAbsolution/Epic Games/UE_5.8"' >> ~/.zshrc
   source ~/.zshrc
   ```

2. Run the check from the root of the checkout.

   ```
   make toolchain-check
   ```

3. Make sure that the last line says `each of the 4 pins holds`.

### The engine cache

The engine keeps its cache in a Zen server. By default, the Zen data goes to the user folder on the internal disk. The internal disk has too little space for it (F-4). The editor setting "Local DDC Path" moves the cache (D-83). The file `BaseEngine.ini` of 5.8.3 names that setting in its section `[Zen.AutoLaunch]`, and Zen uses the `Zen` folder in that path.

The editor starts Zen during its own startup, before Editor Preferences can open (F-23). So the owner writes the setting before the first start of the editor (D-85). The editor keeps the setting in the file `KeyValueStore.ini` of the user.

1. **Owner.** Before the first start of the editor, make the cache folder.

   ```
   mkdir -p /Volumes/IronAbsolution/DerivedDataCache
   ```

2. **Owner.** Write the setting into the file of the editor. The command writes no second copy of the section.

   ```
   store="$HOME/Library/Application Support/Epic/Epic Games/KeyValueStore.ini"
   mkdir -p "$(dirname "$store")"
   grep -q '^\[GlobalDataCachePath\]' "$store" 2>/dev/null || printf '\n[GlobalDataCachePath]\nUE-LocalDataCachePath=/Volumes/IronAbsolution/DerivedDataCache\n' >> "$store"
   grep -A1 GlobalDataCachePath "$store"
   ```

3. Make sure that the last command prints the path of step 1.

At the first start, the owner makes sure that Editor Preferences shows the path. "The first start of the editor" below gives that step.

### The checkout of the engine work

The Unreal project of PR-9 builds from a case-insensitive volume too (D-81).

1. **Owner.** Clone the repository to the volume, and install the hook.

   ```
   git clone git@github.com:nkramber/iron-absolution.git /Volumes/IronAbsolution/iron-absolution
   cd /Volumes/IronAbsolution/iron-absolution
   make hooks
   ```

### The first start of the editor

The first start compiles the shaders of the engine, so it takes a long time. Do the steps of "The engine cache" first (D-85).

1. **Owner.** Build the editor target from the root of the checkout.

   ```
   make editor-build
   ```

2. **Owner.** Start the editor with the project.

   ```
   "$IRON_ABSOLUTION_ENGINE_DIR/Engine/Binaries/Mac/UnrealEditor.app/Contents/MacOS/UnrealEditor" "$PWD/Game/IronAbsolution.uproject"
   ```

3. **Owner.** Open Edit, then Editor Preferences, then General, then Global. Make sure that "Local DDC Path" shows `/Volumes/IronAbsolution/DerivedDataCache`.
4. **Owner.** Open Edit, then Plugins, and find Enhanced Input. Make sure that the plugin is on, and note its label (F-6).
5. **Owner.** Close the editor.

Exit test 8 of PR-9 checks that the internal disk holds no cache data. These folders are on the internal disk:

- `~/Library/Application Support/Epic/Zen`: the default data folder of Zen. It stays empty when the setting holds.
- `~/.epic/UnrealBuildAccelerator`: the store of the build accelerator. A build with no remote agent writes almost nothing to it.

### The package

The package is a Development build of the game, with the test map (D-89).

1. **Owner.** Make the package from the root of the checkout.

   ```
   make package-build
   ```

2. **Owner.** Start the package for its timed run.

   ```
   make package-run
   ```

The package opens a window, runs the test map for 10 seconds, and stops. The start command gives the exit code 1 when a check fails. A pass needs the exit code 0 of the package and the success line of the timed run in `Game/Saved/Logs/package-run.log`.

## The Windows PC

The owner runs each step on the Windows PC and posts the output in the PR (D-33).

### Visual Studio 2026

1. **Owner.** Download Visual Studio 2026 from the [Visual Studio page](https://visualstudio.microsoft.com/downloads/).
2. **Owner.** In the installer, select these workloads: ".NET desktop development", "Desktop development with C++", ".NET Multi-platform App UI development", and "Game development with C++".
3. **Owner.** Under "Game development with C++", select "C++ profiling tools", "C++ AddressSanitizer", "Unreal Engine installer", and a Windows SDK of 10.0.26100 or later.
4. **Owner.** Under Individual components, search for `14.50`. Select "MSVC Build Tools v14.50 for x64/x86" and the ATL component of v14.50.
5. **Owner.** Keep the "Latest" toolset. The check reads each installed toolset, not the default.

### Git and Git LFS

1. **Owner.** Install Git for Windows from the [Git page](https://git-scm.com/download/win). Its installer includes Git LFS.
2. **Owner.** In PowerShell, add the hooks of Git LFS.

   ```
   git lfs install
   git lfs version
   ```

### Unreal Engine 5.8.3

1. **Owner.** Install the Epic Games Launcher, and install Unreal Engine `5.8.0` from Library, as on the Mac.
2. **Owner.** Keep the default folder, or select another folder. Note the folder that holds `Engine`.

### The engine variable and the check

1. **Owner.** In PowerShell, set the variable for the user. Put the folder of the engine in the command.

   ```
   [Environment]::SetEnvironmentVariable('IRON_ABSOLUTION_ENGINE_DIR', 'C:\Program Files\Epic Games\UE_5.8', 'User')
   ```

2. **Owner.** Open a new PowerShell window, and run the check from the root of the checkout.

   ```
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\toolchain-check.ps1
   ```

3. **Owner.** Post the full output in the PR (D-33).

### The checkout

The build output of Unreal has deep folders, and a long root path can pass the path limit of 260 characters of Windows. So the checkout goes to a short path. The owner uses `C:\dev\iron-absolution`.

1. **Owner.** In PowerShell, clone the repository to a short path.

   ```
   git clone https://github.com/nkramber/iron-absolution.git C:\dev\iron-absolution
   cd C:\dev\iron-absolution
   ```

2. **Owner.** For the work of a PR, get its branch and the LFS files of that branch.

   ```
   git fetch origin
   git switch <branch>
   git lfs pull
   ```

### The project build and the tests

Each script matches a Makefile target of the Mac (D-72). Each script reads `IRON_ABSOLUTION_ENGINE_DIR` and finds the checkout from its own folder.

1. **Owner.** In PowerShell, build the editor target from the root of the checkout.

   ```
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\editor-build.ps1
   ```

2. **Owner.** Run each automation test headless.

   ```
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\editor-test.ps1
   ```

3. **Owner.** Post the output of each script in the PR (D-33). Attach `Game\Saved\Logs\editor-build.log` and `Game\Saved\Logs\editor-test.log`.

The test script gives the exit code 1 when a check fails. A pass needs the exit code 0 of the editor, a test report with no failed test, and the success line of the log.

### The package

1. **Owner.** In PowerShell, make the package from the root of the checkout.

   ```
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\package-build.ps1
   ```

2. **Owner.** Start the package for its timed run.

   ```
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\package-run.ps1
   ```

3. **Owner.** Post the output of each script in the PR (D-33). Attach `Game\Saved\Logs\package-build.log` and `Game\Saved\Logs\package-run.log`.

The package opens a window, runs the test map for 10 seconds, and stops. The start script gives the exit code 1 when a check fails.

## Traps

- On an external volume, `xip` can leave `Xcode.app` in a temporary folder with a UUID name. Find the app with `find`, and do not expect `Xcode.app` in the current folder.
- `xcodebuild` fails with "requires Xcode" when the active folder is the Command Line Tools. Select the app with `xcode-select -s`.
- The name of an MSVC toolset folder does not change after a servicing update. The folder 14.50.35717 can hold `cl.exe` 14.50.35739. The check reads `cl.exe`, as UnrealBuildTool does (F-22).
- A program from the Dock or the Finder does not read `~/.zshrc`. The check and the Makefile targets run from the shell, so they read the variable.
- The first clone on `/Volumes/SSD-1TB` has the same folder name. The editor does not find the project there, so run `pwd` before each engine command.
- `Build.sh` of the engine gives 0 for the result "up to date" of the build tool, so `make editor-build` gives 0 when no file changed.
- On the Mac, a package with no `-package` step holds no libraries, and it stops at its start with "Library not loaded" (F-26). `make package-build` has the step.
- The Mac package runs in the App Sandbox. It cannot write a log outside its container, and `-abslog` fails with no error. So `make package-run` reads the log from stdout (F-27).
- Windows PowerShell 5.1 writes `package-build.log` in UTF-16. PowerShell 7 writes UTF-8. Both logs are complete.
