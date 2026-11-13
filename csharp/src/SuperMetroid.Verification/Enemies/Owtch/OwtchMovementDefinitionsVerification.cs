using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Owtch;
/// <summary>Checks the Owtch travel distances and underground waits the import decodes, and the patrol they give.</summary>
internal static class OwtchMovementDefinitionsVerification
{
    /// <summary>Checks the decoded tables against the cartridge and 144 wrapped initial positions they give.</summary>
    internal static void VerifyOwtchMovementDefinitions(CartridgeImportAddressSpace rom)
    {

        Suite(nameof(VerifyOwtchTravelDistanceAlgorithm), () => VerifyOwtchTravelDistanceAlgorithm(rom));
        Suite(nameof(VerifyOwtchUndergroundTimerAlgorithm), () => VerifyOwtchUndergroundTimerAlgorithm(rom));

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
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
                    int distance = EnemySpawnSelectors.OwtchTravelDistance(distanceIndex);
                    AssertEqual(EnemySpawnSelectors.OwtchUndergroundWait(timerIndex),
                        state.UndergroundTimer,
                        $"Owtch production timer {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX - distance)), state.MinimumPosition.X,
                        $"Owtch minimum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                    AssertEqual(unchecked((ushort)(spawnX + distance)), state.MaximumPosition.X,
                        $"Owtch maximum X {spawnX:X4}/{timerIndex}/{distanceIndex}");
                }
            }
        }

        Console.WriteLine(
            "Owtch movement definitions: fourteen native words and 144 wrapped production initializers pass.");
    }

    /// <summary>Compares the eight decoded travel distances with their cartridge words; the import rejects a selector past them.</summary>
    private static void VerifyOwtchTravelDistanceAlgorithm(CartridgeImportAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Owtch, PopulationWords.Parameters(0, 8 << 8)),
            "the import rejects Owtch distance selector eight");
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Owtch, PopulationWords.Parameters(0, byte.MaxValue << 8)),
            "the import rejects the largest Owtch distance selector");
        for (byte index = 0; index < 8; index++)
        {
            int address = OwtchMovementDefinitions.DistanceReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, EnemySpawnSelectors.OwtchTravelDistance(index),
                $"decoded Owtch travel distance {index}");
        }
    }

    /// <summary>Compares the six decoded underground waits with their cartridge words; the import rejects a selector past them.</summary>
    private static void VerifyOwtchUndergroundTimerAlgorithm(CartridgeImportAddressSpace rom)
    {
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Owtch, PopulationWords.Parameters(6 << 8, 0)),
            "the import rejects Owtch underground-wait selector six");
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Owtch, PopulationWords.Parameters(byte.MaxValue << 8, 0)),
            "the import rejects the largest Owtch underground-wait selector");
        for (byte index = 0; index < 6; index++)
        {
            int address = OwtchMovementDefinitions.TimerReferenceAddress + index * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, EnemySpawnSelectors.OwtchUndergroundWait(index),
                $"decoded Owtch underground wait {index}");
        }
    }

}
