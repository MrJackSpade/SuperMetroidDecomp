using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares all eight compiled Fireflea radii to native data and checks the production initializer uses them.</summary>
    /// <param name="rom">The cartridge address space used for the reference radius-table words.</param>
    private static void VerifyFirefleaMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const ushort radiusTable = 0x8d1d;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new FirefleaRadiusReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeFireflea", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (byte radiusIndex = 0; radiusIndex < 8; radiusIndex++)
        {
            ushort address = checked((ushort)(radiusTable + radiusIndex * 2));
            ushort native = (ushort)(rom.ReadByte(0xa30000 | address) |
                rom.ReadByte(0xa30000 | (address + 1)) << 8);
            AssertEqual(native, FirefleaMovementDefinitions.Radius(radiusIndex),
                $"compiled Fireflea radius {radiusIndex}");

            slot.Parameter1 = 0x0002;
            slot.Parameter2 = (ushort)(radiusIndex << 8);
            slot.XPosition = 0x100;
            slot.YPosition = 0x100;
            initialize(slot);
            AssertEqual(native, enemies.FirefleaStates[0]!.Radius,
                $"Fireflea initializer radius {radiusIndex}");
        }

        AssertThrows<InvalidDataException>(() => FirefleaMovementDefinitions.Radius(8),
            "Fireflea radius beyond authored table");
        AssertThrows<InvalidDataException>(() => FirefleaMovementDefinitions.Radius(byte.MaxValue),
            "Fireflea restored radius selector does not read adjacent code");

        Console.WriteLine(
            "Fireflea movement radii: all eight native words and production initializers pass with table reads forbidden.");
    }

    /// <summary>Wraps an address space while rejecting runtime reads from the migrated Fireflea radius table.</summary>
    /// <param name="source">The underlying address space for permitted reads and forwarded writes.</param>
    private sealed class FirefleaRadiusReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the radius-table restriction.</summary>
        /// <param name="address">The address requested by the importer.</param>
        /// <returns>The underlying byte when the address is outside the migrated table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated radius table and delegates all other reads.</summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The underlying byte when the read is permitted.</returns>
        public byte ReadByte(int address) => address is >= 0xa38d1d and < 0xa38d2d
            ? throw new InvalidOperationException(
                $"Fireflea initializer attempted migrated radius read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">The destination address.</param>
        /// <param name="value">The byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
