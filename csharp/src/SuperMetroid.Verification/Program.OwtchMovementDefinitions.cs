using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents owtch movement definitions verification.</summary>
internal static class OwtchMovementDefinitionsVerification
{
    /// <summary>Verifies owtch movement definitions.</summary>
    internal static void VerifyOwtchMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyOwtchTravelDistanceAlgorithm), () => VerifyOwtchTravelDistanceAlgorithm(rom));
        Suite(nameof(VerifyOwtchUndergroundTimerAlgorithm), () => VerifyOwtchUndergroundTimerAlgorithm(rom));

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewOwtchMovementReadGuard(new TestAddressSpace()));
        Action<RoomEnemySlot> initialize = enemies.OwtchBehavior.InitializeOwtch;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];

        foreach (ushort spawnX in new ushort[] { 0, 0x0100, ushort.MaxValue })
        {
            for (byte timerIndex = 0; timerIndex < 6; timerIndex++)
            {
                for (byte distanceIndex = 0; distanceIndex < 8; distanceIndex++)
                {
                    byte initialState = (byte)((timerIndex + distanceIndex) % 5);
                    slot.Parameter1 = (ushort)(SnesLittleEndian.Word(initialState, timerIndex));
                    slot.Parameter2 = (ushort)(distanceIndex << 8);
                    slot.Position = slot.Position.WithWhole(new(spawnX, 0x0100));
                    initialize(slot);

                    OwtchEnemyState state = enemies.OwtchStates[0]!;
                    ushort distance = OwtchMovementDefinitions.TravelDistance(distanceIndex);
                    AssertEqual(OwtchMovementDefinitions.UndergroundTimer(timerIndex),
                        state.UndergroundTimer,
                        $"Owtch production timer {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX - distance)), state.MinimumXPosition,
                        $"Owtch minimum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX + distance)), state.MaximumXPosition,
                        $"Owtch maximum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                }
            }
        }

        Console.WriteLine(
            "Owtch movement definitions: fourteen native words and 144 wrapped production initializers pass with table reads forbidden.");
    }

    /// <summary>Verifies owtch travel distance algorithm.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Owtch travel distance algorithm")]
    private static void VerifyOwtchTravelDistanceAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(8), "Owtch distance upper bound");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.TravelDistance(byte.MaxValue), "Owtch distance maximum invalid input");
        for (byte index = 0; index < 8; index++)
        {
            int address = OwtchMovementDefinitions.DistanceReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, OwtchMovementDefinitions.TravelDistance(index),
                $"compiled Owtch travel distance {index}");
        }
    }

    /// <summary>Verifies owtch underground timer algorithm.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Owtch underground timer algorithm")]
    private static void VerifyOwtchUndergroundTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(6), "Owtch timer upper bound");
        AssertThrows<InvalidDataException>(() => OwtchMovementDefinitions.UndergroundTimer(byte.MaxValue), "Owtch timer maximum invalid input");
        for (byte index = 0; index < 6; index++)
        {
            int address = OwtchMovementDefinitions.TimerReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, OwtchMovementDefinitions.UndergroundTimer(index),
                $"compiled Owtch underground timer {index}");
        }
    }

    /// <summary>Creates owtch movement read guard.</summary>
    private static CartridgeReadGuard NewOwtchMovementReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa2a3dd and < 0xa2a3f9,
                address => $"Owtch movement attempted migrated definition read ${address:X6}.");
}
