using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainBabyMetroidDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const int populationAddress = 0xa9be28;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
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
        for (int word = 0; word < actual.Length; word++)
        {
            AssertEqual(ReadWord(rom, populationAddress + word * 2), actual[word],
                $"Mother Brain Baby population word {word}");
        }
        AssertEqual(MotherBrainBabyMetroidDefinitions.EnemyDefinition,
            population.DefinitionPointer,
            "Mother Brain Baby population definition");

        var guarded = new MotherBrainBabyPopulationReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_cgram", instanceFlags)!
            .SetValue(enemies, new SnesCgram());

        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = 0xec7f;
        var state = new MotherBrainEnemyState(body)
        {
            RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence(),
        };
        typeof(RoomEnemySystem).GetField("_motherBrain", instanceFlags)!
            .SetValue(enemies, state);

        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnMotherBrainBabyMetroid",
            instanceFlags)!;
        spawn.Invoke(enemies, [state]);

        RoomEnemySlot baby = state.BabyMetroidSlot ??
            throw new InvalidDataException("Mother Brain Baby allocator produced no physical slot.");
        AssertEqual(1, baby.SlotIndex, "Mother Brain Baby first free physical slot");
        AssertEqual(MotherBrainBabyMetroidDefinitions.EnemyDefinition,
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

    private sealed class MotherBrainBabyPopulationReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa9be28 and < 0xa9be38
                ? throw new InvalidOperationException(
                    $"Mother Brain Baby attempted migrated population read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
