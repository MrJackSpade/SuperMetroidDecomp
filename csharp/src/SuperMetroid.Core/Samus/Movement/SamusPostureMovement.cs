using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Movement for ordinary/aimed crouching and the crouch/stand animation poses.
/// </summary>
/// <remarks>
/// These movements look stationary but are not no-ops: the game still publishes
/// total X speed, runs horizontal block/slope collision, and performs its downward grounding
/// probe every frame. Keeping those calls makes ledges, slopes, and conveyors meet
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
        ISamusRoomObjects roomObjects)
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

        // The posture's cleanup runs after both collision passes, whether or not she is stationary; each cleared value is
        // observable.
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
        ISamusRoomObjects roomObjects)
    {
        Validate(level, samus);
        if (!SamusPoseMetadata.IsCrouchStandTransitionPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Crouch/stand transition movement requires a crouching or standing transition pose, not {samus.Pose}.");
        }

        // Unlike crouching movement, the four crouch and stand transitions keep Samus's base momentum after moving,
        // although ordinary standing and crouching input brings her here at zero speed.
        return MoveWithZeroBaseSpeed(level, samus, frameCounter, roomObjects);
    }

    /// <summary>Runs horizontal collision with zero base displacement, then the coupled downward grounding probe.</summary>
    private static GroundedMovementResult MoveWithZeroBaseSpeed(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        // Samus moves with no base speed, but keeps any extra run speed, which her horizontal movement adds to it.
        // A run at its full boost stage therefore keeps sliding through the crouch transition into a crouch, and
        // her pose handling stores the shine before ordinary crouching stops that momentum.
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        SamusTerrain terrain = new() { Level = level, RoomObjects = roomObjects };
        BlockMoveResult horizontal = SamusTerrainMoves.MoveStoppingAtWalls(terrain, samus, requestedHorizontal);

        // An external Y displacement, when present, replaces the floor probe. Otherwise the probe couples
        // its downward reach to the total X speed calculated by the horizontal pass;
        // this matters for a Speed-Booster slide through the crouching transition.
        BlockMoveResult vertical = SamusTerrainMoves.ProbeGroundWithoutSpeed(terrain, samus, frameCounter);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Requires the level and Samus state used by both posture movements.</summary>
    private static void Validate(
        RoomLevelData level,
        SamusState samus)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
    }
}
