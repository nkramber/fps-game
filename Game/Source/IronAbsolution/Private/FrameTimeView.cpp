// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#include "FrameTimeView.h"

#include "Camera/CameraComponent.h"

AFrameTimeView::AFrameTimeView()
{
	// A camera actor draws black bars when the screen has another aspect than its own. The camera
	// of the player draws none, so a view of the capture draws none too.
	GetCameraComponent()->bConstrainAspectRatio = false;
}
