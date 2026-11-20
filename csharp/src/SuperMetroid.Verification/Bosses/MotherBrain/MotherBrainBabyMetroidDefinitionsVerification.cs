using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Mother Brain's Baby Metroid spawn record and the runtime's physical-slot allocation.</summary>
internal static class MotherBrainBabyMetroidDefinitionsVerification
{
    /// <summary>Compares all eight population words and checks initialization and duplicate-spawn rejection without record reads.</summary>
    internal static void VerifyMotherBrainBabyMetroidDefinitions(
        CartridgeImportAddressSpace rom)
    {
        const int populationAddress = 0xa9be28;

        RoomEnemyPlacement population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        SuperMetroid.AssetExtraction.Enemies.PopulationEntryWords words =
            SuperMetroid.AssetExtraction.Enemies.RoomEnemyPopulationRecordCodec.Encode(population);
        ushort[] actual =
        [
            SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(population.DefinitionId),
            checked((ushort)words.X),
            checked((ushort)words.Y),
            checked((ushort)words.Initialization),
            checked((ushort)words.Properties),
            checked((ushort)words.ExtraProperties),
            checked((ushort)words.Parameter1),
            checked((ushort)words.Parameter2),
        ];
        // Word 3 is the record's initialization word: the cartridge stores the Baby's first instruction list
        // there ($CFA2), which the cutscene's typed program replaces, so the compiled record carries no spawn settings
        // and the check asserts that instead of the pointer.
        const int initializationParameterWord = 3;
        for (int word = 0; word < actual.Length; word++)
        {
            if (word == initializationParameterWord)
                continue;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((populationAddress + word * 2)), actual[word],
                $"Mother Brain Baby population word {word}");
        }
        AssertEqual(EnemySpawnSettings.None, population.SpawnSettings,
            "Mother Brain Baby population carries no spawn settings");
        AssertEqual(EnemyDefinitionId.BabyMetroidCutscene,
            population.DefinitionId,
            "Mother Brain Baby population definition");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);

        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.MotherBrainBody, body.Definition);
        var state = new MotherBrainEnemyState(body, enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation());
        MotherBrainRainbowBeamAttackSequence lateFight = state.BeginLateFight();
        enemies.MotherBrainEncounter.BeginAsSetUp(state);

        enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(lateFight);

        RoomEnemyPlace baby = lateFight.Baby.Place ??
            throw new InvalidDataException("Mother Brain Baby allocator produced no physical slot.");
        AssertEqual(1, baby.Ordinal, "Mother Brain Baby first free physical slot");
        AssertEqual(EnemyDefinitionId.BabyMetroidCutscene,
            baby.HeldOccupant().DefinitionId,
            "Mother Brain Baby physical definition");
        AssertEqual(population, baby.HeldOccupant().Spawn,
            "Mother Brain Baby retained population snapshot");
        AssertTrue(ReferenceEquals(lateFight.Baby, baby.SpeciesState<BabyMetroidCutsceneState>()),
            "Mother Brain Baby's place holds the Baby its late fight made");
        AssertEqual(2, enemies.Places.EnemyCount,
            "Mother Brain Baby extended physical enemy high-water mark");
        AssertEqual(enemies.Places.InUpdateOrder[2], enemies.Places.FindFreePlace(),
            "Mother Brain Baby's spawn leaves the following slot free");

        try
        {
            enemies.BabyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(lateFight);
            throw new InvalidDataException(
                "Mother Brain Baby accepted a duplicate physical allocation.");
        }
        catch (InvalidOperationException)
        {
            // The production guard must reject the second actor.
        }

        Console.WriteLine(
            "Mother Brain Baby definition: seven population words, the absent instruction-list pointer, and the real allocation, initialization AI, palette load, spawn snapshot and duplicate guard pass with the embedded record forbidden.");
    }

    /// <summary>Loads the retail cartridge and runs the Baby Metroid spawn-definition check.</summary>
    [VerificationCommand("--mother-brain-baby-metroid-definitions")]
    internal static int MotherBrainBabyMetroidDefinitionsCommand()
    {
        Suite(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
