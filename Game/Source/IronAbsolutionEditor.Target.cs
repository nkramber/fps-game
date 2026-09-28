// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

using UnrealBuildTool;

public class IronAbsolutionEditorTarget : TargetRules
{
	public IronAbsolutionEditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("IronAbsolution");
	}
}
