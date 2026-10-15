namespace SuperMetroid.Core.Enemies.Corpses;

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
