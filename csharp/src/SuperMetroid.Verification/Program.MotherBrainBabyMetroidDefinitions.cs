using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class MotherBrainBabyMetroidDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain Baby Metroid definitions")]
    internal static void VerifyMotherBrainBabyMetroidDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const int populationAddress = 0xa9be28;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        ushort[] actual =
        [
            (ushort)population.DefinitionPointer,
            population.Position.X,
            population.Position.Y,
            population.InitializationParameter,
            population.Properties,
            population.ExtraProperties,
            population.Parameter1,
            population.Parameter2,
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
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_cgram", instanceFlags)!
            .SetValue(enemies, new SnesCgram());

        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.MotherBrainBody;
        var state = new MotherBrainEnemyState(body)
        {
            RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence(),
        };
        typeof(RoomEnemySystem).GetField("<MotherBrain>k__BackingField", instanceFlags)!
            .SetValue(enemies, state);

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainBabyMetroid",
            instanceFlags)!;
        spawn.Invoke(enemies, [state]);

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
        AssertEqual(2, enemies.EnemyCount,
            "Mother Brain Baby extended physical enemy high-water mark");
        AssertEqual(2 * 0x40, enemies.FirstFreeEnemyIndex,
            "Mother Brain Baby published next native enemy index");

        try
        {
            spawn.Invoke(enemies, [state]);
            throw new InvalidDataException(
                "Mother Brain Baby accepted a duplicate physical allocation.");
        }
        catch (TargetInvocationException exception)
            when (exception.InnerException is InvalidOperationException)
        {
            // The production guard must reject the second actor, rather than merely
            // failing later for an unrelated reflected-call error.
        }

        Console.WriteLine(
            "Mother Brain Baby definition: all eight population words and the real allocation, initialization AI, palette load, spawn snapshot and duplicate guard pass with the embedded record forbidden.");
    }

    private static CartridgeReadGuard NewMotherBrainBabyPopulationReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa9be28 and < 0xa9be38,
                address => $"Mother Brain Baby attempted migrated population read ${address:X6}.");

    [VerificationCommand("--mother-brain-baby-metroid-definitions")]
    internal static int MotherBrainBabyMetroidDefinitionsCommand()
    {
        Suite(nameof(VerifyMotherBrainBabyMetroidDefinitions), () => VerifyMotherBrainBabyMetroidDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
