using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile half of the Draygon encounter: wall-turret shots and destroyable
/// goop. These occupy the shared eighteen-slot pool and execute the cartridge instruction
/// lists; they are not draw-only effects attached to the boss state.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports <c>HandleFiringWallTurret</c> and initializer <c>$86:8D40</c>.</summary>
    private void SpawnDraygonWallTurret(
        DraygonEnemyState state,
        SamusState samus,
        ushort random)
    {
        DraygonCannonTarget target = DraygonCannonData.FiringTarget(
            ClosedNativeWords.Decode<DraygonFiringCannon>((byte)(random & 3), "Draygon firing cannon"));
        if (state.DisabledCannonWords.Contains((ushort)target.DisabledWord))
            return;

        RoomEnemyProjectileSlot? turret = AllocateEnemyProjectile();
        if (turret is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.DraygonWallTurret,
            graphicsIndex: EnemyPaletteBits.Palette5);
        turret.Position = FixedPosition<RoomPixels>.FromWhole(target.Origin);

        // PlaceAndAim negates the common zero-up angle, then adds $40 before retaining the
        // low byte. ConvertAngleToXy stores unsigned magnitudes; the pre-instruction applies
        // signs from this retained angle on every frame.
        byte cartridgeAngle = CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - turret.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - turret.Position.Y.Whole))));
        byte flightAngle = unchecked((byte)(-cartridgeAngle + 0x40));
        SetDraygonProjectileVelocity(turret, flightAngle, DraygonProjectileSpeeds.WallTurret);

        // The definition says $8DFF, but init replaces it with RTS until the 84-frame muzzle
        // bloom reaches instruction $8CF6 and explicitly enables flight.
        turret.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_868D54;
        state.WallTurretsSpawned++;
    }

    /// <summary>
    /// Implements bank $84's write through <c>PLM_Variable</c>. Cannon setup stores one of
    /// Draygon's extra-enemy WRAM addresses, then the damage instruction writes one here.
    /// </summary>
    public void DisableDraygonCannon(ushort variablePointer)
    {
        if (!DraygonCannonData.IsControlWord(variablePointer))
        {
            throw new InvalidDataException(
                $"Draygon cannon PLM targeted unrecognized WRAM word ${variablePointer:X4}.");
        }

        // Native writes WRAM, not an actor reference. The defeated room still has
        // cannon PLMs even though its enemy population no longer includes Draygon.
        EnsureLoaded();
        new DraygonCannonMemory(EnemyMemoryBus).Disable((DraygonCannonControlWord)variablePointer);
        // Keep the existing live-actor projection synchronized when one exists.
        Draygon?.DisabledCannonWords.Add(variablePointer);
    }

    /// <summary>Ports the two body instruction producers at <c>$A5:9F7C/9FAE</c>.</summary>
    private void SpawnDraygonGoop(DraygonEnemyState state, bool movingRight)
    {
        RoomEnemyProjectileSlot? goop = AllocateEnemyProjectile();
        if (goop is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            graphicsIndex: 0x0400);
        goop.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(state.Body.Position.X.Whole + (movingRight ? 24 : -28))), unchecked((ushort)(state.Body.Position.Y.Whole - 16))));

        ushort random = EnemyRandom.Generate();
        byte angle = unchecked((byte)((random & 0x003f) + (movingRight ? 0x00c0 : 0x0080)));
        SetDraygonProjectileVelocity(goop, angle, DraygonProjectileSpeeds.Goop);
        state.GoopProjectilesSpawned++;
    }

    private static void SetDraygonProjectileVelocity(
        RoomEnemyProjectileSlot projectile,
        byte angle,
        ushort speed)
    {
        projectile.DirectionParameter = angle;
        int xMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x40);
        int yMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x80);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(SnesFixedPosition.WholeOf(xMagnitude)));
        projectile.Variable0 = unchecked((ushort)xMagnitude);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(SnesFixedPosition.WholeOf(yMagnitude)));
        projectile.Variable1 = unchecked((ushort)yMagnitude);
    }

    private static void RunDraygonProjectileFlight(RoomEnemyProjectileSlot projectile)
    {
        int xMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.X.RawValue, projectile.Variable0)));
        int yMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.Y.RawValue, projectile.Variable1)));
        int xDisplacement = ((projectile.DirectionParameter + 64) & 0x80) != 0
            ? -xMagnitude
            : xMagnitude;
        int yDisplacement = ((projectile.DirectionParameter + 128) & 0x80) != 0
            ? -yMagnitude
            : yMagnitude;
        projectile.Position = projectile.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        projectile.Position = projectile.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(yDisplacement));

        if (IsOutsideDraygonRoom(projectile))
            projectile.Clear();
    }

    private void RunFlyingDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8E0F: a power-bombed goop loses its ID but the routine still moves it and
        // tests Samus; only the room-boundary branch ends it early.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        bool releasedByPowerBomb = !goop.IsActive;
        RunDraygonProjectileFlight(goop);
        if (!releasedByPowerBomb && !goop.IsActive || samus is null)
            return;

        ushort xDistance = WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - goop.Position.X.Whole)));
        ushort yDistance = WrappedMagnitude(unchecked((ushort)(samus.Position.Y.Whole - goop.Position.Y.Whole)));
        if (xDistance < 0x0010 && yDistance < 0x0014)
        {
            goop.InstructionPointer = DraygonProjectileInstructionProgramDefinitions.GoopTouch;
            goop.InstructionTimer = 1;
        }
    }

    private void AttachDraygonGoopToSamus(
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
        goop.Variable0 = 0x0100;
        goop.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus;
        goop.BlocksSamusProjectiles = false;
        goop.CanDamageSamus = false;
        goop.PersistsOnSamusContact = true;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    private void RunAttachedDraygonGoop(
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
        goop.Variable0 = unchecked((ushort)(goop.Variable0 - 1));
        if (goop.Variable0 == 0)
            RemoveAttachedDraygonGoop(goop, samus);
    }

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
    private void DeleteEnemyProjectileIfPowerBombed(RoomEnemyProjectileSlot projectile, SamusState? samus)
    {
        // A standalone projectile pass has no bomb owner, which is the radius-zero case.
        if (_audioPowerBomb is not { } powerBomb)
            return;
        int horizontalRadius = SnesLittleEndian.HighByte(powerBomb.ExplosionRadius);
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

    private static bool IsOutsideDraygonRoom(RoomEnemyProjectileSlot projectile) =>
        unchecked((short)projectile.Position.X.Whole) < 0 || projectile.Position.X.Whole >= 0x0200 ||
        unchecked((short)projectile.Position.Y.Whole) < 0 || projectile.Position.Y.Whole >= 0x0200;

}
