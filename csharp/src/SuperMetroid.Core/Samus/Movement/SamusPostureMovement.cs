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
    /// Runs crouching movement for the poses of the crouching movement type, the crouch family unaimed or aimed and the
    /// X-ray scope's crouching poses alike: as the game's crouching movement does, it reads only the way the pose faces.
    /// </summary>
    /// <param name="movement">The crouch the movement dispatch selected.</param>
    /// <param name="terrain">The terrain Samus meets: its level and, in a room, the room objects its reacting blocks reach.</param>
    /// <param name="samus">Samus, crouching.</param>
    /// <param name="frameCounter">The update's frame counter, whose parity picks the scan's column order.</param>
    internal static GroundedMovementResult StepCrouching(
        SamusMovement.CrouchingMovement movement,
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Ensure.NotNull(movement);
        Validate(terrain.Level, samus);
        GroundedMovementResult result = MoveWithZeroBaseSpeed(
            terrain,
            samus,
            frameCounter);

        // The posture's cleanup runs after both collision passes, whether or not she is stationary; each cleared value is
        // observable.
        // Unlike collision teardown, this does not cancel the boost.
        // The later input fallback decides whether the retained stage ends.
        samus.HorizontalSpeed.ClearHorizontalVelocity();
        return result;
    }

    /// <summary>
    /// Runs the crouch/stand subset of posture-transition movement: the crouching and
    /// standing transitions, unaimed or aimed. It reads only the way the pose faces.
    /// </summary>
    internal static GroundedMovementResult StepCrouchStandTransition(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        Validate(terrain.Level, samus);

        // Unlike crouching movement, the four crouch and stand transitions keep Samus's base momentum after moving,
        // although ordinary standing and crouching input brings her here at zero speed.
        return MoveWithZeroBaseSpeed(terrain, samus, frameCounter);
    }

    /// <summary>Runs horizontal collision with zero base displacement, then the coupled downward grounding probe.</summary>
    private static GroundedMovementResult MoveWithZeroBaseSpeed(
        SamusTerrain terrain,
        SamusState samus,
        int frameCounter)
    {
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // Samus moves with no base speed, but keeps any extra run speed, which her horizontal movement adds to it.
        // A run at its full boost stage therefore keeps sliding through the crouch transition into a crouch, and
        // her pose handling stores the shine before ordinary crouching stops that momentum.
        var requestedHorizontal = SamusHorizontalDisplacement.TowardFacing(samus, baseSpeed: default);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));

        // An external Y displacement, when present, replaces the floor probe. Otherwise the probe couples
        // its downward reach to the total X speed calculated by the horizontal pass;
        // this matters for a Speed-Booster slide through the crouching transition.
        SignedSixteenSixteen verticalDisplacement = SamusExtraDisplacement
            .CalculateNoSpeedVerticalDisplacement(samus.Kinematics, speed);
        BlockMoveResult vertical = SamusBlockCollision.MoveVertical(terrain, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = verticalDisplacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
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
