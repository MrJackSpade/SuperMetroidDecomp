using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb jump's movement.
/// </summary>
/// <remarks>
/// A bomb explosion does not substitute a normal jump. Enemy collision first publishes direction
/// left/straight/right from bomb-versus-Samus X, Samus's pose handling locks pose input, and this special
/// movement owns only the rising part of the arc. Input returns shortly before the apex;
/// normal movement returns on a direction cancellation, downward turn, or collision.
/// The retained pose can be morphed, humanoid, or damaged rather than always a ball.
/// </remarks>
public static class SamusBombJumpMovement
{
    /// <summary>Runs the bomb jump's setup, which initializes velocity but deliberately moves zero pixels.</summary>
    public static BombJumpMovementResult Start(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.BombJumpRequest.Accepted)
            throw new InvalidOperationException($"Bomb-jump start requires an accepted request, got {samus.BombJumpRequest}.");

        // The launch uses the same bottom-edge air/water/lava classification as ordinary
        // launch, with a launch speed for each medium. Gravity is refreshed by
        // the input phase, so this launch replaces only the launch speed/direction.
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Rising(
            SamusVerticalMotion.BombJump(samus.LiquidPhysics.DetermineMovementMedium(samus)));
        samus.BombJumpStarting = false;
        samus.BombJumpActive = true;
        return new BombJumpMovementResult(null);
    }

    /// <summary>Runs one bomb-jump frame.</summary>
    internal static BombJumpMovementResult Step(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        Ensure.NotNull(roomObjects);
        if (!samus.BombJumpActive)
            throw new InvalidOperationException("Bomb-jump movement requires an active jump.");

        SamusBombJumpDirection direction = samus.BombJumpRequest.Direction;
        if (direction == SamusBombJumpDirection.None)
            return End(samus, null);

        BlockMoveResult? horizontal;
        if (direction != SamusBombJumpDirection.Straight)
        {
            // The bomb jump bypasses its movement's speeds and changes her base speed by its own, as a movement that
            // may slow her does. Existing base speeds intentionally participate.
            UnsignedSixteenSixteen baseSpeed = samus.HorizontalSpeed.CalculateBaseSpeed(
                SamusHorizontalSpeedDefinitions.DiagonalBombJump);
            var displacement = SamusHorizontalDisplacement.Toward(
                direction == SamusBombJumpDirection.Left ? HorizontalDirection.Left : HorizontalDirection.Right, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = displacement.Displacement, CollisionMovementDirection = displacement.CollisionDirection });
            // A wall stops her horizontal acceleration but does not end the rise.
            if (horizontal.Value.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        // The jump turns a speed that passed zero into the falling direction. A diagonal jump
        // additionally selects SamusAccelerationMode.Decelerating so normal horizontal movement decelerates after
        // the bomb jump hands her movement back to her pose.
        if (samus.Kinematics.VerticalVelocity.HasPassedApex)
        {
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
            if (direction != SamusBombJumpDirection.Straight)
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }
        else if (samus.Kinematics.VerticalVelocity is { IsRising: true, Speed.Whole: 0 })
        {
            // The rise gives her pose input back while her speed is still a fraction of a pixel upward, before the apex
            // or a collision gives her movement back, so the next update's pose input can end the rest of the rise.
            samus.BombJumpPoseInputLocked = false;
        }

        // The game ends immediately once direction becomes down; it does not spend one
        // bomb-jump frame moving by a zero/negative vertical magnitude.
        if (samus.Kinematics.VerticalVelocity.IsFalling)
            return End(samus, null);

        (BlockMoveResult vertical, bool movedDown) = MoveUpWithGravity(
            level,
            samus,
            frameCounter,
            roomObjects);
        // Only a ceiling ends the rise; a shaft wall does not, and she stays in the bomb jump.
        if (vertical.Collided)
            return End(samus, vertical);

        // At the apex the negated speed is non-negative, so the vertical move moves down; finding
        // no floor, the falling check publishes the falling result for the pose pass.
        return new BombJumpMovementResult(vertical, FellWithoutFloor: movedDown);
    }

    /// <summary>Applies bomb-jump gravity and vertical block collision, reporting whether she has passed the apex and moved down.</summary>
    private static BombJumpRise MoveUpWithGravity(
        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        // The rise moves by its OLD velocity before gravity slows it; it never falls here, so the terminal speed takes no
        // part.
        SamusKinematicsState state = samus.Kinematics;
        SignedSixteenSixteen rise = state.VerticalVelocity.Displacement;
        state.VerticalVelocity = state.VerticalVelocity.AfterGravity(state.YAcceleration, SamusVerticalMotionDefinitions.TerminalFallWholeSpeed);
        SignedSixteenSixteen displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(state, rise);
        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, samus.Pose, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
        if (result.Collided)
        {
            state.VerticalVelocity = SamusVerticalVelocity.FallFromRest;
        }
        // A non-negative displacement, including the apex's zero, moves Samus down.
        return new BombJumpRise(result, !displacement.IsNegative);
    }

    /// <summary>Ends the bomb jump and hands her movement and pose input back.</summary>
    private static BombJumpMovementResult End(
        SamusState samus,
        BlockMoveResult? vertical)
    {
        samus.BombJumpRequest = SamusBombJumpRequest.None;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = false;
        samus.BombJumpPoseInputLocked = false;
        return new BombJumpMovementResult(vertical);
    }

}
