namespace IronAbsolution.Tools.ToolchainCheck;

/// <summary>
/// The pins of the Mac toolchain (D-28) and the variable that names the engine folder (D-79).
/// The PowerShell script of the Windows PC names the same engine pin and variable (D-72, D-74).
/// </summary>
public static class ToolchainPins
{
    /// <summary>The environment variable that names the engine folder, the folder that holds `Engine` (D-79). No commit holds that path (D-9).</summary>
    public const string EngineVariable = "IRON_ABSOLUTION_ENGINE_DIR";

    /// <summary>The version file of the engine, under the engine folder, with forward slashes.</summary>
    public const string BuildVersionPath = "Engine/Build/Build.version";

    /// <summary>Gets the one Xcode version that D-28 pins.</summary>
    public static ToolVersion Xcode { get; } = new ToolVersion(26, 1, 1);

    /// <summary>Gets the oldest Xcode that Unreal Engine 5.8 accepts (F-3).</summary>
    public static ToolVersion XcodeMinimum { get; } = new ToolVersion(26, 0, 0);

    /// <summary>Gets the first Xcode that does not work with Unreal Engine 5.8 (F-3). Each newer version fails too.</summary>
    public static ToolVersion XcodeFirstRefused { get; } = new ToolVersion(26, 4, 0);

    /// <summary>Gets the engine version that D-28 pins: Unreal Engine 5.8 at the hotfix 5.8.3.</summary>
    public static ToolVersion Engine { get; } = new ToolVersion(5, 8, 3);
}
