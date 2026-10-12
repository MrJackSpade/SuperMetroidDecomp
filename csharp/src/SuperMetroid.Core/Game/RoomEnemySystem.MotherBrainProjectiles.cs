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
        turret.Position = turret.Position.WithWhole(definition.Position);

        // The turret is stationary, so the initializer intentionally repurposes both
        // subposition words. X-subposition is a bank-$86 pointer to this turret's allowed
        // direction bytes. Y-subposition packs direction in the low byte and signed
        // rotation delta (+1 initially) in the high byte.
        turret.Position = turret.Position.WithFractionX(definition.AllowedRotationPointer).WithFractionY(unchecked((ushort)(0x0100 | (byte)direction)));
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
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = MotherBrain ??
            throw new InvalidDataException("A Mother Brain turret has no owning encounter state.");
        bool onScreen = MotherBrainTurretIsOnScreen(turret, camera);

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
            ushort x = turret.Position.X.Whole;
            ushort y = turret.Position.Y.Whole;
            turret.Clear();
            SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 0x000c);
            return;
        }

        turret.Velocity = turret.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(turret.Velocity.X.RawValue - 1))));
        if (turret.Velocity.X.RawValue == 0)
        {
            ResetMotherBrainTurretRotationTimer(turret);
            SelectNextMotherBrainTurretDirection(turret);
            MotherBrainTurretDirection direction =
                (MotherBrainTurretDirection)unchecked((byte)turret.Position.Y.Fraction);
            turret.InstructionPointer = MotherBrainTurretDefinitions
                .ForDirection(direction).InstructionPointer;
            turret.InstructionTimer = 1;
        }

        turret.Velocity = turret.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(turret.Velocity.Y.RawValue - 1))));
        if (turret.Velocity.Y.RawValue == 0)
        {
            ResetMotherBrainTurretCooldown(turret);
            SpawnMotherBrainTurretBullet(turret);
        }
    }

    /// <summary>Ports the signed direction-delta and allowed-rotation test at $86:C050.</summary>
    private static void SelectNextMotherBrainTurretDirection(RoomEnemyProjectileSlot turret)
    {
        byte currentDirection = unchecked((byte)turret.Position.Y.Fraction);
        sbyte rotationDelta = unchecked((sbyte)(turret.Position.Y.Fraction >> 8));
        byte candidate = unchecked((byte)((currentDirection + rotationDelta) & 7));
        bool allowed = MotherBrainTurretDefinitions.IsRotationAllowed(
            turret.Position.X.Fraction,
            (MotherBrainTurretDirection)candidate);

        if (allowed)
        {
            turret.Position = turret.Position.WithFractionY(unchecked((ushort)(
                (turret.Position.Y.Fraction & 0xff00) | candidate)));
            return;
        }

        // A disallowed next step reverses the signed high-byte delta and immediately takes
        // one step in the new direction. The cartridge does not perform a second mask test.
        rotationDelta = unchecked((sbyte)-rotationDelta);
        byte reversedDirection = unchecked((byte)(currentDirection + rotationDelta));
        turret.Position = turret.Position.WithFractionY(unchecked((ushort)(
            (unchecked((byte)rotationDelta) << 8) | reversedDirection)));
    }

    private void ResetMotherBrainTurretRotationTimer(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)EnemyRandom.Generate());
        turret.Velocity = turret.Velocity.WithX(new SnesSignedEightEight(Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay)));
    }

    private void ResetMotherBrainTurretCooldown(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)EnemyRandom.Generate());
        turret.Velocity = turret.Velocity.WithY(new SnesSignedEightEight(Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown)));
    }

    /// <summary>Ports the asymmetric viewport bounds at $86:C0B4.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        RoomEnemyProjectileSlot turret,
        Position16<RoomPixels> camera)
    {
        if (unchecked((short)turret.Position.Y.Whole) < 0 ||
            unchecked((short)(turret.Position.Y.Whole + 0x0010 - camera.Y)) < 0 ||
            unchecked((ushort)(turret.Position.Y.Whole + 0x0010 - camera.Y)) >= 0x0100)
        {
            return false;
        }

        return unchecked((short)turret.Position.X.Whole) >= 0 &&
            unchecked((short)(turret.Position.X.Whole + 4 - camera.X)) >= 0 &&
            unchecked((ushort)(turret.Position.X.Whole + 4 - camera.X)) < 0x0108;
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
            (MotherBrainTurretDirection)unchecked((byte)turret.Position.Y.Fraction);
        MotherBrainTurretDirectionDefinition definition =
            MotherBrainTurretDefinitions.ForDirection(direction);
        bullet.DirectionParameter = (byte)direction;
        bullet.Variable0 = unchecked((ushort)((byte)direction * 2));
        bullet.Variable1 = 0;
        bullet.Velocity = definition.BulletVelocity;
        bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
    }

    /// <summary>Ports the bullet property flicker, 8.8 movement, and point collision.</summary>
    private static void RunMotherBrainTurretBulletPreInstruction(
        RoomEnemyProjectileSlot bullet,
        RoomLevelData level)
    {
        // Property $8000 is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToSixteenSixteen());

        if (NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Clear();
    }

}
