using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies that Cacatac initialization uses the six compiled travel distances without reading their former cartridge table.</summary>
    /// <param name="rom">Pinned cartridge image supplying the native distance words used as expected values.</param>
    private static void VerifyCacatacMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int distanceTable = 0xa29f36;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CacatacDistanceReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeCacatac", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        foreach (ushort spawnX in new ushort[] { 0, 0x100, ushort.MaxValue })
        {
            for (byte distanceIndex = 0; distanceIndex < 6; distanceIndex++)
            {
                int address = distanceTable + distanceIndex * 2;
                ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(native, CacatacMovementDefinitions.TravelDistance(distanceIndex),
                    $"compiled Cacatac travel distance {distanceIndex}");

                slot.Parameter1 = 0;
                slot.Parameter2 = distanceIndex;
                slot.XPosition = spawnX;
                initialize(slot);
                CacatacEnemyState state = enemies.CacatacStates[0]!;
                AssertEqual(unchecked((ushort)(spawnX - native)), state.MinimumXPosition,
                    $"Cacatac minimum X {spawnX:X4}/{distanceIndex}");
                AssertEqual(unchecked((ushort)(spawnX + native)), state.MaximumXPosition,
                    $"Cacatac maximum X {spawnX:X4}/{distanceIndex}");
            }
        }

        AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(6),
            "Cacatac travel distance beyond authored table");
        AssertThrows<InvalidDataException>(() => CacatacMovementDefinitions.TravelDistance(byte.MaxValue),
            "Cacatac restored travel selector does not read adjacent code");

        Console.WriteLine(
            "Cacatac travel distances: six native words and 18 wrapped production initializers pass with table reads forbidden.");
    }

    /// <summary>Address-space proxy that rejects reads from the migrated Cacatac distance table while forwarding other memory access.</summary>
    /// <param name="source">Underlying address space used for all permitted reads and writes.</param>
    private sealed class CacatacDistanceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes a cartridge-byte request through the guarded address-space read path.</summary>
        /// <param name="address">Address requested by the caller.</param>
        /// <returns>The byte from the underlying space unless the address is in the forbidden distance table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated distance-table reads and forwards every other byte request.</summary>
        /// <param name="address">Address to read.</param>
        /// <returns>The byte read from the underlying address space.</returns>
        /// <exception cref="InvalidOperationException">The address is within the former Cacatac distance-table range.</exception>
        public byte ReadByte(int address) => address is >= 0xa29f36 and < 0xa29f42
            ? throw new InvalidOperationException(
                $"Cacatac initializer attempted migrated distance read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
