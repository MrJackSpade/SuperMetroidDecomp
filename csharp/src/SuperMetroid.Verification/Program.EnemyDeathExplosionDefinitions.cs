using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class EnemyDeathExplosionDefinitionsVerification
{
    internal static void VerifyEnemyDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = NewEnemyDeathExplosionReadGuard(rom);

        for (ushort animation = 0; animation < 5; animation++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86efd5 + animation * 2));
            AssertEqual(expected,
                EnemyDeathExplosionDefinitions.InstructionPointer(animation),
                $"enemy death explosion instruction {animation}");

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guarded);
            RoomEnemySlot enemy = enemies.SlotTable.Slots[0];
            enemy.EnemyDefinitionPointer = EnemyDefinitionId.Sciser;
            enemy.Position = enemy.Position.WithWhole(new(unchecked((ushort)(0xfff0 + animation)), unchecked((ushort)(0x0100 + animation))));

            enemies.DeathsAndDrops.StartGenericEnemyDeath(enemy, animation);
            RoomEnemyProjectileSlot explosion =
                enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
                $"production enemy death explosion kind {animation}");
            AssertEqual(expected, explosion.InstructionPointer,
                $"production enemy death explosion instruction {animation}");
            AssertEqual(unchecked((ushort)(0xfff0 + animation)), explosion.Position.X.Whole,
                $"production enemy death explosion X {animation}");
            AssertEqual(unchecked((ushort)(0x0100 + animation)), explosion.Position.Y.Whole,
                $"production enemy death explosion Y {animation}");
        }

        var clamped = new RoomEnemySystem();
        clamped.BindFixtureBus(guarded);
        RoomEnemySlot clampedEnemy = clamped.SlotTable.Slots[0];
        clampedEnemy.EnemyDefinitionPointer = EnemyDefinitionId.Sciser;
        clamped.DeathsAndDrops.StartGenericEnemyDeath(clampedEnemy, 5);
        AssertEqual(EnemyDeathExplosionDefinitions.InstructionPointer(
                (ushort)EnemyDeathAnimation.SmallExplosion),
            clamped.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive).InstructionPointer,
            "production enemy death out-of-range animation clamps to small explosion");

        AssertThrows<ArgumentOutOfRangeException>(
            () => EnemyDeathExplosionDefinitions.InstructionPointer(5),
            "enemy death animation past table");
        Console.WriteLine(
            "Enemy death explosion definitions: five native selectors, five real variants and the generic clamp path pass with the pointer table forbidden.");
    }

    private static CartridgeReadGuard NewEnemyDeathExplosionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86efd5 and < 0x86efdf,
                address => $"Enemy death explosion attempted migrated selector read ${address:X6}.");

    [VerificationCommand("--enemy-death-explosion-definitions")]
    internal static int EnemyDeathExplosionDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyDeathExplosionDefinitions), () => VerifyEnemyDeathExplosionDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
