using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb-jump movement handler.
/// </summary>
/// <remarks>
/// A bomb explosion does not substitute a normal jump. Enemy collision first publishes direction
/// left/straight/right from bomb-versus-Samus X, Samus's pose handling locks pose input, and this special
/// handler owns only the rising part of the arc. Input returns shortly before the apex;
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
        // launch, indexing adjacent table entries by medium. Gravity is refreshed by
        // the input phase, so this launch replaces only the launch speed/direction.
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.BombJump(samus.LiquidPhysics.DetermineMovementMedium(samus));
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = true;
        return new BombJumpMovementResult(null);
    }

    /// <summary>Runs one main-handler frame.</summary>
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
            throw new InvalidOperationException("Bomb-jump main handler requires an active jump.");

        SamusBombJumpDirection direction = samus.BombJumpRequest.Direction;
        if (direction == SamusBombJumpDirection.None)
            return End(samus, null);

        BlockMoveResult? horizontal;
        if (direction != SamusBombJumpDirection.Straight)
        {
            // The bomb jump bypasses its movement's speeds and feeds its own speeds directly to the
            // deceleration-allowed calculator. Existing base speeds intentionally participate.
            UnsignedSixteenSixteen baseSpeed = samus.HorizontalSpeed.CalculateBaseSpeed(
                SamusHorizontalSpeedDefinitions.DiagonalBombJump);
            var displacement = SamusHorizontalDisplacement.Toward(direction == SamusBombJumpDirection.Left, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = displacement.Displacement, CollisionMovementDirection = displacement.CollisionDirection });
            // The direction-aware X mover clears momentum on a wall hit
            // before the later Y scan replaces its collision result. This must
            // not end the bomb ascent, but it must stop horizontal acceleration.
            if (horizontal.Value.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        // The jump turns a speed that passed zero into the falling direction. A diagonal jump
        // additionally selects mode two so normal horizontal movement decelerates after
        // the bomb jump hands her movement back to her pose.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            SamusSpeedMagnitude.HasPassedZero(samus.Kinematics.YSpeed.Whole))
        {
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
            if (direction != SamusBombJumpDirection.Straight)
                samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Decelerating;
        }
        else if (samus.Kinematics.YDirection == SamusVerticalDirection.Up && samus.Kinematics.YSpeed.Whole == 0)
        {
            // The rising helper restores input while speed is still fractional and
            // positive, BEFORE the apex or a collision restores normal movement.
            // A next-frame table match can therefore cancel the remaining bomb rise.
            samus.BombJumpPoseInputLocked = false;
        }

        // The game ends immediately once direction becomes down; it does not spend one
        // special-handler frame moving by a zero/negative vertical magnitude.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Down)
            return End(samus, null);

        (BlockMoveResult vertical, bool movedDown) = MoveUpWithGravity(

            level,
            samus,
            frameCounter,
            roomObjects);
        // The handler tests Samus's collision result after moving Y. That move
        // replaces the horizontal result, so a shaft wall alone must not cancel the
        // upward bomb arc or return control to ordinary Morph Ball movement early.
        if (vertical.Collided)
            return End(samus, vertical);

        // At the apex the negated speed is non-negative, so the vertical move moves down; finding
        // no floor, the falling check publishes the falling result for the pose pass.
        return new BombJumpMovementResult(vertical, FellWithoutFloor: movedDown);
    }

    /// <summary>Applies bomb-jump gravity and vertical block collision, reporting the apex's downward branch.</summary>
    private static BombJumpRise MoveUpWithGravity(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomObjects roomObjects)
    {
        SamusKinematicsState state = samus.Kinematics;
        UnsignedSixteenSixteen oldSpeed = state.YSpeed;
        state.YSpeed = oldSpeed - state.YAcceleration;
        SignedSixteenSixteen displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            -oldSpeed.AsDisplacement());
        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, state, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
        if (result.Collided)
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }
        // A non-negative displacement, including the apex's zero, moves Samus down.
        return new BombJumpRise(result, !displacement.IsNegative);
    }

    /// <summary>Clears bomb-jump direction, handler, and pose-input latches before normal movement resumes.</summary>
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
