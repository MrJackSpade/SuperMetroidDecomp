using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Reads an overlay's written pictures as the tile words the cartridge writes for them (#1466), so verification can compare
/// them with the words the cartridge writes: each Ceres warning page as its installed words, each typed glyph as the tile
/// its typewriter types. The overlay itself names only its pictures; the words are the import's knowledge.
/// </summary>
internal static class RoomWrittenOverlayAccess
{
    /// <summary>The value a position the overlay leaves to the level data reads as.</summary>
    private const int UnwrittenWord = -1;

    /// <summary>The packed words of an overlay's pictures.</summary>
    extension(RoomWrittenOverlay overlay)
    {
        /// <summary>
        /// Each position's packed tilemap word, or -1 where nothing was written, the Ceres warning pages' words read from
        /// <paramref name="pages"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">A picture is one no writer writes, or a page is written with no pages given.</exception>
        internal ImmutableArray<int> WordsIn(CeresEscapeOverlays? pages)
        {
            int[] words = [.. Enumerable.Repeat(UnwrittenWord, RoomWrittenOverlay.CellCount)];
            foreach (WrittenPicture written in overlay.Pictures)
            {
                BackgroundTile[] tiles = TilesOf(written.Picture, pages);
                for (int index = 0; index < tiles.Length; index++)
                    words[written.Cell + index] = tiles[index].PackedWord;
            }
            return [.. words];
        }
    }

    /// <summary>The marker for a position an overlay leaves to the level data.</summary>
    extension(RoomWrittenOverlay)
    {
        /// <summary>Marks a tilemap word the overlay leaves to the level data.</summary>
        internal static int Unwritten => UnwrittenWord;
    }

    /// <summary>The tiles, along one row, the cartridge writes for <paramref name="picture"/>.</summary>
    private static BackgroundTile[] TilesOf(string picture, CeresEscapeOverlays? pages)
    {
        foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (page.Picture != picture)
                continue;
            if (pages is null || !pages.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles))
                throw new InvalidOperationException($"No installed Ceres warning page gives the words of {picture}.");
            return tiles.ToArray();
        }
        foreach ((EscapeTypewriterProgramId program, BackgroundTile tileBase) in new[]
                 {
                     (EscapeTypewriterProgramId.Ceres, EscapeTypewriterDefinitions.CeresTileBase),
                     (EscapeTypewriterProgramId.Zebes, EscapeTypewriterDefinitions.ZebesTileBase),
                 })
        {
            for (int character = 0; character <= byte.MaxValue; character++)
            {
                if (EscapeTypewriterGlyph.PictureOf(program, (char)character) == picture
                    && EscapeTypewriterState.GlyphOf(character, tileBase) is { } glyph)
                    return [glyph];
            }
        }
        throw new InvalidOperationException($"No writer writes a picture named {picture}.");
    }
}
