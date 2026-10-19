using SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks the three compiled Mother Brain death-explosion selectors in production spawns.</summary>
internal static class MotherBrainDeathExplosionDefinitionsVerification
{
    /// <summary>Compares bank $86 selector words and body-relative projectile placement without runtime table reads.</summary>
    internal static void VerifyMotherBrainDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c929 + parameter * 2)),
                MiscDustProjectileNativeListAddresses.Entry(MotherBrainDeathExplosionDefinitions.Program(parameter)),
                $"Mother Brain death explosion instruction {parameter}");
        }

        var guarded = NewMotherBrainDeathExplosionReadGuard(rom);
        for (ushort parameter = 0; parameter < 3; parameter++)
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0200))};
            enemies.MotherBrainEncounter.MotherBrain = new MotherBrainEnemyState(body);
            var request = new MotherBrainDeathExplosionRequest(
                Offset: new(-7, 9),
                ProjectileParameter: parameter,
                SoundEffect: MotherBrainSoundDefinitions.Explosion);
            enemies.MotherBrainBehavior.SpawnMotherBrainDeathExplosion(request);

            RoomEnemyProjectileSlot projectile =
                enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(RoomEnemyProjectileKind.MotherBrainDeathExplosion, projectile.Kind,
                $"production Mother Brain death explosion kind {parameter}");
            AssertTrue(projectile.Animation is MiscDustProjectileAnimation started &&
                    started.Cursor.Program == MotherBrainDeathExplosionDefinitions.Program(parameter),
                $"production Mother Brain death explosion instruction {parameter}");
            AssertEqual(0x00f9, projectile.Position.X.Whole,
                $"production Mother Brain death explosion X {parameter}");
            AssertEqual(0x0209, projectile.Position.Y.Whole,
                $"production Mother Brain death explosion Y {parameter}");
            AssertEqual(unchecked((ushort)-7), projectile.Velocity.X.RawValue,
                $"production Mother Brain death explosion X offset {parameter}");
            AssertEqual(9, projectile.Velocity.Y.RawValue,
                $"production Mother Brain death explosion Y offset {parameter}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainDeathExplosionDefinitions.Program(3),
            "Mother Brain death explosion parameter past table");
        Console.WriteLine(
            "Mother Brain death explosions: three native program selectors and all three real body-relative spawns pass with the selector table forbidden.");
    }

    /// <summary>Guards the six native selector bytes after their definitions have been compiled.</summary>
    private static CartridgeReadGuard NewMotherBrainDeathExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86c929 and < 0x86c92f,
                address => $"Mother Brain death explosion attempted migrated selector read ${address:X6}.");
}
