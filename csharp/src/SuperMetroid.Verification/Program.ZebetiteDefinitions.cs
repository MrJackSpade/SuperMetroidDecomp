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

/// <summary>Checks compiled Zebetite generation, health, and spawn definitions against retail tables.</summary>
internal static class ZebetiteDefinitionsVerification
{
    /// <summary>Compares table words, then exercises initialization, health handoffs, and both respawn roles.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Zebetite definitions")]
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
                ZebetiteDefinitions.Generation(generation);
            ushort[] actual =
            [
                definition.GenerationFlags,
                definition.YRadius,
                definition.InstructionList,
                definition.XPosition,
                definition.PrimaryYPosition,
                definition.LinkedYPosition,
            ];
            for (int field = 0; field < actual.Length; field++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((generationTables[field] + generation * 2)),
                    actual[field],
                    $"Zebetite generation {generation} field {field}");
            }
        }

        AssertThrows<InvalidDataException>(
            () => ZebetiteDefinitions.Generation(4),
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
                ZebetiteDefinitions.HealthInstruction(linkedPair, health),
                $"Zebetite {(linkedPair ? "linked" : "big")} health {health}");
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            RoomEnemyPopulationRecord population =
                ZebetiteDefinitions.SpawnPopulation(linkedHalf);
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
                guarded,
                generation);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = linkedHalf ? (ushort)2 : (ushort)0;
            enemies.ZebetiteBehavior.InitializeZebetite(slot);
            ZebetiteEnemyState state = enemies.ZebetiteStates[0] ??
                throw new InvalidDataException("Zebetite initializer omitted typed state.");
            ZebetiteGenerationDefinition expected = ZebetiteDefinitions.Generation(generation);
            AssertEqual(expected.GenerationFlags, state.GenerationFlags,
                $"production Zebetite generation {generation} flags");
            AssertEqual(expected.YRadius, slot.Radius.Y,
                $"production Zebetite generation {generation} radius");
            AssertEqual(expected.InstructionList, slot.CurrentInstruction,
                $"production Zebetite generation {generation} instruction");
            AssertEqual(expected.XPosition, slot.Position.X.Whole,
                $"production Zebetite generation {generation} X");
            AssertEqual(expected.YPosition(linkedHalf), slot.Position.Y.Whole,
                $"production Zebetite generation {generation} Y");

            foreach (ushort health in healthSamples)
            {
                slot.Health = health;
                ZebetiteBehavior.SelectZebetiteHealthAnimation(slot, state);
                AssertEqual(ZebetiteDefinitions.HealthInstruction(
                        (expected.GenerationFlags & 0x8000) != 0,
                        health),
                    slot.CurrentInstruction,
                    $"production Zebetite generation {generation} health {health}");
            }
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            var enemies = NewZebetiteDefinitionSystem(guarded, 0);
            RoomEnemySlot slot = (RoomEnemySlot)enemies.ZebetiteBehavior.SpawnZebetite(linkedHalf);
            AssertEqual(ZebetiteDefinitions.SpawnPopulation(linkedHalf), slot.Spawn.Population,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} population");
            AssertEqual(linkedHalf, slot.Parameter1 != 0,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} role");
        }

        Console.WriteLine(
            "Zebetite definitions: 24 generation words, ten health selectors, both embedded populations, eight initializers, 80 health handoffs and both real respawns pass with all fixed source ranges forbidden.");
    }

    /// <summary>Supplies event bits selecting one Zebetite generation in an isolated enemy runtime.</summary>
    private static RoomEnemySystem NewZebetiteDefinitionSystem(
        ISnesAddressSpace bus,
        ushort generation)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().HasEvent = (Func<EventNumber, bool>)(eventNumber => eventNumber switch
            {
                EventNumber.ZebetiteDestroyedBit2 => (generation & 4) != 0,
                EventNumber.ZebetiteDestroyedBit1 => (generation & 2) != 0,
                EventNumber.ZebetiteDestroyedBit0 => (generation & 1) != 0,
                _ => false,
            });
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
}
