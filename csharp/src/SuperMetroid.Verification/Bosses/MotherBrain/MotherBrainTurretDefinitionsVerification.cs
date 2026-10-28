using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mother Brain turret placement, rotation, direction, and bullet tables against bank 86.</summary>
internal static class MotherBrainTurretDefinitionsVerification
{
    /// <summary>Exercises all twelve turret placements and eight bullet directions with source table reads forbidden.</summary>
    internal static void VerifyMotherBrainTurretDefinitions(SuperMetroidAddressSpace rom)
    {
        var guarded = NewMotherBrainTurretReadGuard(rom);

        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            MotherBrainTurretDefinition definition =
                MotherBrainTurrets.ForTurret(parameter);
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
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bee1 + wordOffset)),
                (ushort)definition.InitialDirection,
                $"Mother Brain turret initial direction {parameter}");

            for (byte direction = 0; direction < 8; direction++)
            {
                bool nativeAllowed = rom.ReadCartridgeByte(
                    0x860000 | (policyRow + direction)) != 0;
                AssertEqual(nativeAllowed, MotherBrainTurrets.IsRotationAllowed(
                    parameter,
                    (MotherBrainTurretDirection)direction),
                    $"Mother Brain turret allowed direction {parameter}/{direction}");
            }

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 0;
            enemies.MotherBrainBehavior.Body.SpawnMotherBrainTurret(parameter);

            RoomEnemyProjectileSlot turret =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            MotherBrainTurretDirectionDefinition directionDefinition =
                MotherBrainTurrets.ForDirection(definition.InitialDirection);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurret, turret.Kind,
                $"production Mother Brain turret kind {parameter}");
            AssertEqual(definition.Position, turret.Position.Whole,
                $"production Mother Brain turret position {parameter}");
            AssertEqual(parameter, turret.DirectionParameter,
                $"production Mother Brain turret identity {parameter}");
            AssertEqual(unchecked((ushort)(0x0100 | (byte)definition.InitialDirection)),
                turret.Position.Y.Fraction,
                $"production Mother Brain turret packed direction {parameter}");
            AssertTrue(turret.Animation is MotherBrainTurretAnimation { Program: MotherBrainTurretProgram.TurretPose },
                $"production Mother Brain turret {parameter} starts its typed pose");
            AssertEqual(MotherBrainTurretDefinitions.MinimumRotationDelay, turret.Velocity.X.RawValue,
                $"production Mother Brain turret rotation timer {parameter}");
            AssertEqual(MotherBrainTurretDefinitions.MinimumFiringCooldown, turret.Velocity.Y.RawValue,
                $"production Mother Brain turret cooldown {parameter}");

            foreach (sbyte delta in new sbyte[] { -1, 1 })
            for (byte current = 0; current < 8; current++)
            {
                var rotation = new RoomEnemyProjectileSlot(0)
                {
                    Position = new(new(0, 0), new(0, unchecked(LittleEndianBytes.Word(current, unchecked((byte)delta))))),
                    DirectionParameter = parameter,
                };
                byte candidate = unchecked((byte)((current + delta) & 7));
                bool allowed = rom.ReadCartridgeByte(
                    0x860000 | (policyRow + candidate)) != 0;
                sbyte expectedDelta = allowed ? delta : unchecked((sbyte)-delta);
                byte expectedDirection = allowed
                    ? candidate
                    : unchecked((byte)(current + expectedDelta));

                MotherBrainProjectiles.SelectNextMotherBrainTurretDirection(rotation);
                AssertEqual(unchecked(
                        LittleEndianBytes.Word(expectedDirection, unchecked((byte)expectedDelta))),
                    rotation.Position.Y.Fraction,
                    $"production Mother Brain turret rotation {parameter}/{current}/{delta}");
            }
        }

        for (byte direction = 0; direction < 8; direction++)
        {
            MotherBrainTurretDirection typedDirection =
                (MotherBrainTurretDirection)direction;
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
                definition.BulletVelocity.X.SignedRawValue,
                $"Mother Brain turret bullet X velocity {direction}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86bfcf + wordOffset))),
                definition.BulletVelocity.Y.SignedRawValue,
                $"Mother Brain turret bullet Y velocity {direction}");

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var turret = new RoomEnemyProjectileSlot(0)
            {
                Position = new(new(0x0400, 0), new(0x0200, direction)),
            };
            enemies.MotherBrainBehavior.Projectiles.SpawnMotherBrainTurretBullet(turret);

            RoomEnemyProjectileSlot bullet =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, bullet.Kind,
                $"production Mother Brain turret bullet kind {direction}");
            AssertEqual(direction, bullet.DirectionParameter,
                $"production Mother Brain turret bullet direction {direction}");
            AssertEqual((ushort)(direction * 2), bullet.Variable0,
                $"production Mother Brain turret bullet direction offset {direction}");
            AssertEqual(new Position16<RoomPixels>(0x0400, 0x0200) + definition.BulletOffset, bullet.Position.Whole,
                $"production Mother Brain turret bullet position {direction}");
            AssertEqual(definition.BulletVelocity, bullet.Velocity,
                $"production Mother Brain turret bullet velocity {direction}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTurrets.ForTurret(12),
            "Mother Brain turret parameter past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTurrets.ForDirection((MotherBrainTurretDirection)8),
            "Mother Brain turret direction past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainTurrets.IsRotationAllowed(
                12, MotherBrainTurretDirection.Left),
            "Mother Brain turret rotation policy past table");

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
