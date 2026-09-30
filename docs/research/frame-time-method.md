# Frame-time method

Status: research, checked 2026-09-29 for PR-22. Written in ASD-STE100 (D-17).

Section 7.2 of `docs/roadmaps/phase-3-core-feel.md` asks PR-22 to read the Epic pages of the frame-time capture again, with a date. This file records each page, each fact of the engine source, and the method of M-3 that the owner chose (D-137).

## 1. Epic pages, read 2026-09-29

| Page | Version | Fact that PR-22 uses |
|---|---|---|
| [Build configurations reference](https://dev.epicgames.com/documentation/en-us/unreal-engine/build-configurations-reference-for-unreal-engine) | 5.8 | Test is "the Shipping configuration, but with some console commands, stats, and profiling tools enabled". Shipping "strips out console commands, stats, and profiling tools". |
| [Adjusting engine feature levels](https://dev.epicgames.com/documentation/en-us/unreal-engine/adjusting-engine-feature-levels?application_version=4.27) | 4.27 | "You should never profile or measure performance in Debug builds. For convenience, we suggest profiling and working on performance in Development build." |
| [CSV profiler](https://dev.epicgames.com/documentation/en-us/unreal-engine/csv-profiler?application_version=4.27) | 4.27 | The profiler "works on all consoles and also PC and in all builds except shipping". `-csvCaptureFrames=N` captures N frames from the start. The files go to `[ProjectDirectory]/Saved/Profiling/CSV`. |
| [Performance and profiling overview](https://dev.epicgames.com/documentation/en-us/unreal-engine/performance-and-profiling-overview?application_version=4.27) | 4.27 | "If you see a limit on 30 fps (~33.3ms) or 60 fps (~16.6ms), you likely have VSync enabled. For more accurate timing, it is better if you profile without it." |

The session found no 5.8 page of the CSV profiler. Section 2 gives the facts of the profiler from the engine source of 5.8.3.

## 2. Facts of the engine source, Unreal Engine 5.8.3

The CSV profiler:

- `CsvProfilerConfig.h` sets `CSV_PROFILER` to `WITH_ENGINE && (!UE_BUILD_SHIPPING || CSV_PROFILER_ENABLE_IN_SHIPPING)`, and the second flag is 0 by default. So a Development package has the profiler, and a Shipping package does not.
- `FCsvProfiler::BeginCapture` takes a number of frames, a folder, and a file name. The value -1 captures until `EndCapture`. The profiler adds `.csv` to the name.
- `EndCapture` gives a future of the file name. The profiler writes the file on its own thread, so the game waits for the future before it stops.
- The file has a header row, one row for each frame, the header row again, and one line of metadata pairs such as `[config],Development`. The first column holds the events of the frame.
- `SetMetadata` writes each key in lower case, and it changes each comma of a value to `&#44;`. The command line comes last, with its commas.
- The stat `FrameTime` is the time between the ends of two frames on the game thread, in milliseconds.
- The value -1 of `csv.CompressionMode` compresses a file only when the caller asks for it. The capture does not ask, so the file is plain text.

The engine install and the start of the game:

- The launcher engine 5.8.3 of D-28 holds the game builds of DebugGame, Development, and Shipping alone. A Test package needs an engine that the owner builds from source.
- `UGameUserSettings::GetDefaultWindowMode` gives `WindowedFullscreen`, with the comment "WindowedFullscreen should be the general default for games". `GetDefaultResolution` gives 0 by 0, and `DetermineGameWindowResolution` then uses the size of the monitor. So the game runs borderless fullscreen at the desktop size with no project setting (D-138).
- The game reads `FullscreenMode` from the user settings only when the key `Version` matches the engine.
- `UEngine::LoadMap` spawns the player and starts play before it sends `PostLoadMapWithWorld`. So the capture finds the player controller in that event.
- `ACameraActor` constrains the aspect ratio of its camera. The view of the player does not, so `AFrameTimeView` turns the constraint off.

## 3. The PC of the capture

The Windows PC of D-32 has an Intel Core i9-13900K, an NVIDIA GeForce RTX 4090, and 32 GB of memory. On 2026-09-29, Windows gave the display of the RTX 4090 as 2560x1440 at 239 Hz. So borderless fullscreen gives the 1440p of D-32 on this PC.

## 4. The method of M-3

The owner chose the method from the options of the session (D-137). `run.ps1 frame-capture` does these steps:

1. Start the Development package of `run.ps1 package-build` on the gym, with the full path of the CSV file.
2. Use no window option, so the game runs borderless fullscreen at the desktop size (D-138).
3. After the map loads, turn off VSync, the smooth frame rate, and the frame-rate cap.
4. Show view 1 of the gym for a warm-up of 5 seconds, with no capture.
5. Start the capture, and show each of the 4 views for 5 seconds.
6. Write the settings of the run in the metadata, and end the capture.
7. Read the file, and compare the mean and the 99th percentile of `FrameTime` with 8.33 ms (D-32).

The 99th percentile uses the nearest rank: the value at rank ceiling(0.99 × n) of the sorted frame times. A capture needs 100 frames or more. With fewer frames, this rank gives the slowest frame.

The command fails when the metadata does not show a Development package, borderless fullscreen at 2560x1440, VSync off, and no frame-rate cap. So a value of M-3 always comes from the settings of D-137.

## 5. The captures of 2026-09-29

Both captures used the Development package of commit `13bdbb1` on the Windows PC.

| Capture | Frames | Mean | 99th percentile | Mean of the GPU | Result |
|---|---|---|---|---|---|
| 1, with another game open | 2,721 | 7.36 ms | 10.16 ms | 6.49 ms | Void |
| 2, with no other game | 6,004 | 3.33 ms | 3.67 ms | 1.96 ms | Pass, the first value of M-3 |

The owner had another game open during capture 1, so capture 1 is void. It shows that a program in the background changes the value very much. So a session asks the owner to close each other game before a capture.

In capture 2, each view has a mean of 3.33 ms. The render thread has a mean of 3.33 ms, the game thread 1.14 ms, and the GPU 1.96 ms. So the render thread sets the pace of the gym.

The files of the two captures showed two more facts of the CSV profiler:

- A row has fewer fields than the closing header when a stat first appears later in the capture. Capture 2 has rows of 288, 290, and 291 fields. The closing header holds each column, and it starts with the first header.
- The place of the column `FrameTime` changes from one capture to the next: column 49 in capture 1, and column 185 in capture 2. So the command finds the column by its name.
