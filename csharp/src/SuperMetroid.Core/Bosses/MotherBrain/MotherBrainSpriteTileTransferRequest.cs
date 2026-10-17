namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One native seven-byte sprite-tile transfer entry consumed by `$A9:C5BE`.
/// </summary>
public readonly record struct MotherBrainSpriteTileTransferRequest(
    int EntryIndex,
    int Size,
    uint SourceAddress,
    ushort VramDestination);
