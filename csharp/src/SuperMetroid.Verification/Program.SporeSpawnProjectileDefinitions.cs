using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares Spore Spawn's spawn geometry and wrapped movement vectors with native data, then verifies production spawns and mirrored motion while compiled table reads are blocked.</summary>
    /// <param name="rom">Cartridge address space supplying the native geometry and movement bytes used as expectations.</param>
    private static void VerifySporeSpawnProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort index = 0; index < 4; index++)
        {
            AssertEqual(Word(0x86dcb9 + index * 2),
                SporeSpawnProjectileDefinitions.StalkYOffset(index),
                $"Spore Spawn stalk Y {index}");
            AssertEqual(Word(0x86dce6 + index * 2),
                SporeSpawnProjectileDefinitions.SpawnerX(index),
                $"Spore Spawn emitter X {index}");
        }
        for (int offset = 0; offset < 256; offset++)
        {
            SporeSpawnMovementDelta delta =
                SporeSpawnProjectileDefinitions.MovementAt(unchecked((byte)offset));
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x86dd6c + offset)), delta.X,
                $"Spore Spawn movement X {offset:X2}");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0x86dd6c + ((offset + 1) & 0xff))),
                delta.Y, $"Spore Spawn movement Y {offset:X2}");
        }

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new SporeSpawnProjectileReadGuard(rom));
        var spawnStalk = typeof(RoomEnemySystem).GetMethod("SpawnSporeSpawnStalk", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var spawnSpawner = typeof(RoomEnemySystem).GetMethod("SpawnSporeSpawnSpawner", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var move = typeof(RoomEnemySystem).GetMethod(
                "RunSporeSpawnSporePreInstruction", flags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot>>();
        RoomEnemySlot body = enemies.Slots[0];
        body.XPosition = 0x1234;
        body.YPosition = 0x0200;
        for (ushort index = 0; index < 4; index++)
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();
            spawnStalk(body, index);
            RoomEnemyProjectileSlot stalk = enemies.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(body.XPosition, stalk.XPosition, $"Spore Spawn production stalk X {index}");
            AssertEqual(unchecked((ushort)(body.YPosition + Word(0x86dcb9 + index * 2))),
                stalk.YPosition, $"Spore Spawn production stalk Y {index}");

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();
            spawnSpawner(body, index);
            RoomEnemyProjectileSlot spawner = enemies.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(Word(0x86dce6 + index * 2), spawner.XPosition,
                $"Spore Spawn production emitter X {index}");
            AssertEqual((ushort)520, spawner.YPosition,
                $"Spore Spawn production emitter Y {index}");
        }

        RoomEnemyProjectileSlot spore = enemies.EnemyProjectiles[0];
        for (int offset = 0; offset < 256; offset++)
        for (int mirrored = 0; mirrored < 2; mirrored++)
        {
            spore.Kind = RoomEnemyProjectileKind.SporeSpawnSpore;
            spore.XPosition = 0x0200;
            spore.YPosition = 0x0200;
            spore.Variable0 = unchecked((ushort)offset);
            spore.Variable1 = mirrored == 0 ? (ushort)0 : (ushort)0x0080;
            int x = unchecked((sbyte)rom.ReadByte(0x86dd6c + offset));
            int y = unchecked((sbyte)rom.ReadByte(0x86dd6c + ((offset + 1) & 0xff)));
            move(spore);
            AssertEqual(unchecked((ushort)(0x0200 + (mirrored == 0 ? x : -x))),
                spore.XPosition, $"Spore Spawn production X {offset:X2}/{mirrored}");
            AssertEqual(unchecked((ushort)(0x0200 + y + y)), spore.YPosition,
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

    /// <summary>Wraps an address space to reject runtime reads from the compiled Spore Spawn geometry and movement tables.</summary>
    /// <param name="source">Underlying address space for reads outside those table ranges and for writes.</param>
    private sealed class SporeSpawnProjectileReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the projectile-definition table guard.</summary>
        /// <param name="address">Address requested by the importer.</param>
        /// <returns>The byte supplied by the wrapped address space when it is outside a blocked table range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the compiled stalk, emitter, and movement tables and forwards other reads.</summary>
        /// <param name="address">Address of the requested byte.</param>
        /// <returns>The byte supplied by the wrapped address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address lies within a blocked Spore Spawn definition-table range.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x86dcb9 and < 0x86dcc1 or
                >= 0x86dce6 and < 0x86dcee or
                >= 0x86dd6c and < 0x86de6c
                ? throw new InvalidOperationException(
                    $"Spore Spawn attempted migrated projectile-definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
