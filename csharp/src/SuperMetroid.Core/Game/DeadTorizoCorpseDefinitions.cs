namespace SuperMetroid.Core.Game;

/// <summary>Compiled bank-$A9 corpse-rotting metadata for the dead Torizo actor.</summary>
internal static class DeadTorizoCorpseDefinitions
{
    /// <summary>
    /// <c>CorpseRottingDefinitions_Torizo</c> at <c>$A9:DD58-$A9:DD67</c> and
    /// its derived wrap offset from <c>CorpseRottingTileRowOffsets_Torizo+2</c>
    /// at <c>$A9:E228</c>.
    /// </summary>
    internal static readonly DeadTorizoCorpseDefinition Corpse = new(
        ConfigurationPointer: 0xdd58,
        RottingTablePointer: 0x9000,
        VramTransferPointer: 0x0000,
        CopyFunction: 0xe38b,
        MoveFunction: 0xe272,
        EntryCount: 0x0060,
        GraphicsInitializationFunction: 0xde18,
        RotationTablePointer: 0xe226,
        FinishFunction: 0xd5bd,
        WrapOffset: 0x0134);
}

/// <summary>One immutable native dead-Torizo corpse-rotting configuration record.</summary>
internal readonly record struct DeadTorizoCorpseDefinition(
    ushort ConfigurationPointer,
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort GraphicsInitializationFunction,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
