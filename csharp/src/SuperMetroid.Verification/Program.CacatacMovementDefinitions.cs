using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;

internal static class CacatacMovementDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Cacatac movement definitions")]
    internal static void VerifyCacatacMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const int distanceTable = 0xa29f36;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewCacatacDistanceReadGuard(new TestAddressSpace()));
        Action<RoomEnemySlot> initialize = enemies.CacatacBehavior.InitializeCacatac;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        foreach (ushort spawnX in new ushort[] { 0, 0x100, ushort.MaxValue })
        {
            for (byte distanceIndex = 0; distanceIndex < 6; distanceIndex++)
            {
                int address = distanceTable + distanceIndex * 2;
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                AssertEqual(native, CacatacMovementDefinitions.TravelDistance(distanceIndex),
                    $"compiled Cacatac travel distance {distanceIndex}");

                slot.Parameter1 = 0;
                slot.Parameter2 = distanceIndex;
                slot.Position = slot.Position.WithWholeX(spawnX);
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

    private static CartridgeReadGuard NewCacatacDistanceReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa29f36 and < 0xa29f42,
                address => $"Cacatac initializer attempted migrated distance read ${address:X6}.");
}
