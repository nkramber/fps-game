# Weapon and sound

Status: research, checked 2026-10-01 for PR-25. Written in ASD-STE100 (D-17).

Section 7.5 of `docs/roadmaps/phase-3-core-feel.md` asks the review to read the trace of the weapon against Unreal practice (D-34). This file records each Epic page, each fact of the engine source, and each fact of freesound.org that PR-25 uses.

## 1. Epic pages, read 2026-10-01

| Page | Version | Fact that PR-25 uses |
|---|---|---|
| [Add a Custom Object Type to Your Project](https://dev.epicgames.com/documentation/en-us/unreal-engine/add-a-custom-object-type-to-your-project-in-unreal-engine) | 5.8 | The collision page of the project settings adds a new trace channel with a name and a default response. A project can have up to 18 custom channels. |
| [Collision Response Reference](https://dev.epicgames.com/documentation/en-us/unreal-engine/collision-response-reference-in-unreal-engine) | 5.8 | The engine gives two trace channels: "Visibility" and "Camera". |

## 2. Facts of the engine source, Unreal Engine 5.8.3

- `EngineTypes.h` recommends a custom channel for a weapon: `#define COLLISION_WEAPON ECC_GameTraceChannel1`, with the name "Weapon" in `DefaultEngine.ini`. The same comment shows a trigger profile that ignores the channel "Weapon".
- `UCollisionProfile` reads the list `DefaultChannelResponses` from the section `[/Script/Engine.CollisionProfile]`. Each entry of `FCustomChannelSetup` has the channel, the default response, the trace type, the static object flag, and the name. The editor writes this form.
- `APawn::GetActorEyesViewPoint` gives `GetPawnViewLocation` and `GetViewRotation`. `APawn::GetViewRotation` gives the control rotation of the controller, or the rotation of the actor with no controller.
- `UCameraComponent::GetCameraView` reads `GetViewRotation` of the pawn when `bUsePawnControlRotation` is on and a local player controls the pawn. So an override of `GetViewRotation` turns the camera, the shot, and the trace of interact together.
- `APlayerCameraManager` sets `ViewPitchMin` to -89.9 and `ViewPitchMax` to 89.9.
- `FRandomStream::VRandCone` takes the angle from the axis through `Fmod`, so its points are not even over the cone. The weapon rule computes an even point of the spherical cap instead. The cosine of the angle is even from 1 to the cosine of the spread.
- The sound factory of the editor imports WAV, AIFF, OGG, FLAC, Opus, and MP3 files (`SoundFactory.cpp`).
- `USoundWave::Volume` is a linear multiplier of the volume of a sound asset, with the default 1.0.
- `UGameplayStatics::PlaySound2D` takes a start time into the sound. Its default `bIsUISound` is true, so a pause does not stop the sound. The weapon passes false.

## 3. The method of the shot

- A line trace for each pellet from the eye of the pawn, on the channel "Weapon" (`ECC_GameTraceChannel1`), with the default response "block" (D-130).
- The trace is complex, so it hits the triangles of a mesh, not its simple collision.
- The trace ignores the pawn that fires.
- The recoil adds a pitch and a random yaw to the view. Each shot sets a rate of return that brings the whole kick back in the recovery time. The control rotation does not change (D-159, D-161, D-162).

## 4. A measure of the sound files, 2026-10-01

The session decoded each OGG file with the Python package soundfile, and measured the RMS level in steps of 10 ms. The level is in dB below the full scale of the file.

| File | Level in the first 50 ms | Time to 10 percent of the peak |
|---|---|---|
| `rifle_shot.ogg` | -3 dB | 0 ms |
| `scatter_shot.ogg` | -57 dB, then about -1 dB from 56 ms | 56 ms |
| `target_hit.ogg` | -16 dB | 7 ms |
| `empty_click.ogg` | -10 dB, with its loud part from 40 ms | 6 ms |

The attack of the rifle shot is about 12 dB above the hit sound at the same moment, so the shot hides the hit (D-164, superseded by D-168). The scatter shot has 56 ms of silence at its start (D-165).

The mix of D-164 (superseded by D-168) did not make the hit clear. A measure of the energy by band showed the cause. The session took the spectrum of each file with a Hann window, and summed the energy in each band.

| File | Center of the energy | Below 250 Hz | 1 kHz to 4 kHz | 4 kHz to 8 kHz |
|---|---|---|---|---|
| `rifle_shot.ogg`, first 0.2 s | 624 Hz | 26 percent | 17 percent | 0.9 percent |
| `scatter_shot.ogg`, 0.05 s to 0.25 s | 1755 Hz | 16 percent | 26 percent | 10 percent |
| 570335, the first hit file | 41 Hz | 99.7 percent | 0.1 percent | 0 percent |
| 634690, the hit file of D-166 (superseded by D-167) | 2037 Hz | 0.1 percent | 99.8 percent | 0 percent |

The first hit file was a low thump in the band of the shots, and many speakers play 41 Hz weakly. The file of D-166 is a clean tone near 2 kHz. The owner then tried an original dry click, and removed the hit sound (D-167).

On 2026-10-02, the session measured the two shot files again for the mix. The rifle shot has a peak of 0.3 dB and an RMS level of -13.5 dB over its 1 s. The scatter shot has a peak of 1.4 dB and an RMS level of -13.0 dB from 0.05 s. The files have about the same level. The rifle fires 10 shots each second, so the tails of a burst overlap and sum. The mix of D-168 plays each rifle shot about 9 dB under a scatter shot.

## 5. Facts of freesound.org, read 2026-10-01

| Source | Fact |
|---|---|
| [Authentication](https://freesound.org/docs/api/authentication.html) | Token authentication gives the read-only resources. OAuth2 gives the other resources. |
| [Resources APIv2](https://freesound.org/docs/api/resources_apiv2.html) | The download of a sound in its original form needs OAuth2. The field `previews` of a sound gives `preview-hq-ogg`, an OGG file of about 192 kbps. |
| The API, `GET /apiv2/sounds/<id>/` | Each of the sounds 212601, 427595, 570335, 634690, and 725402 has the license `http://creativecommons.org/publicdomain/zero/1.0/`. |

The session used the token of the owner. The token stays out of the repository (G-6). `docs/game/provenance.md` holds the record of each file (D-38, D-155).

The description of each file states its source:

- 212601, "Machine Gun 001 - single shot" by pgi: "Made with Audacity from scratch."
- 427595, "20 gauge shotgun gunshot" by michorvath: "A 20 gauge shotgun being fired."
- 570335, "Hitmarker Sound Effect" by User391915396: "Made with some noise from a recording and sampled 5 tones higher." D-166 replaced it, and D-167 removed the hit sound.
- 634690, "Lil Pip" by adh.dreaming: "A very tiny sound." The description gives no other source. D-167 removed it.
- 725402, "A rifle being dry fired once" by serøutōnin--deprivəd: "made by dry firing a Ruger 10/22 rifle."
