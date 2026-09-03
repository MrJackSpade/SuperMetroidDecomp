namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$86 projectile half of the Draygon encounter: wall-turret shots and destroyable
/// goop. These occupy the shared eighteen-slot pool and execute the cartridge instruction
/// lists; they are not draw-only effects attached to the boss state.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private static readonly (ushort X, ushort Y)[] DraygonTurretPositions =
    [
        (0x0038, 0x00c0),
        (0x0034, 0x012f),
        (0x01cc, 0x0101),
    ];

    /// <summary>Ports <c>HandleFiringWallTurret</c> and initializer <c>$86:8D40</c>.</summary>
    private void SpawnDraygonWallTurret(
        DraygonEnemyState state,
        SamusState samus,
        ushort random)
    {
        int selection = random & 3;
        // InitAI_DraygonBody writes one to extra-enemy word $45, permanently suppressing
        // the fourth coordinate. The first three words remain zero until their PLM cannon
        // is destroyed; that destruction seam will later update the same typed flags.
        if (selection == 3 && state.BottomUnusedTurretDisabled)
            return;

        RoomEnemyProjectileSlot? turret = AllocateEnemyProjectile();
        if (turret is null)
            return;

        state.ProjectileSpeedParameter = 3;
        InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.DraygonWallTurret,
            graphicsIndex: 0x0a00);
        (turret.XPosition, turret.YPosition) = DraygonTurretPositions[selection];
        turret.XSubposition = 0;
        turret.YSubposition = 0;

        // PlaceAndAim negates the common zero-up angle, then adds $40 before retaining the
        // low byte. ConvertAngleToXy stores unsigned magnitudes; the pre-instruction applies
        // signs from this retained angle on every frame.
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - turret.XPosition)),
            unchecked((short)(samus.YPosition - turret.YPosition)));
        byte flightAngle = unchecked((byte)(-cartridgeAngle + 0x40));
        SetDraygonProjectileVelocity(turret, flightAngle, state.ProjectileSpeedParameter);

        // The definition says $8DFF, but init replaces it with RTS until the 84-frame muzzle
        // bloom reaches instruction $8CF6 and explicitly enables flight.
        turret.PreInstruction = EnemyProjectileCodePointers.RTS_868D54;
        state.WallTurretsSpawned++;
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
        goop.XPosition = unchecked((ushort)(state.Body.XPosition + (movingRight ? 24 : -28)));
        goop.YPosition = unchecked((ushort)(state.Body.YPosition - 16));
        goop.XSubposition = 0;
        goop.YSubposition = 0;

        ushort random = _nextRandom!();
        byte angle = unchecked((byte)((random & 0x003f) + (movingRight ? 0x00c0 : 0x0080)));
        // Retail never initializes parameter zero here. Preserve the inherited word most
        // recently published by a successful turret spawn—including zero if none existed.
        SetDraygonProjectileVelocity(goop, angle, state.ProjectileSpeedParameter);
        state.GoopProjectilesSpawned++;
    }

    private void SetDraygonProjectileVelocity(
        RoomEnemyProjectileSlot projectile,
        byte angle,
        ushort speed)
    {
        projectile.DirectionParameter = angle;
        int xMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x40);
        int yMagnitude = ReadUnsignedSineMagnitudeProduct(angle, speed, 0x80);
        projectile.XVelocity = unchecked((ushort)(xMagnitude >> 16));
        projectile.Variable0 = unchecked((ushort)xMagnitude);
        projectile.YVelocity = unchecked((ushort)(yMagnitude >> 16));
        projectile.Variable1 = unchecked((ushort)yMagnitude);
    }

    private static void RunDraygonProjectileFlight(RoomEnemyProjectileSlot projectile)
    {
        int xMagnitude = unchecked((projectile.XVelocity << 16) | projectile.Variable0);
        int yMagnitude = unchecked((projectile.YVelocity << 16) | projectile.Variable1);
        int xDisplacement = ((projectile.DirectionParameter + 64) & 0x80) != 0
            ? -xMagnitude
            : xMagnitude;
        int yDisplacement = ((projectile.DirectionParameter + 128) & 0x80) != 0
            ? -yMagnitude
            : yMagnitude;
        (projectile.XPosition, projectile.XSubposition) = AddDraygonProjectileFixed(
            projectile.XPosition,
            projectile.XSubposition,
            xDisplacement);
        (projectile.YPosition, projectile.YSubposition) = AddDraygonProjectileFixed(
            projectile.YPosition,
            projectile.YSubposition,
            yDisplacement);

        if (IsOutsideDraygonRoom(projectile))
            projectile.Clear();
    }

    private static void RunFlyingDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        RunDraygonProjectileFlight(goop);
        if (!goop.IsActive || samus is null)
            return;

        ushort xDistance = WrappedMagnitude(unchecked((ushort)(samus.XPosition - goop.XPosition)));
        ushort yDistance = WrappedMagnitude(unchecked((ushort)(samus.YPosition - goop.YPosition)));
        if (xDistance < 0x0010 && yDistance < 0x0014)
        {
            goop.InstructionPointer = EnemyProjectileInstructionLists.DraygonGoopAttached;
            goop.InstructionTimer = 1;
        }
    }

    private static void AttachDraygonGoopToSamus(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        if (samus is null)
            return;
        ushort nextDivisor = unchecked((ushort)(samus.XSpeedDivisor + 1));
        if (unchecked((short)(nextDivisor - 6)) >= 0)
            return;

        samus.XSpeedDivisor = nextDivisor;
        goop.Variable1 = nextDivisor;
        goop.Variable0 = 0x0100;
        goop.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_DraygonGoop_StuckToSamus;
        goop.BlocksSamusProjectiles = false;
        goop.CanDamageSamus = false;
        goop.PersistsOnSamusContact = true;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    private static void RunAttachedDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        if (samus is null)
            return;
        if (samus.HorizontalSpeed.ContactDamageIndex != 0)
        {
            RemoveAttachedDraygonGoop(goop, samus);
            return;
        }

        goop.XPosition = samus.XPosition;
        goop.YPosition = unchecked((ushort)(samus.YPosition + goop.Variable1 * 4 - 12));
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

    private static bool IsOutsideDraygonRoom(RoomEnemyProjectileSlot projectile) =>
        unchecked((short)projectile.XPosition) < 0 || projectile.XPosition >= 0x0200 ||
        unchecked((short)projectile.YPosition) < 0 || projectile.YPosition >= 0x0200;

    private static (ushort Position, ushort Subposition) AddDraygonProjectileFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (uint)displacement);
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }
}
