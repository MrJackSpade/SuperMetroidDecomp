using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled Cacatac spike selectors and speeds against production projectile spawns.</summary>
    /// <param name="rom">The ROM used to verify the native instruction-table entries.</param>
    private static void VerifyCacatacProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0x86d96a;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CacatacProjectileReadGuard(rom));
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnCacatacSpike", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        RoomEnemySlot source = enemies.Slots[0];
        source.XPosition = 0x1234;
        source.XSubposition = 0x5678;
        source.YPosition = 0x9abc;
        source.YSubposition = 0xdef0;
        source.PaletteIndex = 0x0c00;
        source.VramTilesIndex = 0x0123;

        foreach (CacatacSpikeDirection direction in Enum.GetValues<CacatacSpikeDirection>())
        {
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
                projectile.Clear();

            ushort raw = (ushort)direction;
            ushort native = (ushort)(rom.ReadByte(instructionTable + raw) |
                rom.ReadByte(instructionTable + raw + 1) << 8);
            AssertEqual(native, CacatacProjectileDefinitions.InstructionList(direction),
                $"compiled Cacatac spike instruction {direction}");

            CacatacSpikeSpeedPair speeds = CacatacProjectileDefinitions.SpeedPair(direction);
            ushort expectedNegative = direction >= CacatacSpikeDirection.UpLeft
                ? (ushort)0xfe80
                : (ushort)0xfe00;
            ushort expectedPositive = direction >= CacatacSpikeDirection.UpLeft
                ? (ushort)0x0180
                : (ushort)0x0200;
            AssertEqual(expectedNegative, speeds.Negative, $"compiled Cacatac negative speed {direction}");
            AssertEqual(expectedPositive, speeds.Positive, $"compiled Cacatac positive speed {direction}");

            spawn(source, raw);
            RoomEnemyProjectileSlot actual = enemies.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
            AssertEqual(native, actual.InstructionPointer, $"spawned Cacatac instruction {direction}");
            AssertEqual(expectedNegative, actual.YVelocity, $"spawned Cacatac negative speed {direction}");
            AssertEqual(expectedPositive, actual.XVelocity, $"spawned Cacatac positive speed {direction}");
            AssertEqual(source.XPosition, actual.XPosition, $"spawned Cacatac X {direction}");
            AssertEqual(source.XSubposition, actual.XSubposition, $"spawned Cacatac X fraction {direction}");
            AssertEqual(source.YPosition, actual.YPosition, $"spawned Cacatac Y {direction}");
            AssertEqual(source.YSubposition, actual.YSubposition, $"spawned Cacatac Y fraction {direction}");
        }

        AssertThrows<InvalidDataException>(
            () => CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)1),
            "Cacatac odd spike selector");
        AssertThrows<InvalidDataException>(
            () => CacatacProjectileDefinitions.InstructionList((CacatacSpikeDirection)20),
            "Cacatac spike selector beyond authored table");

        Console.WriteLine(
            "Cacatac spike definitions: ten native selectors, speed pairs and production spawns pass with selector reads forbidden.");
    }

    /// <summary>Wraps an address space while rejecting reads from the migrated Cacatac selector table.</summary>
    /// <param name="source">The underlying address space used for all permitted reads and writes.</param>
    private sealed class CacatacProjectileReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the guarded address-space read path.</summary>
        /// <param name="address">The address requested by the importer.</param>
        /// <returns>The underlying byte unless the address is in the prohibited selector table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated selector-table reads and delegates all other reads.</summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The byte from the underlying address space when the read is permitted.</returns>
        public byte ReadByte(int address) => address is >= 0x86d96a and < 0x86d97e
            ? throw new InvalidOperationException(
                $"Cacatac spike spawn attempted migrated selector read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">The destination address.</param>
        /// <param name="value">The byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
