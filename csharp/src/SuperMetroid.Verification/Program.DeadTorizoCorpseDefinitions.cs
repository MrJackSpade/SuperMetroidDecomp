using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the compiled dead-Torizo corpse definition with retail data and checks the production initializer.</summary>
    /// <param name="rom">Retail address space supplying the expected configuration words.</param>
    private static void VerifyDeadTorizoCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;

        // Native configuration record $A9:DD58. Word 5 (graphics initializer $DE18) has
        // no port counterpart; the remaining seven words are the definition's fields.
        const int address = 0xa9dd58;
        ushort[] expected = new ushort[7];
        for (int index = 0, word = 0; word < 8; word++)
        {
            if (word == 5)
                continue;
            expected[index++] = ReadDeadTorizoCorpseWord(rom, address + word * 2);
        }

        ushort[] actual =
        [
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.EntryCount,
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
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
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

    /// <summary>Reads a little-endian word from the retail corpse metadata tables.</summary>
    /// <param name="bus">Retail address space containing the source bytes.</param>
    /// <param name="address">Full cartridge address of the word's low byte.</param>
    /// <returns>The two bytes combined in little-endian order.</returns>
    private static ushort ReadDeadTorizoCorpseWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Rejects runtime reads from corpse metadata that has been compiled into the port.</summary>
    /// <param name="source">Address space that handles reads outside the migrated metadata ranges and receives writes.</param>
    private sealed class DeadTorizoCorpseDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the guard's address-space check.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Blocks reads from the corpse definition and wrap-offset words, delegating other addresses.</summary>
        /// <param name="address">Full cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the migrated metadata.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa9dd58 and < 0xa9dd68 or
                >= 0xa9e228 and < 0xa9e22a
                ? throw new InvalidOperationException(
                    $"Dead Torizo attempted migrated corpse metadata read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged to the wrapped address space.</summary>
        /// <param name="address">Full cartridge address to write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
