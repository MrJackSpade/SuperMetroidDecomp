namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The row schedule a corpse's rotting runs with: the exclusive pixel-row limit, the first entry that moves its row
/// rather than copying it during its last delay calls, and the byte displacement that carries pixel rows 6 and 7 into
/// the next tile row (#633).
/// </summary>
internal sealed record CorpseRottingProgram
{
    /// <summary>Exclusive pixel-row limit of the two-pixel advance, and the final entry index: the corpse height less one.</summary>
    public required int YLimit { get; init; }

    /// <summary>First entry index that moves its row, rather than copying it, during its last delay calls: the corpse height less two.</summary>
    public required int LateMoveEntryIndex { get; init; }

    /// <summary>Byte displacement added to the destination of pixel rows 6 and 7 so their two-pixel fall lands in the next tile row.</summary>
    public required int WrapOffset { get; init; }
}
