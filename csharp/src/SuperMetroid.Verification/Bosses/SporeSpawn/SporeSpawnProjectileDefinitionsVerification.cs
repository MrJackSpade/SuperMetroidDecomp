using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>Checks Spore Spawn stalk, emitter, and spore-motion definitions against native data.</summary>
internal static class SporeSpawnProjectileDefinitionsVerification
{
    /// <summary>Compares geometry and all movement bytes, then runs real spawn and mirrored-motion paths with source reads blocked.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Spore Spawn projectile definitions")]
    internal static void VerifySporeSpawnProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
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
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + offset)), delta.X,
                $"Spore Spawn movement X {offset:X2}");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0x86dd6c + ((offset + 1) & 0xff))),
                delta.Y, $"Spore Spawn movement Y {offset:X2}");
        }

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, ushort> spawnStalk = enemies.SporeSpawnBehavior.SpawnSporeSpawnStalk;
        Action<RoomEnemySlot, ushort> spawnSpawner = enemies.SporeSpawnBehavior.SpawnSporeSpawnSpawner;
        Action<RoomEnemyProjectileSlot> move = SporeSpawnBehavior.RunSporeSpawnSporePreInstruction;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(0x1234, 0x0200));
        for (ushort index = 0; index < 4; index++)
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Clear();
            spawnStalk(body, index);
            RoomEnemyProjectileSlot stalk = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(body.Position.X.Whole, stalk.Position.X.Whole, $"Spore Spawn production stalk X {index}");
            AssertEqual(unchecked((ushort)(body.Position.Y.Whole + CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dcb9 + index * 2)))),
                stalk.Position.Y.Whole, $"Spore Spawn production stalk Y {index}");

            foreach (RoomEnemyProjectileSlot projectile in enemies.Projectiles.EnemyProjectiles)
                projectile.Clear();
            spawnSpawner(body, index);
            RoomEnemyProjectileSlot spawner = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86dce6 + index * 2)), spawner.Position.X.Whole,
                $"Spore Spawn production emitter X {index}");
            AssertEqual((ushort)520, spawner.Position.Y.Whole,
                $"Spore Spawn production emitter Y {index}");
        }

        RoomEnemyProjectileSlot spore = enemies.Projectiles.EnemyProjectiles[0];
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
            AssertEqual(unchecked((byte)(offset + 2)), spore.Variable0,
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

}
