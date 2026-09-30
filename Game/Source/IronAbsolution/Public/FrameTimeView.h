// Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.

#pragma once

#include "CoreMinimal.h"
#include "Camera/CameraActor.h"

#include "FrameTimeView.generated.h"

/**
 * One view of the frame-time capture of M-3 (D-137). The content script places each view in the
 * gym. The capture shows the views in the order of Order, and holds each one for a set time.
 * The camera of the actor gives the field of view, so each view can match the view of the player.
 */
UCLASS()
class IRONABSOLUTION_API AFrameTimeView : public ACameraActor
{
	GENERATED_BODY()

public:
	AFrameTimeView();

	/** The place of this view in the capture. Each view of a map has its own value, from 1 up. */
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Frame time", meta = (ClampMin = "1"))
	int32 Order = 0;
};
