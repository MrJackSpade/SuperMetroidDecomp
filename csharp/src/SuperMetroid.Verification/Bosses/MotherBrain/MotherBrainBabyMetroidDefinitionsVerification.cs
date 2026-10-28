using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
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
        SuperMetroidAddressSpace rom)
    {
        const int populationAddress = 0xa9be28;

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        ushort[] actual =
        [
            SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(population.DefinitionId),
            population.Position.X,
            population.Position.Y,
            checked((ushort)population.InitializationParameter),
            population.Properties.ToWord(),
            population.ExtraProperties.ToWord(),
            checked((ushort)population.Parameter1),
            checked((ushort)population.Parameter2),
        ];
        // Word 3 is the record's initialization parameter: the cartridge stores the Baby's first instruction list
        // there ($CFA2), which the cutscene's typed program replaces, so the compiled record holds zero and the check
        // asserts that instead of the pointer.
        const int initializationParameterWord = 3;
        for (int word = 0; word < actual.Length; word++)
        {
            if (word == initializationParameterWord)
                continue;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((populationAddress + word * 2)), actual[word],
                $"Mother Brain Baby population word {word}");
        }
        AssertEqual(0, population.InitializationParameter,
            "Mother Brain Baby population carries no instruction-list pointer");
        AssertEqual(EnemyDefinitionId.BabyMetroidCutscene,
            population.DefinitionId,
            "Mother Brain Baby population definition");

        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));

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
            baby.DefinitionId,
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
