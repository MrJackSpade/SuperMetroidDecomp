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
        RottingTablePointer: 0x9000,
        VramTransferPointer: 0x0000,
        CopyFunction: 0xe38b,
        MoveFunction: 0xe272,
        EntryCount: 0x0060,
        RotationTablePointer: 0xe226,
        FinishFunction: 0xd5bd,
        WrapOffset: 0x0134);
}

/// <summary>One immutable native dead-Torizo corpse-rotting configuration record.</summary>
/// <param name="RottingTablePointer">WRAM-relative base of the per-row source-offset table initialized for the corpse.</param>
/// <param name="VramTransferPointer">Native transfer-list pointer used during rotting; zero for this Torizo variant.</param>
/// <param name="CopyFunction">Bank-$A9 routine that copies the selected corpse row into its destination.</param>
/// <param name="MoveFunction">Bank-$A9 routine that advances the corpse's vertical rotting position.</param>
/// <param name="EntryCount">Number of row-offset entries used to set the corpse's terminal scanline limit.</param>
/// <param name="RotationTablePointer">Bank-$A9 row-offset table used to select the rotated source position.</param>
/// <param name="FinishFunction">Bank-$A9 routine that completes the corpse-rotting sequence.</param>
/// <param name="WrapOffset">Additional source offset applied when row addressing wraps past the table's data.</param>
internal readonly record struct DeadTorizoCorpseDefinition(
    ushort RottingTablePointer,
    ushort VramTransferPointer,
    ushort CopyFunction,
    ushort MoveFunction,
    ushort EntryCount,
    ushort RotationTablePointer,
    ushort FinishFunction,
    ushort WrapOffset);
