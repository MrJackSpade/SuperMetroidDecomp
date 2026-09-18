using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadTourianCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        (DeadTourianCorpseSpecies Species, ushort EnemyDefinition, int VariantCount,
            int InstructionTable, int ConfigurationTable)[] families =
        [
            (DeadTourianCorpseSpecies.Zoomer, RoomEnemySystem.DeadZoomerDefinition,
                3, 0xa9d86a, 0xa9d870),
            (DeadTourianCorpseSpecies.Ripper, RoomEnemySystem.DeadRipperDefinition,
                2, 0xa9d897, 0xa9d89b),
            (DeadTourianCorpseSpecies.Skree, RoomEnemySystem.DeadSkreeDefinition,
                3, 0xa9d8c0, 0xa9d8c6),
        ];

        foreach (var family in families)
        {
            for (int variantIndex = 0; variantIndex < family.VariantCount; variantIndex++)
            {
                DeadTourianCorpseDefinition definition =
                    DeadTourianCorpseDefinitions.For(family.Species, variantIndex);
                AssertEqual(
                    ReadDeadTourianCorpseWord(rom, family.InstructionTable + variantIndex * 2),
                    definition.InitialInstructionPointer,
                    $"dead {family.Species} variant {variantIndex} instruction selector");
                AssertEqual(
                    ReadDeadTourianCorpseWord(rom, family.ConfigurationTable + variantIndex * 2),
                    definition.ConfigurationPointer,
                    $"dead {family.Species} variant {variantIndex} configuration selector");
                VerifyDeadTourianCorpseConfiguration(
                    rom, family.Species, variantIndex, definition);
                VerifyDeadTourianCorpseProductionInitialization(
                    rom, family.Species, family.EnemyDefinition, variantIndex, definition);
            }

            AssertThrows<ArgumentOutOfRangeException>(
                () => DeadTourianCorpseDefinitions.For(family.Species, family.VariantCount),
                $"dead {family.Species} selector past definitions");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => DeadTourianCorpseDefinitions.For((DeadTourianCorpseSpecies)3, 0),
            "unknown dead Tourian corpse species");
        Console.WriteLine(
            "Dead Tourian corpse definitions: 16 selectors, 64 configuration words, eight derived wrap offsets, and all eight real initializers pass with migrated metadata reads forbidden.");
    }

    private static void VerifyDeadTourianCorpseConfiguration(
        SuperMetroidAddressSpace rom,
        DeadTourianCorpseSpecies species,
        int variantIndex,
        DeadTourianCorpseDefinition definition)
    {
        int address = 0xa90000 | definition.ConfigurationPointer;
        ushort[] expected = new ushort[8];
        for (int index = 0; index < expected.Length; index++)
            expected[index] = ReadDeadTourianCorpseWord(rom, address + index * 2);
        ushort[] actual =
        [
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.EntryCount,
            definition.GraphicsInitializationFunction,
            definition.RotationTablePointer,
            definition.FinishFunction,
        ];
        AssertTrue(actual.AsSpan().SequenceEqual(expected),
            $"dead {species} variant {variantIndex} configuration words");

        ushort secondRotationOffset = ReadDeadTourianCorpseWord(
            rom,
            0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2)));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead {species} variant {variantIndex} wrap offset");
    }

    private static void VerifyDeadTourianCorpseProductionInitialization(
        SuperMetroidAddressSpace rom,
        DeadTourianCorpseSpecies species,
        ushort enemyDefinition,
        int variantIndex,
        DeadTourianCorpseDefinition expected)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new DeadTourianCorpseDefinitionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadTourianCorpse", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = enemyDefinition;
        slot.Parameter1 = checked((ushort)(variantIndex * 2));

        initialize(slot);

        DeadTourianCorpseEnemyState state = enemies.DeadTourianCorpses[0]!;
        AssertEqual(species, state.Species,
            $"dead {species} variant {variantIndex} species");
        AssertEqual(variantIndex, state.VariantIndex,
            $"dead {species} variant {variantIndex} index");
        AssertEqual(expected.InitialInstructionPointer, slot.CurrentInstruction,
            $"dead {species} variant {variantIndex} installed instruction");
        AssertEqual(expected.ConfigurationPointer, state.ConfigurationPointer,
            $"dead {species} variant {variantIndex} configuration");
        AssertEqual(expected.RottingTablePointer, state.TablePointer,
            $"dead {species} variant {variantIndex} rotting table");
        AssertEqual(expected.VramTransferPointer, state.VramTablePointer,
            $"dead {species} variant {variantIndex} VRAM table");
        AssertEqual(expected.CopyFunction, state.CopyFunction,
            $"dead {species} variant {variantIndex} copy callback");
        AssertEqual(expected.MoveFunction, state.MoveFunction,
            $"dead {species} variant {variantIndex} move callback");
        AssertEqual(expected.RotationTablePointer, state.RotationTablePointer,
            $"dead {species} variant {variantIndex} rotation table");
        AssertEqual(expected.FinishFunction, state.FinishFunction,
            $"dead {species} variant {variantIndex} finish callback");
        AssertEqual(expected.EntryCount, state.EntryCount,
            $"dead {species} variant {variantIndex} row count");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 1)), state.YLimit,
            $"dead {species} variant {variantIndex} Y limit");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 2)), state.LateMoveEntryIndex,
            $"dead {species} variant {variantIndex} late-move row");
        AssertEqual(expected.WrapOffset, state.WrapOffset,
            $"dead {species} variant {variantIndex} wrap offset");
    }

    private static ushort ReadDeadTourianCorpseWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class DeadTourianCorpseDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) => IsMigratedAddress(address)
            ? throw new InvalidOperationException(
                $"Dead Tourian corpse attempted migrated metadata read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsMigratedAddress(int address) =>
            address is >= 0xa9d86a and < 0xa9d876 ||
            address is >= 0xa9d897 and < 0xa9d89f ||
            address is >= 0xa9d8c0 and < 0xa9d8cc ||
            address is >= 0xa9dd88 and < 0xa9de08 ||
            address is >= 0xa9e24e and < 0xa9e250 ||
            address is >= 0xa9e254 and < 0xa9e256 ||
            address is >= 0xa9e25a and < 0xa9e25c;
    }
}
