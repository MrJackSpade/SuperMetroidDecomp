using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadSidehopperCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyAlive,
            expectedConfigurationPointer: 0xdd68);
        VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyDead,
            expectedConfigurationPointer: 0xdd78);

        VerifyProductionInitialization(
            rom,
            parameter1: 0,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive);
        VerifyProductionInitialization(
            rom,
            parameter1: 2,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead);

        Console.WriteLine(
            "Dead sidehopper corpse definitions: all 16 native configuration words, two derived wrap offsets, and both production initializers pass with migrated metadata reads forbidden.");
    }

    private static void VerifyDefinition(
        SuperMetroidAddressSpace rom,
        DeadSidehopperCorpseDefinition definition,
        ushort expectedConfigurationPointer)
    {
        AssertEqual(expectedConfigurationPointer, definition.ConfigurationPointer,
            "dead sidehopper configuration identity");
        int address = 0xa90000 | definition.ConfigurationPointer;
        ushort[] expected = new ushort[8];
        for (int index = 0; index < expected.Length; index++)
            expected[index] = ReadDeadSidehopperCorpseWord(rom, address + index * 2);

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
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4}");

        ushort secondRotationOffset = ReadDeadSidehopperCorpseWord(
            rom,
            0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2)));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4} wrap offset");
    }

    private static void VerifyProductionInitialization(
        SuperMetroidAddressSpace rom,
        ushort parameter1,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition expected)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new DeadSidehopperCorpseDefinitionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadSidehopper", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Parameter1 = parameter1;

        initialize(slot);

        DeadSidehopperEnemyState state = enemies.DeadSidehoppers[0]!;
        AssertEqual(graphicsVariant, state.GraphicsVariant,
            $"dead sidehopper parameter {parameter1} graphics variant");
        AssertEqual(expected.ConfigurationPointer, state.ConfigurationPointer,
            $"dead sidehopper parameter {parameter1} configuration");
        AssertEqual(expected.RottingTablePointer, state.TablePointer,
            $"dead sidehopper parameter {parameter1} rotting table");
        AssertEqual(expected.VramTransferPointer, state.VramTablePointer,
            $"dead sidehopper parameter {parameter1} VRAM table");
        AssertEqual(expected.CopyFunction, state.CopyFunction,
            $"dead sidehopper parameter {parameter1} copy callback");
        AssertEqual(expected.MoveFunction, state.MoveFunction,
            $"dead sidehopper parameter {parameter1} move callback");
        AssertEqual(expected.RotationTablePointer, state.RotationTablePointer,
            $"dead sidehopper parameter {parameter1} rotation table");
        AssertEqual(expected.FinishFunction, state.FinishFunction,
            $"dead sidehopper parameter {parameter1} finish callback");
        AssertEqual(expected.EntryCount, state.EntryCount,
            $"dead sidehopper parameter {parameter1} row count");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 1)), state.YLimit,
            $"dead sidehopper parameter {parameter1} Y limit");
        AssertEqual(unchecked((ushort)(expected.EntryCount - 2)), state.LateMoveEntryIndex,
            $"dead sidehopper parameter {parameter1} late-move row");
        AssertEqual(expected.WrapOffset, state.WrapOffset,
            $"dead sidehopper parameter {parameter1} wrap offset");
    }

    private static ushort ReadDeadSidehopperCorpseWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class DeadSidehopperCorpseDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa9dd68 and < 0xa9dd88 ||
            address is >= 0xa9e242 and < 0xa9e244
                ? throw new InvalidOperationException(
                    $"Dead sidehopper attempted migrated corpse metadata read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
