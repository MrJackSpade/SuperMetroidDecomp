using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Verification.Enemies.Deaths;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the five enemy death explosion selectors and production spawn behavior.</summary>
internal static class EnemyDeathExplosionDefinitionsVerification
{
    /// <summary>Compares cartridge pointers, live explosion placement, and the out-of-range small-explosion clamp.</summary>
    internal static void VerifyEnemyDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        var guarded = NewEnemyDeathExplosionReadGuard(rom);

        for (ushort animation = 0; animation < 5; animation++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86efd5 + animation * 2));
            AssertEqual(expected,
                EnemyDeathProjectileNativeListAddresses.Entry(EnemyDeathExplosionDefinitions.Program(animation)),
                $"enemy death explosion program {animation}");

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot enemy = enemies.SlotTable.Slots[0];
            enemy.Bind(EnemyDefinitionId.Sciser, enemy.Definition);
            enemy.Position = enemy.Position.WithWhole(new(unchecked((ushort)(0xfff0 + animation)), unchecked((ushort)(0x0100 + animation))));

            enemies.DeathsAndDrops.StartGenericEnemyDeath(enemy, animation);
            RoomEnemyProjectileSlot explosion =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
                $"production enemy death explosion kind {animation}");
            AssertEqual(EnemyDeathExplosionDefinitions.Program(animation), ProgramOf(explosion),
                $"production enemy death explosion program {animation}");
            AssertEqual(unchecked((ushort)(0xfff0 + animation)), explosion.Position.X.Whole,
                $"production enemy death explosion X {animation}");
            AssertEqual(unchecked((ushort)(0x0100 + animation)), explosion.Position.Y.Whole,
                $"production enemy death explosion Y {animation}");
        }

        var clamped = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot clampedEnemy = clamped.SlotTable.Slots[0];
        clampedEnemy.Bind(EnemyDefinitionId.Sciser, clampedEnemy.Definition);
        clamped.DeathsAndDrops.StartGenericEnemyDeath(clampedEnemy, 5);
        AssertEqual(EnemyDeathProjectileProgram.SmallExplosion,
            ProgramOf(clamped.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive)),
            "production enemy death out-of-range animation clamps to small explosion");

        AssertThrows<ArgumentOutOfRangeException>(
            () => EnemyDeathExplosionDefinitions.Program(5),
            "enemy death animation past table");
        Console.WriteLine(
            "Enemy death explosion definitions: five native selectors, five real variants and the generic clamp path pass with the pointer table forbidden.");
    }

    /// <summary>Rejects live reads of the migrated explosion selector table.</summary>
    private static CartridgeReadGuard NewEnemyDeathExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86efd5 and < 0x86efdf,
                address => $"Enemy death explosion attempted migrated selector read ${address:X6}.");

    /// <summary>Runs the enemy death explosion selector and spawn checks.</summary>
    [VerificationCommand("--enemy-death-explosion-definitions")]
    internal static int EnemyDeathExplosionDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyDeathExplosionDefinitions), () => VerifyEnemyDeathExplosionDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
