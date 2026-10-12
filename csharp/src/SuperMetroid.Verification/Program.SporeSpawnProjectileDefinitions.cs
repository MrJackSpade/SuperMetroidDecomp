using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Spore Spawn projectile definitions")]
    private static void VerifySporeSpawnProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        for (ushort index = 0; index < 4; index++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + index * 2)),
                SporeSpawnProjectileDefinitions.StalkYOffset(index),
                $"Spore Spawn stalk Y {index}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + index * 2)),
                SporeSpawnProjectileDefinitions.SpawnerX(index),
                $"Spore Spawn emitter X {index}");
        }
        for (int offset = 0; offset < 256; offset++)
        {
            Offset16<RoomPixels> delta =
                SporeSpawnProjectileDefinitions.MovementAt(unchecked((byte)offset));
            AssertEqual((short)unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + offset)), delta.X,
                $"Spore Spawn movement X {offset:X2}");
            AssertEqual((short)unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + ((offset + 1) & 0xff))),
                delta.Y, $"Spore Spawn movement Y {offset:X2}");
        }

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, NewSporeSpawnProjectileReadGuard(rom));
        var spawnStalk = typeof(RoomEnemySystem).GetMethod("SpawnSporeSpawnStalk", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var spawnSpawner = typeof(RoomEnemySystem).GetMethod("SpawnSporeSpawnSpawner", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var move = typeof(RoomEnemySystem).GetMethod(
                "RunSporeSpawnSporePreInstruction", flags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot>>();
        RoomEnemySlot body = enemies.Slots[0];
        body.Position = body.Position.WithWhole(new(0x1234, 0x0200));
        for (ushort index = 0; index < 4; index++)
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();
            spawnStalk(body, index);
            RoomEnemyProjectileSlot stalk = enemies.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(body.Position.X.Whole, stalk.Position.X.Whole, $"Spore Spawn production stalk X {index}");
            AssertEqual(unchecked((ushort)(body.Position.Y.Whole + CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + index * 2)))),
                stalk.Position.Y.Whole, $"Spore Spawn production stalk Y {index}");

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();
            spawnSpawner(body, index);
            RoomEnemyProjectileSlot spawner = enemies.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + index * 2)), spawner.Position.X.Whole,
                $"Spore Spawn production emitter X {index}");
            AssertEqual((ushort)520, spawner.Position.Y.Whole,
                $"Spore Spawn production emitter Y {index}");
        }

        RoomEnemyProjectileSlot spore = enemies.EnemyProjectiles[0];
        for (int offset = 0; offset < 256; offset++)
        for (int mirrored = 0; mirrored < 2; mirrored++)
        {
            spore.Kind = RoomEnemyProjectileKind.SporeSpawnSpore;
            spore.Position = spore.Position.WithWhole(new(0x0200, 0x0200));
            spore.Variable0 = unchecked((ushort)offset);
            spore.Variable1 = mirrored == 0 ? (ushort)0 : (ushort)0x0080;
            int x = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + offset));
            int y = unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + ((offset + 1) & 0xff)));
            move(spore);
            AssertEqual(unchecked((ushort)(0x0200 + (mirrored == 0 ? x : -x))),
                spore.Position.X.Whole, $"Spore Spawn production X {offset:X2}/{mirrored}");
            AssertEqual(unchecked((ushort)(0x0200 + y + y)), spore.Position.Y.Whole,
                $"Spore Spawn production Y {offset:X2}/{mirrored}");
            AssertEqual(unchecked((ushort)(byte)(offset + 2)), spore.Variable0,
                $"Spore Spawn production cursor {offset:X2}/{mirrored}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SporeSpawnProjectileDefinitions.StalkYOffset(4),
            "Spore Spawn stalk index past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SporeSpawnProjectileDefinitions.SpawnerX(ushort.MaxValue),
            "Spore Spawn emitter index underflow");
        Console.WriteLine(
            "Spore Spawn projectile definitions: eight geometry words, 256 movement bytes, all real spawns and 512 movement/mirroring handoffs pass with source tables forbidden.");
    }

    private static CartridgeReadGuard NewSporeSpawnProjectileReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86dcb9 and < 0x86dcc1 or
                >= 0x86dce6 and < 0x86dcee or
                >= 0x86dd6c and < 0x86de6c,
            address => $"Spore Spawn attempted migrated projectile-definition read ${address:X6}.");
}
