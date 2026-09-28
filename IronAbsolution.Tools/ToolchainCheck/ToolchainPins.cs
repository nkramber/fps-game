using System;

namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The pins of the Windows toolchain (D-28, D-74) and the variable that names the engine folder
/// (D-79). The Visual Studio, MSVC, and Windows SDK values come from the Visual Studio page of
/// Epic for Unreal Engine 5.8, read on 2026-09-27 (D-74):
/// https://dev.epicgames.com/documentation/unreal-engine/setting-up-visual-studio-development-environment-for-cplusplus-projects-in-unreal-engine
/// </summary>
public static class ToolchainPins
{
    /// <summary>The environment variable that names the engine folder, the folder that holds `Engine` (D-79). No commit holds that path (D-9).</summary>
    public const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";

    /// <summary>The environment variable of the 32-bit program folder of Windows, which holds vswhere and the Windows SDK.</summary>
    public const string ProgramFilesX86Variable = "ProgramFiles(x86)";

    /// <summary>The version file of the engine, under the engine folder, with forward slashes.</summary>
    public const string BuildVersionPath = "Engine/Build/Build.version";

    /// <summary>The program that finds each install of Visual Studio, under the 32-bit program folder. The Visual Studio Installer ships it.</summary>
    public const string VswherePath = "Microsoft Visual Studio/Installer/vswhere.exe";

    /// <summary>The component of the C++ tools. vswhere gives only an install that holds it.</summary>
    public const string CppToolsComponent = "Microsoft.VisualStudio.Component.VC.Tools.x86.x64";

    /// <summary>The folder of the MSVC toolsets, under the install folder of Visual Studio.</summary>
    public const string MsvcFolder = "VC/Tools/MSVC";

    /// <summary>The x64 compiler, under the folder of one toolset.</summary>
    public const string CompilerPath = "bin/Hostx64/x64/cl.exe";

    /// <summary>The folder of the headers of each Windows SDK, under the 32-bit program folder. Each subfolder name is one SDK version.</summary>
    public const string WindowsSdkFolder = "Windows Kits/10/Include";

    /// <summary>The major version of Visual Studio 2026 (D-74).</summary>
    public const int VisualStudioMajor = 18;

    /// <summary>
    /// Gets the oldest cl.exe that UnrealBuildTool of 5.8.3 accepts in the MSVC family 14.50.
    /// `Engine/Config/Windows/Windows_SDK.json` of 5.8.3 bans 14.50.0 to 14.50.35722. The ban reads
    /// the product version of cl.exe, not the folder name, because a servicing update changes
    /// cl.exe and keeps the folder name (MicrosoftPlatformSDK.cs, IsValidToolChainDirMSVC).
    /// </summary>
    public static Version MsvcMinimum { get; } = new Version(14, 50, 35723);

    /// <summary>
    /// Gets the first cl.exe of the next MSVC family. Visual Studio can install a newer default
    /// toolset, such as 14.51, and the build tool takes a toolset of the preferred family first.
    /// So the check reads each installed toolset, not the default.
    /// </summary>
    public static Version MsvcNextFamily { get; } = new Version(14, 51, 0);

    /// <summary>Gets the oldest Windows SDK of the Epic page (D-74).</summary>
    public static Version WindowsSdkMinimum { get; } = new Version(10, 0, 22621, 0);

    /// <summary>Gets the engine version that D-28 pins: Unreal Engine 5.8 at the hotfix 5.8.3.</summary>
    public static Version Engine { get; } = new Version(5, 8, 3);
}
