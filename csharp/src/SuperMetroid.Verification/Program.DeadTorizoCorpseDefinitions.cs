using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDeadTorizoCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;
        AssertEqual((ushort)0xdd58, definition.ConfigurationPointer,
            "dead Torizo configuration identity");

        int address = 0xa90000 | definition.ConfigurationPointer;
        ushort[] expected = new ushort[8];
        for (int index = 0; index < expected.Length; index++)
            expected[index] = ReadDeadTorizoCorpseWord(rom, address + index * 2);

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
            "dead Torizo corpse configuration");

        ushort secondRotationOffset = ReadDeadTorizoCorpseWord(
            rom,
            0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2)));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            "dead Torizo corpse wrap offset");

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new DeadTorizoCorpseDefinitionReadGuard(rom));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadTorizo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        initialize(slot);

        DeadTorizoEnemyState state = enemies.DeadTorizo!;
        AssertEqual(definition.RottingTablePointer, state.TablePointer,
            "dead Torizo production rotting table");
        AssertEqual(definition.VramTransferPointer, state.VramTablePointer,
            "dead Torizo production VRAM table");
        AssertEqual(definition.CopyFunction, state.CopyFunction,
            "dead Torizo production copy callback");
        AssertEqual(definition.MoveFunction, state.MoveFunction,
            "dead Torizo production move callback");
        AssertEqual(definition.RotationTablePointer, state.RotationTablePointer,
            "dead Torizo production rotation table");
        AssertEqual(definition.FinishFunction, state.FinishFunction,
            "dead Torizo production finish callback");
        AssertEqual(definition.EntryCount, state.EntryCount,
            "dead Torizo production row count");
        AssertEqual(unchecked((ushort)(definition.EntryCount - 1)), state.YLimit,
            "dead Torizo production Y limit");
        AssertEqual(unchecked((ushort)(definition.EntryCount - 2)), state.LateMoveEntryIndex,
            "dead Torizo production late-move row");
        AssertEqual(definition.WrapOffset, state.WrapOffset,
            "dead Torizo production wrap offset");

        Console.WriteLine(
            "Dead Torizo corpse definitions: all eight native configuration words, the derived wrap offset, and the real initializer pass with migrated metadata reads forbidden.");
    }

    private static ushort ReadDeadTorizoCorpseWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class DeadTorizoCorpseDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa9dd58 and < 0xa9dd68 or
                >= 0xa9e228 and < 0xa9e22a
                ? throw new InvalidOperationException(
                    $"Dead Torizo attempted migrated corpse metadata read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
