using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled Zebetite generation, health, and spawn definitions against retail tables.</summary>
internal static class ZebetiteDefinitionsVerification
{
    /// <summary>Compares table words, then exercises initialization, health handoffs, and both respawn roles.</summary>
    internal static void VerifyZebetiteDefinitions(SuperMetroidAddressSpace rom)
    {
        int[] generationTables =
            [0xa6fc03, 0xa6fc0b, 0xa6fc13, 0xa6fc1b, 0xa6fc23, 0xa6fc2b];
        const int bigHealthTable = 0xa6fd4a;
        const int linkedHealthTable = 0xa6fd54;
        const int primarySpawn = 0xa6fce1;
        const int linkedSpawn = 0xa6fcf9;

        for (ushort generation = 0; generation < 4; generation++)
        {
            ZebetiteGenerationDefinition definition =
                Zebetites.Generation(generation);
            ushort[] actual =
            [
                MotherBrainNativeWords.GenerationFlagsWord(definition.GenerationFlags),
                checked((ushort)definition.YRadius),
                ZebetiteEntry(definition.Program),
                checked((ushort)definition.XPosition),
                checked((ushort)definition.PrimaryYPosition),
                checked((ushort)definition.LinkedYPosition),
            ];
            for (int field = 0; field < actual.Length; field++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((generationTables[field] + generation * 2)),
                    actual[field],
                    $"Zebetite generation {generation} field {field}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => Zebetites.Generation(4),
            "Zebetite generation past active table");

        ushort[] healthSamples = [1000, 800, 799, 600, 599, 400, 399, 200, 199, 0];
        foreach (bool linkedPair in new[] { false, true })
        foreach (ushort health in healthSamples)
        {
            int tier = health < 200 ? 4 :
                health < 400 ? 3 :
                health < 600 ? 2 :
                health < 800 ? 1 : 0;
            int table = linkedPair ? linkedHealthTable : bigHealthTable;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((table + tier * 2)),
                ZebetiteEntry(ZebetiteAnimations.HealthProgram(linkedPair, health)),
                $"Zebetite {(linkedPair ? "linked" : "big")} health {health}");
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            RoomEnemyPopulationRecord population =
                Zebetites.SpawnPopulation(linkedHalf ? ZebetiteHalf.Linked : ZebetiteHalf.Primary);
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
            int record = linkedHalf ? linkedSpawn : primarySpawn;
            for (int word = 0; word < actual.Length; word++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + word * 2)), actual[word],
                    $"Zebetite {(linkedHalf ? "linked" : "primary")} spawn word {word}");
            }
        }

        var guarded = NewZebetiteDefinitionReadGuard(rom);

        foreach (ushort generation in new ushort[] { 0, 1, 2, 3 })
        foreach (bool linkedHalf in new[] { false, true })
        {
            var enemies = NewZebetiteDefinitionSystem(
                generation);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.SetSpawnParameter1(EnemyDefinitionId.Zebetite, linkedHalf ? (ushort)2 : (ushort)0);
            slot.Bind(EnemyDefinitionId.Zebetite, slot.Definition);
            enemies.ZebetiteBehavior.InitializeZebetite(slot);
            ZebetiteEnemyState state = enemies.ZebetiteStates[0] ??
                throw new InvalidDataException("Zebetite initializer omitted typed state.");
            ZebetiteGenerationDefinition expected = Zebetites.Generation(generation);
            AssertEqual(expected.GenerationFlags, state.GenerationFlags,
                $"production Zebetite generation {generation} flags");
            AssertEqual(expected.YRadius, slot.Radius.Y,
                $"production Zebetite generation {generation} radius");
            AssertEqual(AnimationProgramCursor<ZebetiteProgram>.Start(expected.Program), state.Cursor,
                $"production Zebetite generation {generation} program");
            AssertEqual(expected.XPosition, slot.Position.X.Whole,
                $"production Zebetite generation {generation} X");
            AssertEqual(Zebetites.YPosition(expected, linkedHalf), slot.Position.Y.Whole,
                $"production Zebetite generation {generation} Y");

            foreach (ushort health in healthSamples)
            {
                slot.Health = health;
                ZebetiteBehavior.SelectZebetiteHealthAnimation(slot, state);
                AssertEqual(ZebetiteAnimations.HealthProgram(
                        (expected.GenerationFlags & ZebetiteGenerationFlags.LinkedPair) != 0,
                        health),
                    state.Cursor.Program,
                    $"production Zebetite generation {generation} health {health}");
            }
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            var enemies = NewZebetiteDefinitionSystem(0);
            RoomEnemySlot slot = enemies.ZebetiteBehavior.SpawnZebetite(linkedHalf ? ZebetiteHalf.Linked : ZebetiteHalf.Primary);
            AssertEqual(Zebetites.SpawnPopulation(linkedHalf ? ZebetiteHalf.Linked : ZebetiteHalf.Primary), slot.Spawn.Population,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} population");
            AssertEqual(linkedHalf ? ZebetiteHalf.Linked : ZebetiteHalf.Primary, slot.SpawnSettings<ZebetiteSpawnSettings>().Half,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} role");
        }

        Console.WriteLine(
            "Zebetite definitions: 24 generation words, ten health selectors, both embedded populations, eight initializers, 80 health handoffs and both real respawns pass with all fixed source ranges forbidden.");
    }

    /// <summary>Supplies event bits selecting one Zebetite generation in an isolated enemy runtime.</summary>
    private static RoomEnemySystem NewZebetiteDefinitionSystem(
        ushort generation)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().HasEvent = eventNumber => eventNumber switch
            {
                EventNumber.ZebetiteDestroyedBit2 => (generation & 4) != 0,
                EventNumber.ZebetiteDestroyedBit1 => (generation & 2) != 0,
                EventNumber.ZebetiteDestroyedBit0 => (generation & 1) != 0,
                EventNumber.ZebesAwake or
                    EventNumber.ShitroidAteSidehopper or
                    EventNumber.MotherBrainGlassDestroyed or
                    EventNumber.PhantoonStatueGrey or
                    EventNumber.RidleyStatueGrey or
                    EventNumber.DraygonStatueGrey or
                    EventNumber.KraidStatueGrey or
                    EventNumber.TourianUnlocked or
                    EventNumber.MaridiaNoobTubeBroken or
                    EventNumber.LowerNorfairChozoLoweredAcid or
                    EventNumber.ShaktoolClearedPath or
                    EventNumber.ZebesTimebombSet or
                    EventNumber.CrittersEscaped or
                    EventNumber.FirstMetroidHallCleared or
                    EventNumber.FirstMetroidShaftCleared or
                    EventNumber.SecondMetroidHallCleared or
                    EventNumber.SecondMetroidShaftCleared or
                    EventNumber.Unused14 or
                    EventNumber.OutranSpeedBoosterLavaquake => false,
                _ => false,
            };
        return enemies;
    }

    /// <summary>Rejects runtime reads from migrated generation, population, and health tables.</summary>
    private static CartridgeReadGuard NewZebetiteDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa6fc03 and < 0xa6fc33 or
                >= 0xa6fce1 and < 0xa6fcf1 or
                >= 0xa6fcf9 and < 0xa6fd09 or
                >= 0xa6fd4a and < 0xa6fd5e,
            address => $"Zebetite attempted migrated definition read ${address:X6}.");

    /// <summary>Runs the Zebetite geometry and selector comparison from the command dispatcher.</summary>
    [VerificationCommand("--lookup-stream5-zebetite-geometry")]
    internal static int LookupStream5ZebetiteGeometryCommand()
    {
        Suite(nameof(VerifyZebetiteDefinitions), () => VerifyZebetiteDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>The native bank-$A6 list of <paramref name="program"/>, which the generation and health tables select.</summary>
    private static ushort ZebetiteEntry(ZebetiteProgram program) =>
        MotherBrainNativeListAddresses.ZebetiteEntries.Single(entry => entry.Program == program).Entry;
}
