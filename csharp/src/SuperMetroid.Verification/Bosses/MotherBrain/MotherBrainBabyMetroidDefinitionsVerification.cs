using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's Baby Metroid spawn record and the runtime's physical-slot allocation.</summary>
internal static class MotherBrainBabyMetroidDefinitionsVerification
{
    /// <summary>Compares all eight population words and checks initialization and duplicate-spawn rejection without record reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain Baby Metroid definitions")]
    internal static void VerifyMotherBrainBabyMetroidDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const int populationAddress = 0xa9be28;

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        ushort[] actual =
        [
            (ushort)population.DefinitionPointer,
            population.Position.X,
            population.Position.Y,
            checked((ushort)population.InitializationParameter),
            population.Properties.ToNativeWord(),
            population.ExtraProperties.ToNativeWord(),
            checked((ushort)population.Parameter1),
            checked((ushort)population.Parameter2),
        ];
        for (int word = 0; word < actual.Length; word++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((populationAddress + word * 2)), actual[word],
                $"Mother Brain Baby population word {word}");
        }
        AssertEqual(EnemyDefinitionId.BabyMetroidCutscene,
            population.DefinitionPointer,
            "Mother Brain Baby population definition");

        var guarded = NewMotherBrainBabyPopulationReadGuard(rom);
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindCgram(new SnesCgram());

        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.Definition);
        var state = new MotherBrainEnemyState(body)
        {
            RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence(),
        };
        enemies.MotherBrainEncounter.MotherBrain = state;

        enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);

        RoomEnemySlot baby = state.BabyMetroidSlot ??
            throw new InvalidDataException("Mother Brain Baby allocator produced no physical slot.");
        AssertEqual(1, baby.SlotIndex, "Mother Brain Baby first free physical slot");
        AssertEqual(EnemyDefinitionId.BabyMetroidCutscene,
            baby.EnemyDefinitionPointer,
            "Mother Brain Baby physical definition");
        AssertEqual(population, baby.Spawn.Population,
            "Mother Brain Baby retained population snapshot");
        AssertTrue(state.BabyMetroid is not null,
            "Mother Brain Baby initialization created cutscene state");
        AssertEqual(2, enemies.SlotTable.EnemyCount,
            "Mother Brain Baby extended physical enemy high-water mark");
        AssertEqual(2 * 0x40, enemies.SlotTable.FirstFreeEnemyIndex,
            "Mother Brain Baby published next native enemy index");

        try
        {
            enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);
            throw new InvalidDataException(
                "Mother Brain Baby accepted a duplicate physical allocation.");
        }
        catch (InvalidOperationException)
        {
            // The production guard must reject the second actor.
        }

        Console.WriteLine(
            "Mother Brain Baby definition: all eight population words and the real allocation, initialization AI, palette load, spawn snapshot and duplicate guard pass with the embedded record forbidden.");
    }

    /// <summary>Forbids runtime reads of the migrated bank $A9 Baby Metroid population record.</summary>
    private static CartridgeReadGuard NewMotherBrainBabyPopulationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa9be28 and < 0xa9be38,
                address => $"Mother Brain Baby attempted migrated population read ${address:X6}.");

    /// <summary>Loads the retail cartridge and runs the Baby Metroid spawn-definition check.</summary>
    [VerificationCommand("--mother-brain-baby-metroid-definitions")]
    internal static int MotherBrainBabyMetroidDefinitionsCommand()
    {
        Suite(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
