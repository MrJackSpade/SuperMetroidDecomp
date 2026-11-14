using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mother Brain turret placement, rotation, direction, and bullet tables against bank 86.</summary>
internal static class MotherBrainTurretDefinitionsVerification
{
    /// <summary>Exercises all twelve turret placements and eight bullet directions with source table reads forbidden.</summary>
    internal static void VerifyMotherBrainTurretDefinitions(CartridgeImportAddressSpace rom)
    {
        var guarded = NewMotherBrainTurretReadGuard(rom);

        // The cartridge numbered the turrets by their spawn order, which is the order of the typed list.
        AssertEqual(12, MotherBrainTurretDefinitions.Turrets.Count, "Mother Brain has twelve turrets");
        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            MotherBrainTurretDefinition definition = MotherBrainTurretDefinitions.Turrets[parameter];
            int wordOffset = parameter * 2;
            // $86:BEF9: each turret's eight allowed-direction bytes follow the previous turret's.
            ushort policyRow = (ushort)(0xbef9 + parameter * 8);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86be89 + wordOffset)), definition.Position.X,
                $"Mother Brain turret X {parameter}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bea1 + wordOffset)), definition.Position.Y,
                $"Mother Brain turret Y {parameter}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bec9 + wordOffset)),
                policyRow,
                $"Mother Brain turret rotation pointer {parameter}");
            // The cartridge numbers a turret's directions by their place on the turret ring.
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bee1 + wordOffset)),
                (ushort)MotherBrainTurretDefinitions.Ring.PositionOf(definition.InitialDirection),
                $"Mother Brain turret initial direction {parameter}");

            for (byte direction = 0; direction < 8; direction++)
            {
                bool nativeAllowed = rom.ReadCartridgeByte(
                    0x860000 | (policyRow + direction)) != 0;
                AssertEqual(nativeAllowed, MotherBrainTurrets.IsRotationAllowed(
                    definition,
                    MotherBrainTurretDefinitions.Ring.At(direction)),
                    $"Mother Brain turret allowed direction {parameter}/{direction}");
            }

            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 0;
            enemies.MotherBrainBehavior.Body.SpawnMotherBrainTurret(definition);

            EnemyProjectile turret =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            MotherBrainTurretDirectionDefinition directionDefinition =
                MotherBrainTurrets.ForDirection(definition.InitialDirection);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurret, turret.Kind,
                $"production Mother Brain turret kind {parameter}");
            AssertEqual(definition.Position, turret.Position.Whole,
                $"production Mother Brain turret position {parameter}");
            MotherBrainTurretState turretState = turret.StateOf<MotherBrainTurretState>();
            AssertEqual(definition, turretState.Turret,
                $"production Mother Brain turret identity {parameter}");
            AssertEqual(MotherBrainTurretDefinitions.Ring.PositionOf(definition.InitialDirection), turretState.RingPosition,
                $"production Mother Brain turret initial ring position {parameter}");
            AssertEqual(1, turretState.RotationStep,
                $"production Mother Brain turret starts turning forward {parameter}");
            AssertTrue(turret.Animation is MotherBrainTurretAnimation { Program: MotherBrainTurretSegment.TurretPose },
                $"production Mother Brain turret {parameter} starts its typed pose");
            AssertEqual(MotherBrainTurretDefinitions.MinimumRotationDelay, turretState.RotationWait,
                $"production Mother Brain turret rotation timer {parameter}");
            AssertEqual(MotherBrainTurretDefinitions.MinimumFiringCooldown, turretState.FiringWait,
                $"production Mother Brain turret cooldown {parameter}");

            // Every position a turret may face: a turn never leaves its allowed sector.
            foreach (sbyte delta in new sbyte[] { -1, 1 })
            for (byte current = (byte)MotherBrainTurretDefinitions.Ring.PositionOf(definition.FirstAllowedDirection);
                current <= MotherBrainTurretDefinitions.Ring.PositionOf(definition.LastAllowedDirection); current++)
            {
                var rotation = new EnemyProjectile(0, default)
                {
                    State = new MotherBrainTurretState(definition, current) { RotationStep = delta },
                };
                byte candidate = unchecked((byte)((current + delta) & 7));
                bool allowed = rom.ReadCartridgeByte(
                    0x860000 | (policyRow + candidate)) != 0;
                sbyte expectedDelta = allowed ? delta : unchecked((sbyte)-delta);
                byte expectedDirection = allowed
                    ? candidate
                    : unchecked((byte)(current + expectedDelta));

                MotherBrainProjectiles.SelectNextMotherBrainTurretDirection(rotation);
                MotherBrainTurretState rotated = rotation.StateOf<MotherBrainTurretState>();
                AssertEqual(expectedDirection, rotated.RingPosition,
                    $"production Mother Brain turret rotation {parameter}/{current}/{delta}");
                AssertEqual(expectedDelta, rotated.RotationStep,
                    $"production Mother Brain turret turn step {parameter}/{current}/{delta}");
            }
        }

        for (byte direction = 0; direction < 8; direction++)
        {
            MotherBrainTurretDirection typedDirection =
                MotherBrainTurretDefinitions.Ring.At(direction);
            MotherBrainTurretDirectionDefinition definition =
                MotherBrainTurrets.ForDirection(typedDirection);
            int wordOffset = direction * 2;
            // Both native pose tables select the list the typed turret pose matches (--mother-brain-projectile-animations).
            ushort pose = MotherBrainNativeListAddresses.TurretEntries[direction].TurretPose;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86beb9 + wordOffset)), pose,
                $"Mother Brain turret initial pose {direction}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c040 + wordOffset)), pose,
                $"Mother Brain turret runtime pose {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bf9f + wordOffset))),
                definition.BulletOffset.X,
                $"Mother Brain turret bullet X offset {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bfaf + wordOffset))),
                definition.BulletOffset.Y,
                $"Mother Brain turret bullet Y offset {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bfbf + wordOffset))),
                definition.BulletVelocity.X.InTwoHundredFiftySixthsOfPixel,
                $"Mother Brain turret bullet X velocity {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bfcf + wordOffset))),
                definition.BulletVelocity.Y.InTwoHundredFiftySixthsOfPixel,
                $"Mother Brain turret bullet Y velocity {direction}");

            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var turret = new EnemyProjectile(0, default)
            {
                Position = new(new(0x0400, 0), new(0x0200, 0)),
                State = new MotherBrainTurretState(MotherBrainTurretDefinitions.Turrets[0], direction),
            };
            enemies.MotherBrainBehavior.Projectiles.Projectile.SpawnMotherBrainTurretBullet(turret);

            EnemyProjectile bullet =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, bullet.Kind,
                $"production Mother Brain turret bullet kind {direction}");
            AssertTrue(bullet.Animation is MotherBrainTurretAnimation bulletAnimation &&
                    bulletAnimation.PlayingSegment == new MotherBrainTurretPose(MotherBrainTurretSegment.BulletPose, typedDirection),
                $"production Mother Brain turret bullet direction {direction}");
            AssertEqual(new Position16<RoomPixels>(0x0400, 0x0200) + definition.BulletOffset, bullet.Position.Whole,
                $"production Mother Brain turret bullet position {direction}");
            AssertEqual(definition.BulletVelocity, bullet.Velocity,
                $"production Mother Brain turret bullet velocity {direction}");
        }

        Console.WriteLine(
            "Mother Brain turret definitions: 12 placements, 96 rotation bytes, 16 instruction selectors, 32 bullet words, 384 rotations and all 20 real spawns pass with source tables forbidden.");
    }

    /// <summary>Rejects production reads of compiled turret setup, bullet, and direction-selector ranges.</summary>
    private static CartridgeReadGuard NewMotherBrainTurretReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86be89 and < 0x86bf59 or
            >= 0x86bf9f and < 0x86bfdf or
            >= 0x86c040 and < 0x86c050,
                address => $"Mother Brain turret attempted migrated table read ${address:X6}.");

    /// <summary>Runs the Mother Brain turret comparison with a retail cartridge oracle.</summary>
    [VerificationCommand("--mother-brain-turret-definitions")]
    internal static int MotherBrainTurretDefinitionsCommand()
    {
        Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
