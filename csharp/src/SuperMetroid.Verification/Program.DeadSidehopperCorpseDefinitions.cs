using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the two dead-sidehopper metadata records against retail data and checks their production initialization paths.</summary>
    /// <param name="rom">Retail address space supplying the expected cartridge words.</param>
    private static void VerifyDeadSidehopperCorpseDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyAlive,
            expectedConfigurationPointer: 0xdd68));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(
            rom,
            DeadSidehopperCorpseDefinitions.InitiallyDead,
            expectedConfigurationPointer: 0xdd78));

        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            rom,
            parameter1: 0,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive));
        Suite(nameof(VerifyProductionInitialization), () => VerifyProductionInitialization(
            rom,
            parameter1: 2,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead));

        Console.WriteLine(
            "Dead sidehopper corpse definitions: all 14 consumed native configuration words, two derived wrap offsets, and both production initializers pass with migrated metadata reads forbidden.");
    }

    /// <summary>Compares one compiled corpse definition with its native configuration words and derived wrap offset.</summary>
    /// <param name="rom">Retail address space containing the native configuration and rotation-table data.</param>
    /// <param name="definition">Compiled metadata record under verification.</param>
    /// <param name="expectedConfigurationPointer">Native bank-relative address of the expected configuration record.</param>
    private static void VerifyDefinition(
        SuperMetroidAddressSpace rom,
        DeadSidehopperCorpseDefinition definition,
        ushort expectedConfigurationPointer)
    {
        AssertEqual(expectedConfigurationPointer, definition.ConfigurationPointer,
            "dead sidehopper configuration identity");
        int address = 0xa90000 | definition.ConfigurationPointer;
        // Word five, the graphics-initialization callback, has no compiled consumer.
        ushort[] expected = [.. new[] { 0, 1, 2, 3, 4, 6, 7 }
            .Select(index => ReadDeadSidehopperCorpseWord(rom, address + index * 2))];

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
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4}");

        ushort secondRotationOffset = ReadDeadSidehopperCorpseWord(
            rom,
            0xa90000 | unchecked((ushort)(definition.RotationTablePointer + 2)));
        AssertEqual(unchecked((ushort)(secondRotationOffset - 12)), definition.WrapOffset,
            $"dead sidehopper configuration $A9:{definition.ConfigurationPointer:X4} wrap offset");
    }

    /// <summary>Runs the production initializer for a selected dead-sidehopper variant and checks the populated enemy state.</summary>
    /// <param name="rom">Retail source wrapped to detect reads from migrated definition ranges.</param>
    /// <param name="parameter1">Enemy parameter selecting the initialization branch.</param>
    /// <param name="graphicsVariant">Expected graphics variant assigned by that branch.</param>
    /// <param name="expected">Compiled definition expected to populate the enemy state.</param>
    private static void VerifyProductionInitialization(
        SuperMetroidAddressSpace rom,
        ushort parameter1,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition expected)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
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

    /// <summary>Reads one little-endian word from the retail dead-sidehopper definition data.</summary>
    /// <param name="bus">Address space used to fetch the two constituent bytes.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The decoded 16-bit value.</returns>
    private static ushort ReadDeadSidehopperCorpseWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps the retail bus and rejects runtime reads from dead-sidehopper metadata already migrated into compiled definitions.</summary>
    /// <param name="source">Underlying address space for permitted reads and forwarded writes.</param>
    private sealed class DeadSidehopperCorpseDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the protected-range check.</summary>
        /// <param name="address">Cartridge address requested by the importer.</param>
        /// <returns>The source byte when the requested range is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a source byte unless its address belongs to migrated corpse metadata.</summary>
        /// <param name="address">Address requested by the runtime.</param>
        /// <returns>The source byte for an address outside protected metadata.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to a migrated definition or rotation offset that must not be reread.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa9dd68 and < 0xa9dd88 ||
            address is >= 0xa9e242 and < 0xa9e244
                ? throw new InvalidOperationException(
                    $"Dead sidehopper attempted migrated corpse metadata read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged to the wrapped retail address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte value to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
