using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks compiled Zebetite generation, health, and spawn data against ROM and exercises the production consumers.</summary>
    /// <param name="rom">Retail address space supplying the source definition words.</param>
    private static void VerifyZebetiteDefinitions(SuperMetroidAddressSpace rom)
    {
        int[] generationTables =
            [0xa6fc03, 0xa6fc0b, 0xa6fc13, 0xa6fc1b, 0xa6fc23, 0xa6fc2b];
        const int bigHealthTable = 0xa6fd4a;
        const int linkedHealthTable = 0xa6fd54;
        const int primarySpawn = 0xa6fce1;
        const int linkedSpawn = 0xa6fcf9;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

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
                AssertEqual(ReadWord(rom, generationTables[field] + generation * 2),
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
            AssertEqual(ReadWord(rom, table + tier * 2),
                ZebetiteDefinitions.HealthInstruction(linkedPair, health),
                $"Zebetite {(linkedPair ? "linked" : "big")} health {health}");
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            RoomEnemyPopulationRecord population =
                ZebetiteDefinitions.SpawnPopulation(linkedHalf);
            ushort[] actual =
            [
                population.DefinitionPointer,
                population.XPosition,
                population.YPosition,
                population.InitializationParameter,
                population.Properties,
                population.ExtraProperties,
                population.Parameter1,
                population.Parameter2,
            ];
            int record = linkedHalf ? linkedSpawn : primarySpawn;
            for (int word = 0; word < actual.Length; word++)
            {
                AssertEqual(ReadWord(rom, record + word * 2), actual[word],
                    $"Zebetite {(linkedHalf ? "linked" : "primary")} spawn word {word}");
            }
        }

        var guarded = new ZebetiteDefinitionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeZebetite",
            instanceFlags)!;
        MethodInfo selectHealth = typeof(RoomEnemySystem).GetMethod(
            "SelectZebetiteHealthAnimation",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnZebetite",
            instanceFlags)!;

        foreach (ushort generation in new ushort[] { 0, 1, 2, 3 })
        foreach (bool linkedHalf in new[] { false, true })
        {
            var enemies = NewZebetiteDefinitionSystem(
                guarded,
                generation,
                instanceFlags);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = linkedHalf ? (ushort)2 : (ushort)0;
            initialize.Invoke(enemies, [slot]);
            ZebetiteEnemyState state = enemies.ZebetiteStates[0] ??
                throw new InvalidDataException("Zebetite initializer omitted typed state.");
            ZebetiteGenerationDefinition expected = ZebetiteDefinitions.Generation(generation);
            AssertEqual(expected.GenerationFlags, state.GenerationFlags,
                $"production Zebetite generation {generation} flags");
            AssertEqual(expected.YRadius, slot.YRadius,
                $"production Zebetite generation {generation} radius");
            AssertEqual(expected.InstructionList, slot.CurrentInstruction,
                $"production Zebetite generation {generation} instruction");
            AssertEqual(expected.XPosition, slot.XPosition,
                $"production Zebetite generation {generation} X");
            AssertEqual(expected.YPosition(linkedHalf), slot.YPosition,
                $"production Zebetite generation {generation} Y");

            foreach (ushort health in healthSamples)
            {
                slot.Health = health;
                selectHealth.Invoke(null, [slot, state]);
                AssertEqual(ZebetiteDefinitions.HealthInstruction(
                        (expected.GenerationFlags & 0x8000) != 0,
                        health),
                    slot.CurrentInstruction,
                    $"production Zebetite generation {generation} health {health}");
            }
        }

        foreach (bool linkedHalf in new[] { false, true })
        {
            var enemies = NewZebetiteDefinitionSystem(guarded, 0, instanceFlags);
            RoomEnemySlot slot = (RoomEnemySlot)spawn.Invoke(enemies, [linkedHalf])!;
            AssertEqual(ZebetiteDefinitions.SpawnPopulation(linkedHalf), slot.Spawn.Population,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} population");
            AssertEqual(linkedHalf, slot.Parameter1 != 0,
                $"production Zebetite {(linkedHalf ? "linked" : "primary")} role");
        }

        Console.WriteLine(
            "Zebetite definitions: 24 generation words, ten health selectors, both embedded populations, eight initializers, 80 health handoffs and both real respawns pass with all fixed source ranges forbidden.");
    }

    /// <summary>Creates a room enemy system with a guarded bus and destruction events matching one generation index.</summary>
    /// <param name="bus">Address space used by the system for permitted reads.</param>
    /// <param name="generation">Three-bit destroyed-state selector controlling the Zebetite event queries.</param>
    /// <param name="instanceFlags">Reflection flags used to install the test bus and event callback.</param>
    /// <returns>The configured enemy system used to exercise Zebetite initialization and spawning.</returns>
    private static RoomEnemySystem NewZebetiteDefinitionSystem(
        ISnesAddressSpace bus,
        ushort generation,
        BindingFlags instanceFlags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_hasEvent", instanceFlags)!.SetValue(
            enemies,
            (Func<EventNumber, bool>)(eventNumber => eventNumber switch
            {
                EventNumber.ZebetiteDestroyedBit2 => (generation & 4) != 0,
                EventNumber.ZebetiteDestroyedBit1 => (generation & 2) != 0,
                EventNumber.ZebetiteDestroyedBit0 => (generation & 1) != 0,
                _ => false,
            }));
        return enemies;
    }

    /// <summary>Rejects runtime reads from the migrated Zebetite generation, spawn, and health tables.</summary>
    /// <param name="source">Address space that supplies unrelated reads and accepts writes.</param>
    private sealed class ZebetiteDefinitionReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the same table-range guard.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Blocks reads in compiled Zebetite definition ranges and delegates all other addresses.</summary>
        /// <param name="address">Full cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the migrated tables.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa6fc03 and < 0xa6fc33 or
                >= 0xa6fce1 and < 0xa6fcf1 or
                >= 0xa6fcf9 and < 0xa6fd09 or
                >= 0xa6fd4a and < 0xa6fd5e
                ? throw new InvalidOperationException(
                    $"Zebetite attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Full cartridge address to write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
