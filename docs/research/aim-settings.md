# Aim settings

Status: research, checked 2026-09-30 for PR-23. Written in ASD-STE100 (D-17).

Section 7.3 of `docs/roadmaps/phase-3-core-feel.md` asks PR-23 for the mouse sensitivity and the field of view, in the settings file of the user. This file records the Epic pages that the session read, the facts of the engine source, and the answers of the owner (D-139 to D-142).

## 1. Epic pages, read 2026-09-30

| Page | Version | Fact that PR-23 uses |
|---|---|---|
| [Enhanced Input](https://dev.epicgames.com/documentation/en-us/unreal-engine/enhanced-input-in-unreal-engine) | 5.8 | "Input Modifiers are pre-processors that alter the raw input values." The page gives no fact about the mouse sensitivity or the input scales of the engine. |
| [UGameUserSettings](https://dev.epicgames.com/documentation/en-us/unreal-engine/API/Runtime/Engine/GameFramework/UGameUserSettings) | 5.8 | The page gave no text to the session. Section 2 gives the facts from the engine source. |

## 2. Facts of the engine source, Unreal Engine 5.8.3

The turn of the view before PR-23:

- `BaseInput.ini` gives the axis properties of `MouseX`, `MouseY`, and `Mouse2D` a sensitivity of 0.07. The project copied that value into `DefaultInput.ini`.
- `EnhancedInputSubsystemInterface.cpp` changes the axis properties of a mouse key into modifiers. A sensitivity other than 1 adds a scalar modifier to the mapping.
- `BaseGame.ini` sets `InputYawScale=2.5` and `InputPitchScale=-2.5` on the player controller.
- `APlayerController::AddYawInput` and `AddPitchInput` multiply by these scales when `bEnableLegacyInputScales` is on. `InputSettings.cpp` turns it on by default.
- So one mouse count turned the view 0.07 times 2.5, which is 0.175 degrees. On the scale of D-139, this is a sensitivity of about 7.95.
- The pitch scale was negative, so the mapping context negated the Y axis of the mouse. A forward move of the mouse then turned the view up.

The turn of the view after PR-23:

- `DefaultInput.ini` sets the sensitivity of the three mouse axes to 1, and turns off `bEnableLegacyInputScales`.
- One mouse count reaches the character as 1, and the controller adds the degrees as they are. The character multiplies each count by 0.022 times the sensitivity (D-139).
- The pitch scale is now 1, so the mapping context has no modifier on the mouse. A forward move of the mouse gives a positive Y, and a positive pitch turns the view up.
- `APawn::AddControllerYawInput` sends input only to a local player controller. `APlayerController::IsLocalController` needs a local player when the world has no net driver. The automation tests call `SetAsLocalPlayerController` for this reason.

The settings file of the user:

- `GameUserSettingsClassName` in the section `/Script/Engine.Engine` of the engine config names the class of the settings object. `UEngine::CreateGameUserSettings` makes one object of that class, and loads it.
- The settings object reads the config category `GameUserSettings`. The project file `DefaultGameUserSettings.ini` is a layer of that category, under the file of the user. So a value that the user did not change comes from the project file.
- `SaveConfig` writes a value to the file of the user only when it differs from the layers below. A value equal to the project default stays out of the file.
- `UGameUserSettings::ValidateSettings` removes the whole category and loads it again when the version of the file is old. Then the project file gives each aim setting again.
- `UGameUserSettings::LoadSettings` is virtual, so the class of the project checks the bounds after each load.

## 3. The answers of the owner

- D-139: the scale of Quake and Source games, 0.022 degrees for each count times the sensitivity. The bounds are 0.1 to 20.
- D-140: no vertical invert, now or in phase 8. Revises D-126 in part.
- D-142: after the play test of the package, the default sensitivity is 2.0. Revises D-139 in part.
- D-141: a horizontal field of view with a default of 100 and bounds of 80 to 120. The setting of the player is the one source.

## 4. Choices of the implementation

- The bounds are rules, so C++ holds them. The defaults are in the project config (D-29).
- A setter or a console command can get a value out of bounds. Then an error line gives the name, the value, and the bounds, and the game keeps the last good value.
- A value out of bounds in the file of the user gives an error line at load. The game then uses the nearest bound, because at load it has no earlier good value.
- The views of the frame-time capture take the field of view of the project file, not the setting of the player. So M-3 stays comparable when the owner changes the setting (D-137).
