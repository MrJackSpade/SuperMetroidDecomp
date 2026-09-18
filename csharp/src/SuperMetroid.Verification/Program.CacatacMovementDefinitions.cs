using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
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

    private sealed class CacatacDistanceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa29f36 and < 0xa29f42
            ? throw new InvalidOperationException(
                $"Cacatac initializer attempted migrated distance read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
