using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Bank-$90 bomb-jump movement handler `$E025-$E094`.
/// </summary>
/// <remarks>
/// A bomb explosion does not substitute a normal jump. Bank $A0 first publishes direction
/// left/straight/right from bomb-versus-Samus X, bank $91 locks pose input, and this special
/// handler owns only the rising part of the arc. Input returns shortly before the apex;
/// normal movement returns on a direction cancellation, downward turn, or collision.
/// The retained pose can be morphed, humanoid, or damaged rather than always a ball.
/// </remarks>
public static class SamusBombJumpMovement
{
    /// <summary>Runs `$90:E025`, which initializes velocity but deliberately moves zero pixels.</summary>
    public static BombJumpMovementResult Start(SamusState samus)
    {
        Ensure.NotNull(samus);
        if (!samus.BombJumpRequest.Accepted)
            throw new InvalidOperationException($"Bomb-jump start requires an accepted request, got {samus.BombJumpRequest}.");

        // `$90:9A2C` uses the same bottom-edge air/water/lava classification as ordinary
        // launch, indexing adjacent table words by zero/two/four. Gravity is refreshed by
        // frame-handler alpha, so this routine replaces only the launch speed/direction.
        samus.Kinematics.YSpeed =
            SamusVerticalMotion.BombJump(samus.LiquidPhysics.DetermineMovementMedium(samus));
        samus.Kinematics.YDirection = SamusVerticalDirection.Up;
        samus.BombJumpStarting = false;
        samus.BombJumpActive = true;
        return new BombJumpMovementResult(null);
    }

    /// <summary>Runs one `$90:E032` main-handler frame.</summary>
    internal static BombJumpMovementResult Step(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomPlms plms)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        Ensure.NotNull(plms);
        if (!samus.BombJumpActive)
            throw new InvalidOperationException("Bomb-jump main handler requires an active jump.");

        SamusBombJumpDirection direction = samus.BombJumpRequest.Direction;
        if (direction == SamusBombJumpDirection.None)
            return End(samus, null);

        BlockMoveResult? horizontal;
        if (direction != SamusBombJumpDirection.Straight)
        {
            // `$90:8EF4` bypasses the normal movement-type pointer and feeds `$90:9F25`
            // directly to the deceleration-allowed calculator. Direction one is left;
            // direction three is right. Existing base words intentionally participate.
            uint baseSpeed = samus.HorizontalSpeed.CalculateBaseSpeed(
                SamusStandaloneSpeedRecord.DiagonalBombJump);
            var displacement = SamusHorizontalDisplacement.Toward(direction == SamusBombJumpDirection.Left, samus, baseSpeed);
            horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = displacement.Displacement, CollisionMovementDirection = displacement.CollisionDirection });
            // The native direction-aware X mover clears momentum on a wall hit
            // before the later Y scan replaces its collision result. This must
            // not end the bomb ascent, but it must stop horizontal acceleration.
            if (horizontal.Value.Collided)
                samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
        }

        // `$90:8F1B` changes signed underflow to the falling direction. A diagonal jump
        // additionally selects mode two so normal horizontal movement decelerates after
        // this special handler relinquishes control.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Up &&
            unchecked((short)samus.Kinematics.YSpeed.Whole) < 0)
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

        // Native ends immediately once direction becomes down; it does not spend one
        // special-handler frame moving by a zero/negative vertical magnitude.
        if (samus.Kinematics.YDirection == SamusVerticalDirection.Down)
            return End(samus, null);

        (BlockMoveResult vertical, bool movedDown) = MoveUpWithGravity(

            level,
            samus,
            frameCounter,
            plms);
        // The native handler tests samus_collision_flag after moving Y. That move
        // replaces the horizontal result, so a shaft wall alone must not cancel the
        // upward bomb arc or return control to ordinary Morph Ball movement early.
        if (vertical.Collided)
            return End(samus, vertical);

        // At the apex the negated speed is non-negative, so `$90:915E` moves down; finding
        // no floor, `$90:E639` publishes the falling result for the pose pass.
        return new BombJumpMovementResult(vertical, FellWithoutFloor: movedDown);
    }

    /// <summary>Applies bomb-jump gravity and vertical block collision, reporting the apex's downward branch.</summary>
    private static BombJumpRise MoveUpWithGravity(

        RoomLevelData level,
        SamusState samus,
        int frameCounter,
        ISamusRoomPlms plms)
    {
        SamusKinematicsState state = samus.Kinematics;
        uint oldSpeed = state.VerticalSpeedFixed;
        uint acceleration = state.YAcceleration.RawValue;
        uint nextSpeed = unchecked(oldSpeed - acceleration);
        state.YSpeed = new UnsignedSixteenSixteen(unchecked((ushort)(SixteenSixteen.WholeOf(nextSpeed))), unchecked((ushort)nextSpeed));
        int displacement = SamusExtraDisplacement.AddToVerticalSpeedDisplacement(
            state,
            unchecked(-(int)oldSpeed));
        BlockMoveResult result = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, state, samus, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (frameCounter & 1) == 0 });
        if (result.Collided)
        {
            state.YSpeed = default;
            state.YDirection = SamusVerticalDirection.Down;
        }
        // $90:915C: a non-negative displacement, including the apex's zero, takes MoveSamus_Down.
        return new BombJumpRise(result, displacement >= 0);
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
