using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies.Owtch;
/// <summary>Checks Owtch timer and travel-distance definitions through real initialization.</summary>
internal static class OwtchMovementDefinitionsVerification
{
    /// <summary>Checks 144 wrapped initial positions while compiled movement-table reads are forbidden.</summary>
    internal static void VerifyOwtchMovementDefinitions(SuperMetroidAddressSpace rom)
    {

        Suite(nameof(VerifyOwtchTravelDistanceAlgorithm), () => VerifyOwtchTravelDistanceAlgorithm(rom));
        Suite(nameof(VerifyOwtchUndergroundTimerAlgorithm), () => VerifyOwtchUndergroundTimerAlgorithm(rom));

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot> initialize = OwtchBehavior.InitializeOwtch;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Owtch, slot.Definition);

        foreach (ushort spawnX in new ushort[] { 0, 0x0100, ushort.MaxValue })
        {
            for (byte timerIndex = 0; timerIndex < 6; timerIndex++)
            {
                for (byte distanceIndex = 0; distanceIndex < 8; distanceIndex++)
                {
                    byte initialState = (byte)((timerIndex + distanceIndex) % 5);
                    slot.SetSpawnParameter1(EnemyDefinitionId.Owtch, LittleEndianBytes.Word(initialState, timerIndex));
                    slot.SetSpawnParameter2(EnemyDefinitionId.Owtch, (ushort)(distanceIndex << 8));
                    slot.Position = slot.Position.WithWhole(new(spawnX, 0x0100));
                    initialize(slot);

                    OwtchEnemyState state = (enemies.OwtchStates[0] ?? throw new InvalidOperationException("Verification requires enemies.OwtchStates[0]."));
                    ushort distance = OwtchPatrol.TravelDistance(distanceIndex);
                    AssertEqual(OwtchPatrol.UndergroundTimer(timerIndex),
                        state.UndergroundTimer.RawValue,
                        $"Owtch production timer {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX - distance)), state.MinimumPosition.X,
                        $"Owtch minimum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX + distance)), state.MaximumPosition.X,
                        $"Owtch maximum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                }
            }
        }

        Console.WriteLine(
            "Owtch movement definitions: fourteen native words and 144 wrapped production initializers pass with table reads forbidden.");
    }

    /// <summary>Compares eight indexed travel distances with their original cartridge words.</summary>
    private static void VerifyOwtchTravelDistanceAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchPatrol.TravelDistance(8), "Owtch distance upper bound");
        AssertThrows<InvalidDataException>(() => OwtchPatrol.TravelDistance(byte.MaxValue), "Owtch distance maximum invalid input");
        for (byte index = 0; index < 8; index++)
        {
            int address = OwtchMovementDefinitions.DistanceReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, OwtchPatrol.TravelDistance(index),
                $"compiled Owtch travel distance {index}");
        }
    }

    /// <summary>Compares six underground timers with their original cartridge words.</summary>
    private static void VerifyOwtchUndergroundTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => OwtchPatrol.UndergroundTimer(6), "Owtch timer upper bound");
        AssertThrows<InvalidDataException>(() => OwtchPatrol.UndergroundTimer(byte.MaxValue), "Owtch timer maximum invalid input");
        for (byte index = 0; index < 6; index++)
        {
            int address = OwtchMovementDefinitions.TimerReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, OwtchPatrol.UndergroundTimer(index),
                $"compiled Owtch underground timer {index}");
        }
    }

}
