using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Resolves Kraid's body frame to the BG2 tilemap it shows (#1352): his 64x64 map, whose left screens hold the body's
/// upper and lower pages and whose right screens keep the blank tile the room's library background filled.
/// </summary>
internal static class KraidBodyTilemap
{
    /// <summary>The first cell of the lower-left screen, where the lower page shows.</summary>
    private const int LowerScreenCell = 2 * KraidBackgroundDefinitions.PageCells;

    /// <summary>
    /// Ports the working-map construction performed by <c>$A7:AAC6</c>: the body's upper page over its lower page, as
    /// every body upload copies them.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed source does not hold the native number of cells.</exception>
    internal static BackgroundTile[] ComposeWorkingMap(KraidBackgroundArtwork art)
    {
        Ensure.NotNull(art);
        ReadOnlySpan<BackgroundTile> upper = art.Upper.Tiles.Span;
        ReadOnlySpan<BackgroundTile> lower = art.Lower.Tiles.Span;
        if (upper.Length != KraidBackgroundDefinitions.SourceTilemapCells ||
            lower.Length != KraidBackgroundDefinitions.SourceTilemapCells)
        {
            throw new InvalidDataException(
                $"Kraid BG2 sources must each hold {KraidBackgroundDefinitions.SourceTilemapCells} cells; " +
                $"upper={upper.Length}, lower={lower.Length}.");
        }

        var working = new BackgroundTile[KraidBackgroundDefinitions.WorkingMapCells];
        // The lower stream is decompressed directly over `$7E:2000-$2FFF`, which is also
        // Kraid's working tilemap. `$A7:AB19` then copies only its first $600 bytes upward
        // to `$2800` before the upper stream replaces `$2000-$27FF`. The untouched
        // `$2E00-$2FFF` tail therefore remains lower-stream data. Priority is cleared only
        // in the copied regions, not that untouched tail. Clearing the managed buffer
        // first left those eight rows as tile zero; once growth raised BG2 and
        // enabled priority, character zero became a conspicuous repeated rectangle above
        // Kraid's head.
        lower[..KraidBackgroundDefinitions.WorkingMapCells].CopyTo(working);
        for (int cell = 0; cell < KraidBackgroundDefinitions.LowerSourceCopiedCells; cell++)
            working[KraidBackgroundDefinitions.LowerPageFirstCell + cell] = working[cell] with { HighPriority = false };
        for (int cell = 0; cell < KraidBackgroundDefinitions.PageCells; cell++)
            working[cell] = upper[cell] with { HighPriority = false };
        working.AsSpan(KraidBackgroundDefinitions.BlankRowFirstCell, ScreenGeometry.TilemapScreenWidthInTiles)
            .Fill(KraidBackgroundDefinitions.BlankTile);
        return working;
    }

    /// <summary>The tilemap <paramref name="frame"/> shows, from Kraid's composed <paramref name="working"/> map and <paramref name="art"/>.</summary>
    internal static BossBg2Tilemap Resolve(KraidBodyFrame frame, ReadOnlySpan<BackgroundTile> working, KraidBackgroundArtwork art)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(art);
        Extent16<TilemapCells> size = KraidBackgroundDefinitions.BodyPlaneSize;
        var tiles = new BackgroundTile[size.X * size.Y];
        tiles.AsSpan().Fill(KraidBackgroundDefinitions.BlankTile);
        Page(frame.Top, working[..KraidBackgroundDefinitions.PageCells], tiles.AsSpan(0, KraidBackgroundDefinitions.PageCells));
        if (frame.Head is { } head)
            art.HeadTiles(head).CopyTo(tiles);
        Page(frame.Bottom, working.Slice(KraidBackgroundDefinitions.LowerPageFirstCell, KraidBackgroundDefinitions.PageCells),
            tiles.AsSpan(LowerScreenCell, KraidBackgroundDefinitions.PageCells));
        return new BossBg2Tilemap(ImmutableArray.Create(tiles), size);
    }

    /// <summary>Writes what <paramref name="page"/> shows of <paramref name="body"/> into <paramref name="destination"/>.</summary>
    private static void Page(KraidBodyPage page, ReadOnlySpan<BackgroundTile> body, Span<BackgroundTile> destination)
    {
        switch (page)
        {
            case KraidBodyPage.Blank:
                destination.Fill(KraidBackgroundDefinitions.BlankTile);
                break;
            case KraidBodyPage.Body:
                body.CopyTo(destination);
                break;
            case KraidBodyPage.PriorityBody:
                for (int cell = 0; cell < destination.Length; cell++)
                    destination[cell] = body[cell] with { HighPriority = true };
                break;
            default:
                throw new InvalidDataException($"Undefined Kraid body page {page}.");
        }
    }
}
