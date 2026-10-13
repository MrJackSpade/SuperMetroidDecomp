using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;

/// <summary>Checks compiled Fireflea movement radii and production initialization without ROM table reads.</summary>
internal static class FirefleaMovementDefinitionsVerification
{
    /// <summary>Compares all eight native radii and rejects selectors outside the authored table.</summary>
    internal static void VerifyFirefleaMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const ushort radiusTable = 0x8d1d;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(new CartridgeReadGuard(new TestAddressSpace()).Forbid(
                address => address is >= 0xa38d1d and < 0xa38d2d,
                address => $"Fireflea initializer attempted migrated radius read ${address:X6}."));
        Action<RoomEnemySlot> initialize = enemies.FirefleaBehavior.InitializeFireflea;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        for (byte radiusIndex = 0; radiusIndex < 8; radiusIndex++)
        {
            ushort address = checked((ushort)(radiusTable + radiusIndex * 2));
            ushort native = (ushort)(SnesLittleEndian.Word(FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | address), FirefleaMovementDefinitionsRomByte(rom, 0xa30000 | (address + 1))));
            AssertEqual(native, FirefleaMovementDefinitions.Radius(radiusIndex),
                $"compiled Fireflea radius {radiusIndex}");

            slot.Parameter1 = 0x0002;
            slot.Parameter2 = (ushort)(radiusIndex << 8);
            slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
            initialize(slot);
            AssertEqual(native, (enemies.FirefleaStates[0] ?? throw new InvalidOperationException("Verification requires enemies.FirefleaStates[0].")).Radius,
                $"Fireflea initializer radius {radiusIndex}");
        }

        AssertThrows<InvalidDataException>(() => FirefleaMovementDefinitions.Radius(8),
            "Fireflea radius beyond authored table");
        AssertThrows<InvalidDataException>(() => FirefleaMovementDefinitions.Radius(byte.MaxValue),
            "Fireflea restored radius selector does not read adjacent code");

        Console.WriteLine(
            "Fireflea movement radii: all eight native words and production initializers pass with table reads forbidden.");
    }

    /// <summary>Reads one pinned bank-$A3 radius-table byte for the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Fireflea movement definitions")]
    private static byte FirefleaMovementDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
