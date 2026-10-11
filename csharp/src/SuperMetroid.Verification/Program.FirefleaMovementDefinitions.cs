using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFirefleaMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const ushort radiusTable = 0x8d1d;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CartridgeReadGuard(new TestAddressSpace()).Forbid(
                address => address is >= 0xa38d1d and < 0xa38d2d,
                address => $"Fireflea initializer attempted migrated radius read ${address:X6}."));
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
            slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
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
}
