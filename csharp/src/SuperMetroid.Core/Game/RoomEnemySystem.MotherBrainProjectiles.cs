using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain room turret and bullet translation from <c>$86:BE4F-$C1B7</c>. These are
/// deliberately part of the common eighteen-slot bank-$86 pool: the twelve permanent
/// turrets leave only six slots for bullets, dust, and other actors, exactly as on SNES.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainTurretGraphicsIndex = 0x0400;
    private const ushort MotherBrainTurretInitialListTable = 0xbeb9;
    private const ushort MotherBrainTurretAllowedRotationPointerTable = 0xbec9;
    private const ushort MotherBrainTurretDirectionTable = 0xbee1;
    private const ushort MotherBrainTurretXPositionTable = 0xbe89;
    private const ushort MotherBrainTurretYPositionTable = 0xbea1;
    private const ushort MotherBrainTurretRuntimeListTable = 0xc040;
    private const ushort MotherBrainTurretBulletXOffsetTable = 0xbf9f;
    private const ushort MotherBrainTurretBulletYOffsetTable = 0xbfaf;
    private const ushort MotherBrainTurretBulletXVelocityTable = 0xbfbf;
    private const ushort MotherBrainTurretBulletYVelocityTable = 0xbfcf;

    /// <summary>
    /// Ports the twelve calls to <c>SpawnEprojWithRoomGfx($C17E, A=0..$B)</c> made by
    /// <c>InitAI_MotherBrainBody</c>. Allocation scans from native index $22 down, so request
    /// zero lands in host slot 17 and request $B lands in slot 6.
    /// </summary>
    private void SpawnMotherBrainInitialTurrets()
    {
        for (ushort parameter = 0; parameter < 12; parameter++)
            SpawnMotherBrainTurret(parameter);
    }

    /// <summary>Ports <c>InitAI_EnemyProjectile_MotherBrainsTurrets</c> at $86:BE4F.</summary>
    private void SpawnMotherBrainTurret(ushort parameter)
    {
        if (parameter >= 12)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        RoomEnemyProjectileSlot? turret = AllocateEnemyProjectile();
        if (turret is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.MotherBrainRoomTurret,
            MotherBrainTurretGraphicsIndex);

        int tableOffset = parameter * 2;
        ushort direction = ReadMotherBrainProjectileWord(
            unchecked((ushort)(MotherBrainTurretDirectionTable + tableOffset)));
        turret.DirectionParameter = parameter;
        turret.XPosition = ReadMotherBrainProjectileWord(
            unchecked((ushort)(MotherBrainTurretXPositionTable + tableOffset)));
        turret.YPosition = ReadMotherBrainProjectileWord(
            unchecked((ushort)(MotherBrainTurretYPositionTable + tableOffset)));

        // The turret is stationary, so the initializer intentionally repurposes both
        // subposition words. X-subposition is a bank-$86 pointer to this turret's allowed
        // direction bytes. Y-subposition packs direction in the low byte and signed
        // rotation delta (+1 initially) in the high byte.
        turret.XSubposition = ReadMotherBrainProjectileWord(unchecked((ushort)(
            MotherBrainTurretAllowedRotationPointerTable + tableOffset)));
        turret.YSubposition = unchecked((ushort)(0x0100 | direction));
        turret.InstructionPointer = ReadMotherBrainProjectileWord(unchecked((ushort)(
            MotherBrainTurretInitialListTable + direction * 2)));
        turret.InstructionTimer = 1;

        // X/Y velocity are likewise timers for a stationary turret. Each initializer makes
        // two independent calls to the cartridge RNG and clamps their low bytes upward.
        ResetMotherBrainTurretRotationTimer(turret);
        ResetMotherBrainTurretCooldown(turret);
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_MotherBrainsTurrets</c>.</summary>
    private void RunMotherBrainTurretPreInstruction(
        RoomEnemyProjectileSlot turret,
        ushort cameraX,
        ushort cameraY)
    {
        MotherBrainEnemyState state = _motherBrain ??
            throw new InvalidDataException("A Mother Brain turret has no owning encounter state.");
        bool onScreen = MotherBrainTurretIsOnScreen(turret, cameraX, cameraY);

        // Off-screen turrets deliberately freeze both timers until scrolled into view. Once
        // the shared deletion flag is set they disappear silently off-screen, but visible
        // turrets replace themselves with dust animation parameter $C.
        if (!onScreen)
        {
            if (state.DeleteTurretsAndRinkas)
                turret.Clear();
            return;
        }
        if (state.DeleteTurretsAndRinkas)
        {
            ushort x = turret.XPosition;
            ushort y = turret.YPosition;
            turret.Clear();
            SpawnRoomGraphicsDustExplosion(x, y, animationIndex: 0x000c);
            return;
        }

        turret.XVelocity = unchecked((ushort)(turret.XVelocity - 1));
        if (turret.XVelocity == 0)
        {
            ResetMotherBrainTurretRotationTimer(turret);
            SelectNextMotherBrainTurretDirection(turret);
            ushort direction = unchecked((byte)turret.YSubposition);
            turret.InstructionPointer = ReadMotherBrainProjectileWord(unchecked((ushort)(
                MotherBrainTurretRuntimeListTable + direction * 2)));
            turret.InstructionTimer = 1;
        }

        turret.YVelocity = unchecked((ushort)(turret.YVelocity - 1));
        if (turret.YVelocity == 0)
        {
            ResetMotherBrainTurretCooldown(turret);
            SpawnMotherBrainTurretBullet(turret);
        }
    }

    /// <summary>Ports the signed direction-delta and allowed-rotation test at $86:C050.</summary>
    private void SelectNextMotherBrainTurretDirection(RoomEnemyProjectileSlot turret)
    {
        byte currentDirection = unchecked((byte)turret.YSubposition);
        sbyte rotationDelta = unchecked((sbyte)(turret.YSubposition >> 8));
        byte candidate = unchecked((byte)((currentDirection + rotationDelta) & 7));
        byte allowed = _bus!.ReadByte(0x860000 | unchecked((ushort)(
            turret.XSubposition + candidate)));

        if (allowed != 0)
        {
            turret.YSubposition = unchecked((ushort)(
                (turret.YSubposition & 0xff00) | candidate));
            return;
        }

        // A disallowed next step reverses the signed high-byte delta and immediately takes
        // one step in the new direction. The cartridge does not perform a second mask test.
        rotationDelta = unchecked((sbyte)-rotationDelta);
        byte reversedDirection = unchecked((byte)(currentDirection + rotationDelta));
        turret.YSubposition = unchecked((ushort)(
            (unchecked((byte)rotationDelta) << 8) | reversedDirection));
    }

    private void ResetMotherBrainTurretRotationTimer(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)_nextRandom!());
        turret.XVelocity = Math.Max(sample, (ushort)0x0020);
    }

    private void ResetMotherBrainTurretCooldown(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)_nextRandom!());
        turret.YVelocity = Math.Max(sample, (ushort)0x0080);
    }

    /// <summary>Ports the asymmetric viewport bounds at $86:C0B4.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        RoomEnemyProjectileSlot turret,
        ushort cameraX,
        ushort cameraY)
    {
        if (unchecked((short)turret.YPosition) < 0 ||
            unchecked((short)(turret.YPosition + 0x0010 - cameraY)) < 0 ||
            unchecked((ushort)(turret.YPosition + 0x0010 - cameraY)) >= 0x0100)
        {
            return false;
        }

        return unchecked((short)turret.XPosition) >= 0 &&
            unchecked((short)(turret.XPosition + 4 - cameraX)) >= 0 &&
            unchecked((ushort)(turret.XPosition + 4 - cameraX)) < 0x0108;
    }

    /// <summary>Ports turret-bullet initialization at $86:BF59.</summary>
    private void SpawnMotherBrainTurretBullet(RoomEnemyProjectileSlot turret)
    {
        RoomEnemyProjectileSlot? bullet = AllocateEnemyProjectile();
        if (bullet is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            bullet,
            RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
            MotherBrainTurretGraphicsIndex);
        ushort direction = unchecked((byte)turret.YSubposition);
        ushort directionByteOffset = unchecked((ushort)(direction * 2));
        bullet.DirectionParameter = direction;
        bullet.Variable0 = directionByteOffset;
        bullet.Variable1 = 0;
        bullet.XVelocity = ReadMotherBrainProjectileWord(unchecked((ushort)(
            MotherBrainTurretBulletXVelocityTable + directionByteOffset)));
        bullet.YVelocity = ReadMotherBrainProjectileWord(unchecked((ushort)(
            MotherBrainTurretBulletYVelocityTable + directionByteOffset)));
        bullet.XPosition = unchecked((ushort)(turret.XPosition +
            ReadMotherBrainProjectileWord(unchecked((ushort)(
                MotherBrainTurretBulletXOffsetTable + directionByteOffset)))));
        bullet.YPosition = unchecked((ushort)(turret.YPosition +
            ReadMotherBrainProjectileWord(unchecked((ushort)(
                MotherBrainTurretBulletYOffsetTable + directionByteOffset)))));
    }

    /// <summary>Ports the bullet property flicker, 8.8 movement, and point collision.</summary>
    private static void RunMotherBrainTurretBulletPreInstruction(
        RoomEnemyProjectileSlot bullet,
        RoomLevelData level)
    {
        // Property $8000 is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        (bullet.XPosition, bullet.XSubposition) = AddEightBitVelocity(
            bullet.XPosition,
            bullet.XSubposition,
            bullet.XVelocity);
        (bullet.YPosition, bullet.YSubposition) = AddEightBitVelocity(
            bullet.YPosition,
            bullet.YSubposition,
            bullet.YVelocity);

        if (ProjectileProbeHitsRoom(level, bullet.XPosition, bullet.YPosition))
            bullet.Clear();
    }

    private ushort ReadMotherBrainProjectileWord(ushort pointer) =>
        ReadWord(_bus!, 0x860000 | pointer);
}
