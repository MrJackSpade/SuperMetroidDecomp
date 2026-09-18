using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mother Brain room turret and bullet translation from <c>$86:BE4F-$C1B7</c>. These are
/// deliberately part of the common eighteen-slot bank-$86 pool: the twelve permanent
/// turrets leave only six slots for bullets, dust, and other actors, exactly as on SNES.
/// </summary>
public sealed partial class RoomEnemySystem
{
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
            MotherBrainTurretDefinitions.GraphicsIndex);

        MotherBrainTurretDefinition definition =
            MotherBrainTurretDefinitions.ForTurret(parameter);
        MotherBrainTurretDirection direction = definition.InitialDirection;
        MotherBrainTurretDirectionDefinition directionDefinition =
            MotherBrainTurretDefinitions.ForDirection(direction);
        turret.DirectionParameter = parameter;
        turret.XPosition = definition.X;
        turret.YPosition = definition.Y;

        // The turret is stationary, so the initializer intentionally repurposes both
        // subposition words. X-subposition is a bank-$86 pointer to this turret's allowed
        // direction bytes. Y-subposition packs direction in the low byte and signed
        // rotation delta (+1 initially) in the high byte.
        turret.XSubposition = definition.AllowedRotationPointer;
        turret.YSubposition = unchecked((ushort)(0x0100 | (byte)direction));
        turret.InstructionPointer = directionDefinition.InstructionPointer;
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
            MotherBrainTurretDirection direction =
                (MotherBrainTurretDirection)unchecked((byte)turret.YSubposition);
            turret.InstructionPointer = MotherBrainTurretDefinitions
                .ForDirection(direction).InstructionPointer;
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
    private static void SelectNextMotherBrainTurretDirection(RoomEnemyProjectileSlot turret)
    {
        byte currentDirection = unchecked((byte)turret.YSubposition);
        sbyte rotationDelta = unchecked((sbyte)(turret.YSubposition >> 8));
        byte candidate = unchecked((byte)((currentDirection + rotationDelta) & 7));
        bool allowed = MotherBrainTurretDefinitions.IsRotationAllowed(
            turret.XSubposition,
            (MotherBrainTurretDirection)candidate);

        if (allowed)
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
        turret.XVelocity = Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay);
    }

    private void ResetMotherBrainTurretCooldown(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)_nextRandom!());
        turret.YVelocity = Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown);
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
            MotherBrainTurretDefinitions.GraphicsIndex);
        MotherBrainTurretDirection direction =
            (MotherBrainTurretDirection)unchecked((byte)turret.YSubposition);
        MotherBrainTurretDirectionDefinition definition =
            MotherBrainTurretDefinitions.ForDirection(direction);
        bullet.DirectionParameter = (byte)direction;
        bullet.Variable0 = unchecked((ushort)((byte)direction * 2));
        bullet.Variable1 = 0;
        bullet.XVelocity = unchecked((ushort)definition.BulletXVelocity);
        bullet.YVelocity = unchecked((ushort)definition.BulletYVelocity);
        bullet.XPosition = unchecked((ushort)(turret.XPosition +
            definition.BulletXOffset));
        bullet.YPosition = unchecked((ushort)(turret.YPosition +
            definition.BulletYOffset));
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

}
