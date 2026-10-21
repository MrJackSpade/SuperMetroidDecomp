using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's goop passes and the goop and turret shots as projectiles.</summary>
[RoomLifetime]
internal sealed class DraygonGoop
{
    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues encounter sound effects and music through the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Applies common death and drop behavior after Draygon's special sequence.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Draygon's body, eye and tail: their setup, programs and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>The wall turrets' firing cadence during Draygon's fight.</summary>
    private readonly DraygonTurrets _turrets;
    /// <summary>How Draygon moves along an angle or horizontally, and the breath bubbles it trails.</summary>
    private readonly DraygonMovement _movement;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonGoop(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops, DraygonParts parts, DraygonTurrets turrets, DraygonMovement movement)
    {
        _room = room;
        _audio = audio;
        _deathsAndDrops = deathsAndDrops;
        _parts = parts;
        _turrets = turrets;
        _movement = movement;
    }

    /// <summary>Ports the two mirrored goop setup functions at $A5:8B0A/$8C8E.</summary>
    internal void SetupDraygonGoopPass(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState? samus,
        byte frameCounterLow)
    {
        // Only the rightward setup calls HandleFiringWallTurret; the leftward entry at
        // $8C8E omits it. That asymmetry changes the random seed and is therefore material.
        if (direction == HorizontalDirection.Right)
            _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(new(direction == HorizontalDirection.Right ? DraygonBehavior.DraygonOffscreenLeftX : state.RightSideReset.X, 384));
        state.SwoopVelocity = SnesSixteenSixteen.FromWhole(1u);
        state.GoopYOscillationAngle = SnesAngle.Zero;
        // Retail's leftward setup briefly requests the right-facing apex arm list before
        // the body reset opcode replaces all four lists. Preserve the literal write.
        _parts.StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingRightNearSwoopApex);
        state.Function = direction == HorizontalDirection.Right
            ? DraygonAiFunction.GoopRight
            : DraygonAiFunction.GoopLeft;
        _parts.StartDraygonProgram(
            body,
            direction == HorizontalDirection.Right
                ? DraygonProgram.BodyFacingRightReset
                : DraygonProgram.BodyFacingLeftReset);
        state.FacingRight = direction == HorizontalDirection.Right;
    }

    /// <summary>Ports the oscillating approach at $A5:8B52/$8CD4.</summary>
    internal void ApproachSamusForDraygonGoop(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState? samus,
        byte frameCounterLow)
    {
        _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot position a goop pass without Samus.");

        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(state.Body.Position.X.Whole - samus.Position.X.Whole))) < 208)
        {
            state.Function = direction == HorizontalDirection.Right
                ? DraygonAiFunction.GoopRightTail
                : DraygonAiFunction.GoopLeftTail;
            state.GoopCounter = NativeCountdown.Start(16);
            return;
        }
        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
    }

    /// <summary>Ports the firing and off-screen selection at $A5:8BAE/$8D30.</summary>
    internal void FireDraygonGoop(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState? samus,
        byte frameCounterLow)
    {
        // The right-facing function calls the turret handler; the left-facing copy does not.
        if (direction == HorizontalDirection.Right)
            _turrets.ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus?.XSpeedDivisor != 0)
        {
            state.Function = DraygonAiFunction.TryGrabSamus;
            return;
        }

        int random = _room.Random.Read();
        if ((random & 15) == 0)
        {
            state.GoopCounter = state.GoopCounter.Decrement().Remaining;
            if (state.GoopCounter.IsZero)
            {
                FinishDraygonGoopFiring(state, direction == HorizontalDirection.Right);
                return;
            }
            _parts.StartDraygonProgram(
                state.Body,
                direction == HorizontalDirection.Right
                    ? DraygonProgram.BodyFacingRightFireGoop
                    : DraygonProgram.BodyFacingLeftFireGoop);
        }

        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
        ushort x = state.Body.Position.X.Whole;
        bool crossedExit = direction == HorizontalDirection.Right
            ? unchecked((short)x) >= 0 && x >= 672
            : unchecked((short)x) < 0 && unchecked((short)(x - DraygonBehavior.DraygonOffscreenLeftX)) < 0;
        if (crossedExit)
            FinishDraygonGoopFiring(state, direction == HorizontalDirection.Right);
    }

    /// <summary>Places Draygon offscreen and selects the next swoop after a goop pass exits.</summary>
    private void FinishDraygonGoopFiring(DraygonEnemyState state, bool movingRight)
    {
        _parts.StartDraygonProgram(
            state.Arms,
            movingRight
                ? DraygonProgram.ArmsFacingRightIdle
                : DraygonProgram.ArmsFacingLeftIdle);
        state.Function = movingRight
            ? DraygonAiFunction.GoopRightRecovery
            : DraygonAiFunction.GoopLeftRecovery;
    }

    /// <summary>Ports the final off-screen movement at $A5:8C33/$8DB2.</summary>
    internal void ExitDraygonGoopPass(
        DraygonEnemyState state,
        HorizontalDirection direction,
        SamusState? samus = null)
    {
        _movement.SpawnPeriodicDraygonBreathBubble(state);
        // Only the leftward recovery rechecks whether Samus became gooped after firing.
        if (direction != HorizontalDirection.Right && samus?.XSpeedDivisor != 0)
        {
            state.Function = DraygonAiFunction.TryGrabSamus;
            return;
        }

        MoveDraygonAlongGoopPath(state, direction == HorizontalDirection.Right);
        ushort x = state.Body.Position.X.Whole;
        bool offScreen = direction == HorizontalDirection.Right
            ? unchecked((short)x) >= 0 && x >= 672
            : unchecked((short)x) < 0 && unchecked((short)(x - DraygonBehavior.DraygonOffscreenLeftX)) < 0;
        if (!offScreen)
            return;

        state.SwoopOffset = state.SwoopEndOffset;
        if (direction == HorizontalDirection.Right)
        {
            state.Function = DraygonAiFunction.SwoopLeftSetup;
            state.Body.Position = state.Body.Position.WithWholeX(592);
        }
        else
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.Body.Position = state.Body.Position.WithWholeX(DraygonBehavior.DraygonOffscreenLeftX);
        }
        state.Body.Position = state.Body.Position.WithWholeY(DraygonBehavior.DraygonOffscreenLeftX);
    }

    /// <summary>Applies the goop pass's horizontal motion and one-step Y oscillation.</summary>
    internal static void MoveDraygonAlongGoopPath(DraygonEnemyState state, bool movingRight)
    {
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(
            384 + EnemyMath.ReadEightBitCosineProduct(state.GoopYOscillationAngle.ToByteAngle(), 32))));
        state.GoopYOscillationAngle = state.GoopYOscillationAngle.AddTableUnits(1);
        DraygonMovement.MoveDraygonHorizontally(state, movingRight);
    }

    /// <summary>Registers goop flight and attachment instructions with the projectile interpreter.</summary>
    internal void RegisterDraygonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunAttachedDraygonGoop(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus);

        table.RegisterPreInstruction(context =>
        {
            // $86:8DFF deletes a power-bombed shot, then still moves the released slot.
            DeleteEnemyProjectileIfPowerBombed(context.Projectile, context.Samus);
            MoveDraygonProjectile(context.Projectile);
            DeleteDraygonProjectileOutsideRoom(context.Projectile);
        }, EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired);

        table.RegisterPreInstruction(context =>
        {
            RunFlyingDraygonGoop(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.DraygonGoop);

        // A shot wall turret runs the shared misc-dust small explosion, the list its definition names.
        table.RegisterShotAnimation(RoomEnemyProjectileKind.DraygonWallTurret,
            () => new MiscDustProjectileAnimation(MiscDustProjectileProgram.SmallExplosion));
        table.RegisterTouchAnimation(RoomEnemyProjectileKind.DraygonGoop,
            () => new DraygonProjectileAnimation(DraygonProjectileProgram.GoopTouch));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.DraygonGoop,
            () => new DraygonProjectileAnimation(DraygonProjectileProgram.GoopShot));
        table.RegisterCue<DraygonProjectileCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            SamusState? samus = context.Samus;
            switch (cue)
            {
                case DraygonProjectileCue.StickToSamus:
                    AttachDraygonGoopToSamus(projectile, samus);
                    return;
                case DraygonProjectileCue.SpawnDrops:
                    // The drop is allocated before the deletion releases this projectile, preserving slot order.
                    _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(projectile.Position.Whole, EnemyDefinitionId.DraygonEye);
                    return;
                case DraygonProjectileCue.Delete:
                    projectile.Clear();
                    return;
                case DraygonProjectileCue.Fire:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired;
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DraygonProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>Moves a Draygon projectile in 16.16 space by its angle and speed.</summary>
    internal static void MoveDraygonProjectile(RoomEnemyProjectileSlot projectile)
    {
        int xMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.X.RawValue, checked((ushort)projectile.Variable0))));
        int yMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.Y.RawValue, checked((ushort)projectile.Variable1))));
        int xDisplacement = EnemyMath.CosineIsNegative(new SnesByteAngle(unchecked((byte)(projectile.DirectionParameter)))) ? -xMagnitude : xMagnitude;
        int yDisplacement = EnemyMath.SineIsNegative(new SnesByteAngle(unchecked((byte)(projectile.DirectionParameter)))) ? -yMagnitude : yMagnitude;
        projectile.Position = projectile.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        projectile.Position = projectile.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(yDisplacement));
    }

    /// <summary>Deletes a projectile that has left Draygon's room (<c>$86:E722</c>).</summary>
    private static void DeleteDraygonProjectileOutsideRoom(RoomEnemyProjectileSlot projectile)
    {
        if (IsOutsideDraygonRoom(projectile))
            projectile.Clear();
    }

    /// <summary>Advances airborne goop, including power-bomb deletion and Samus contact setup.</summary>
    internal void RunFlyingDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8E0F: a power-bombed goop loses its ID but the routine still moves it and
        // tests Samus. Contact is tested before the room boundary, so a goop that touches
        // Samus outside the boundary still runs its touch list instead of being deleted.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        MoveDraygonProjectile(goop);
        if (samus is null)
        {
            DeleteDraygonProjectileOutsideRoom(goop);
            return;
        }

        ushort xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - goop.Position.X.Whole)));
        ushort yDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.Y.Whole - goop.Position.Y.Whole)));
        if (xDistance < 16 && yDistance < 20)
        {
            goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.GoopTouch));
            return;
        }
        DeleteDraygonProjectileOutsideRoom(goop);
    }

    /// <summary>Attaches goop to Samus, increasing her X-speed divisor until it expires.</summary>
    internal void AttachDraygonGoopToSamus(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8D99 runs the power-bomb deletion first, then attaches regardless.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        ushort nextDivisor = unchecked((ushort)(samus.XSpeedDivisor + 1));
        if (unchecked((short)(nextDivisor - 6)) >= 0)
            return;

        samus.XSpeedDivisor = nextDivisor;
        goop.Variable1 = nextDivisor;
        goop.Variable0 = 256;
        goop.PreInstruction =
            EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus;
        goop.BlocksSamusProjectiles = false;
        goop.CanDamageSamus = false;
        goop.PersistsOnSamusContact = true;
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        samus.KnockbackTimer = NativeCountdown.Start(0);
    }

    /// <summary>Follows Samus with attached goop and counts down its removal timer.</summary>
    internal void RunAttachedDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8DCA: the power-bomb deletion does not end this pre-instruction.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            RemoveAttachedDraygonGoop(goop, samus);
            return;
        }

        goop.Position = goop.Position.WithWhole(new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + goop.Variable1 * 4 - 12))));
        // The attachment lifetime counts down in variable zero (DEC then BNE): the goop falls off when it reaches zero.
        NativeWordCounterStep lifetime = NativeCountdown.FromRaw(checked((ushort)goop.Variable0)).Decrement();
        goop.Variable0 = lifetime.Remaining.RawValue;
        if (lifetime.IsZero)
            RemoveAttachedDraygonGoop(goop, samus);
    }

    /// <summary>Releases one attached goop projectile and restores one X-speed divisor step.</summary>
    private static void RemoveAttachedDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState samus)
    {
        goop.Clear();
        samus.XSpeedDivisor = samus.XSpeedDivisor == 0
            ? (ushort)0
            : unchecked((ushort)(samus.XSpeedDivisor - 1));
    }

    /// <summary>
    /// Ports <c>Delete_EnemyProjectile_IfPowerBombed</c> at $86:8D5C for the Draygon goop
    /// and wall-turret shot. Inside the explosion's ellipse it clears the projectile ID and
    /// <c>XSpeedDivisor</c> only; the calling routine continues on the released slot.
    /// </summary>
    internal void DeleteEnemyProjectileIfPowerBombed(RoomEnemyProjectileSlot projectile, SamusState? samus)
    {
        // A standalone projectile pass has no bomb owner, which is the radius-zero case.
        if (_audio.PowerBomb is not { } powerBomb)
            return;
        int horizontalRadius = powerBomb.ExplosionRadius.WholePixels;
        if (horizontalRadius == 0)
            return;
        // `LSR; ADC $12; LSR` carries the first shift's low bit into the addition.
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + (horizontalRadius & 1)) >> 1;
        int xDistance = Math.Abs(unchecked((short)(powerBomb.Position.X - projectile.Position.X.Whole)));
        int yDistance = Math.Abs(unchecked((short)(powerBomb.Position.Y - projectile.Position.Y.Whole)));
        if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
            return;
        projectile.ReleaseIdentityOnly();
        if (samus is not null)
            samus.XSpeedDivisor = 0;
    }

    /// <summary>Tests the projectile against the 512-by-512-pixel Draygon arena bounds.</summary>
    private static bool IsOutsideDraygonRoom(RoomEnemyProjectileSlot projectile) =>
        unchecked((short)projectile.Position.X.Whole) < 0 || projectile.Position.X.Whole >= 512 ||
        unchecked((short)projectile.Position.Y.Whole) < 0 || projectile.Position.Y.Whole >= 512;
}
