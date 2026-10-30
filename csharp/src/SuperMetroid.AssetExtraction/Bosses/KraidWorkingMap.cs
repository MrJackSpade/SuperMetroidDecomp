using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Kraid's body as the cartridge assembles it from his two installed body tilemaps (#1352, #1466), which his body sheet's
/// upper and lower pages picture.
/// </summary>
internal static class KraidWorkingMap
{
    /// <summary>
    /// The working map <c>$A7:AAC6</c> builds: the body's upper page over its lower page, as every body upload copies them.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed source does not hold the native number of cells.</exception>
    internal static BackgroundTile[] Compose(KraidBackgroundArtwork art)
    {
        Ensure.NotNull(art);
        ReadOnlySpan<BackgroundTile> upper = art.Upper.Tiles.Span;
        ReadOnlySpan<BackgroundTile> lower = art.Lower.Tiles.Span;
        if (upper.Length != KraidWorkingMapDefinitions.SourceTilemapCells ||
            lower.Length != KraidWorkingMapDefinitions.SourceTilemapCells)
        {
            throw new InvalidDataException(
                $"Kraid BG2 sources must each hold {KraidWorkingMapDefinitions.SourceTilemapCells} cells; " +
                $"upper={upper.Length}, lower={lower.Length}.");
        }

        var working = new BackgroundTile[KraidWorkingMapDefinitions.WorkingMapCells];
        // The lower stream is decompressed directly over the working map. `$A7:AB19` then copies only its first $600
        // bytes upward before the upper stream replaces the upper page, so the lower page's untouched tail remains
        // lower-stream data, priority bits and all. Priority is cleared only in the copied regions; Kraid's grown body
        // shows the tail above his head.
        lower[..KraidWorkingMapDefinitions.WorkingMapCells].CopyTo(working);
        for (int cell = 0; cell < KraidWorkingMapDefinitions.LowerSourceCopiedCells; cell++)
            working[KraidWorkingMapDefinitions.LowerPageFirstCell + cell] = working[cell] with { HighPriority = false };
        for (int cell = 0; cell < KraidWorkingMapDefinitions.PageCells; cell++)
            working[cell] = upper[cell] with { HighPriority = false };
        working.AsSpan(KraidWorkingMapDefinitions.BlankRowFirstCell, ScreenGeometry.TilemapScreenWidthInTiles)
            .Fill(KraidWorkingMapDefinitions.BlankTile);
        return working;
    }
}
