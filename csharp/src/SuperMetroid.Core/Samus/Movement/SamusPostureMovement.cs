using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Movement for ordinary/aimed crouching and the crouch/stand animation poses.
/// </summary>
/// <remarks>
/// These movements look stationary but are not no-ops: the game still publishes
/// total X speed, runs horizontal block/slope collision, and performs its downward grounding
/// probe every frame. Keeping those calls makes ledges, slopes, and future conveyors meet
/// the same seam as standing movement.
/// </remarks>
public static class SamusPostureMovement
{
    /// <summary>
    /// Runs crouching movement for the complete equal-radius crouch family,
    /// unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepCrouching(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        if (!SamusPoseMetadata.IsRightFacingCrouchingPose(samus.Pose) &&
            !SamusPoseMetadata.IsLeftFacingCrouchingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Crouching movement requires a crouching pose, not {samus.Pose}.");
        }

        GroundedMovementResult result = MoveWithZeroBaseSpeed(

            level,
            samus,
            frameCounter,
            roomObjects);

        // Performs this cleanup after both collision passes. It is not
        // inferred from being stationary; these are literal observable writes.
        // Unlike collision teardown, this does not cancel the boost.
        // The later input fallback decides whether the retained stage ends.
        samus.HorizontalSpeed.ClearHorizontalVelocity();
        return result;
    }

    /// <summary>
    /// Runs the crouch/stand subset of posture-transition movement: the crouching and
    /// standing transitions, unaimed or aimed.
    /// </summary>
    internal static GroundedMovementResult StepCrouchStandTransition(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects = null)
    {
        Validate(level, samus);
        if (!SamusPoseMetadata.IsCrouchStandTransitionPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Crouch/stand transition movement requires a crouching or standing transition pose, not {samus.Pose}.");
        }

        // All four corresponding entries in the posture cleanup table do nothing. Unlike movement type five,
        // the wrapper does not clear stored base momentum after moving. That distinction is
        // retained even though ordinary stand/crouch input reaches this code at zero speed.
        return MoveWithZeroBaseSpeed(level, samus, frameCounter, roomObjects);
    }

    /// <summary>Runs horizontal collision with zero base displacement, then the coupled downward grounding probe.</summary>
    private static GroundedMovementResult MoveWithZeroBaseSpeed(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects? roomObjects)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Does exactly two stores before tail-calling the ordinary horizontal
        // mover: it zeros the *base* displacement pair. It emphatically does
        // not require extra run speed to be zero. Horizontal movement subsequently
        // adds the extra run speed to that zero base. This is the retail
        // behavior that lets a stage-four run continue sliding during crouch transition
        // into a crouch; Samus's pose handling samples and stores the shine before normal crouching clears
        // that momentum. The old exception at this seam prevented that route.
        bool facingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left);
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft, samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // Uses the external-Y replacement when present. Otherwise it couples
        // the downward floor probe to the total X speed calculated by the horizontal pass;
        // this matters for a Speed-Booster slide through the crouching transition.
        SignedSixteenSixteen verticalDisplacement = SamusExtraDisplacement
            .CalculateNoSpeedVerticalDisplacement(samus.Kinematics, speed);
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = verticalDisplacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Requires the level and Samus state used by both posture handlers.</summary>
    private static void Validate(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
