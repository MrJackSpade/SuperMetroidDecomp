namespace SuperMetroid.Core.Escape;

/// <summary>
/// One native seven-byte sprite-tile transfer entry consumed by `$A9:C5BE`.
/// </summary>
/// <param name="EntryIndex">The entry's zero-based position in its transfer list.</param>
/// <param name="Size">Bytes copied by the transfer.</param>
/// <param name="SourceAddress">Where the copied tiles come from, which selects the installed artwork page.</param>
/// <param name="VramDestination">The first OBJ VRAM word the tiles land on, 0 through 32,767; converted to the transfer word only at the VRAM boundary.</param>
public readonly record struct MotherBrainSpriteTileTransferRequest(
    int EntryIndex,
    int Size,
    uint SourceAddress,
    int VramDestination);
