// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

using UnrealBuildTool;

public class IronAbsolution : ModuleRules
{
	public IronAbsolution(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput" });

		// The automation test of the project settings reads the map settings (D-71).
		PrivateDependencyModuleNames.AddRange(new string[] { "EngineSettings" });
	}
}
