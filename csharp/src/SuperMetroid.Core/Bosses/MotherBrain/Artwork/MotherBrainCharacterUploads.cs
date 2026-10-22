using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The uploads Mother Brain's encounter makes over the OBJ characters after its room loads (#1392), as its sprite tile
/// transfers record them: each page of its special sprite sheets (legs, the Baby Metroid, its attacks, the exploded door),
/// each page of its corpse, and the escape text and timer pages the escape begins with.
/// </summary>
internal static class MotherBrainCharacterUploads
{
    /// <summary>Every upload, in the order the catalogue names them.</summary>
    internal static IReadOnlyList<RuntimeCharacterUpload> All { get; } =
    [
        .. MotherBrainSpecialSpriteArtworkDefinitions.All.SelectMany((sheet, index) => Enumerable.Range(0, sheet.PageCount)
            .Select(page => Upload(sheet.Transfer(page), new MotherBrainSpecialSpriteCharacters(index, page, MotherBrainSpecialSpriteSheetDefinition.PageByteCount)))),
        .. Enumerable.Range(0, MotherBrainCorpseArtworkDefinitions.RowCount).Select(page => Upload(
            new MotherBrainSpriteTileTransferRequest((ushort)page, MotherBrainCorpseArtworkDefinitions.VramPageByteCount,
                MotherBrainCorpseArtworkDefinitions.VramPageSource(page), MotherBrainCorpseArtworkDefinitions.VramPageDestination(page)),
            new MotherBrainCorpseCharacters(page))),
        .. Enumerable.Range(0, EscapeTextArtworkDefinitions.TransferCount).Select(EscapeUpload),
    ];

    /// <summary>The escape's transfer <paramref name="index"/>: a timer page as its installed asset, else a text page.</summary>
    private static RuntimeCharacterUpload EscapeUpload(int index)
    {
        MotherBrainSpriteTileTransferRequest transfer = EscapeTextArtworkDefinitions.Transfer(index);
        if (EscapeTimerTilePages.TryMatch(unchecked((int)transfer.SourceAddress), transfer.Size, checked((ushort)transfer.VramDestination), out EscapeTimerTilePage timer))
            return Upload(transfer, new AssetObjCharacters(EscapeTimerTilePages.AssetOf(timer)));
        return Upload(transfer, new MotherBrainEscapeTextCharacters(index - EscapeTextArtworkDefinitions.TimerPageCount));
    }

    /// <summary>The upload <paramref name="transfer"/> makes of <paramref name="source"/>.</summary>
    [PpuLayoutBoundary("codec: Mother Brain sprite tile transfer requests' native VRAM destinations")]
    private static RuntimeCharacterUpload Upload(MotherBrainSpriteTileTransferRequest transfer, ObjCharacterSource source) =>
        RuntimeCharacterUpload.To(VramDmaDestination.FromNative(checked((ushort)transfer.VramDestination)), transfer.Size, source);
}
