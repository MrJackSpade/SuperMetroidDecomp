using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Resolves Kraid's body frame to the BG2 tilemap it shows (#1352): his 64x64 map, whose left screens hold the body's
/// upper and lower pages and whose right screens keep the blank tile the room's library background filled.
/// </summary>
[PpuLayoutBoundary("codec: Kraid's 64x64 BG2 map in the hardware's screen order")]
internal static class KraidBodyTilemap
{
    /// <summary>The first word of the lower-left screen, where the lower page shows.</summary>
    private const int LowerScreenWord = 2 * KraidBackgroundDefinitions.PageCells;

    /// <summary>
    /// Ports the working-map construction performed by <c>$A7:AAC6</c>: the body's upper page over its lower page, as
    /// every body upload copies them.
    /// </summary>
    /// <exception cref="InvalidDataException">An installed source does not decompress to the native size.</exception>
    internal static ushort[] ComposeWorkingMap(KraidBackgroundArtwork art)
    {
        Ensure.NotNull(art);
        ReadOnlySpan<byte> upper = art.Upper.Bytes.Span;
        ReadOnlySpan<byte> lower = art.Lower.Bytes.Span;
        if (upper.Length != KraidBackgroundDefinitions.SourceTilemapBytes ||
            lower.Length != KraidBackgroundDefinitions.SourceTilemapBytes)
        {
            throw new InvalidDataException(
                $"Kraid BG2 sources must each decompress to " +
                $"${KraidBackgroundDefinitions.SourceTilemapBytes:X} bytes; " +
                $"upper=${upper.Length:X}, lower=${lower.Length:X}.");
        }

        var working = new ushort[KraidBackgroundDefinitions.WorkingMapCells];
        // The lower stream is decompressed directly over `$7E:2000-$2FFF`, which is also
        // Kraid's working tilemap. `$A7:AB19` then copies only its first $600 bytes upward
        // to `$2800` before the upper stream replaces `$2000-$27FF`. The untouched
        // `$2E00-$2FFF` tail therefore remains lower-stream data. Priority is cleared only
        // in the copied regions, not that untouched tail. Clearing the managed buffer
        // first left those eight rows as tile word zero; once growth raised BG2 and
        // enabled priority, character zero became a conspicuous repeated rectangle above
        // Kraid's head.
        for (int word = 0; word < KraidBackgroundDefinitions.WorkingMapCells; word++)
            working[word] = SnesLittleEndian.Word(lower[word * 2], lower[word * 2 + 1]);
        for (int word = 0; word < KraidBackgroundDefinitions.LowerSourceCopiedCells; word++)
            working[KraidBackgroundDefinitions.LowerPageFirstCell + word] = WithoutPriority(working[word]);
        for (int word = 0; word < KraidBackgroundDefinitions.PageCells; word++)
            working[word] = WithoutPriority(SnesLittleEndian.Word(upper[word * 2], upper[word * 2 + 1]));
        working.AsSpan(KraidBackgroundDefinitions.BlankRowFirstCell, SnesPpuLayout.TilemapPageWidthInTiles)
            .Fill(KraidBackgroundDefinitions.BlankTile);
        return working;
    }

    /// <summary>The tilemap <paramref name="frame"/> shows, from Kraid's composed <paramref name="working"/> map and <paramref name="art"/>.</summary>
    internal static BossBg2Tilemap Resolve(KraidBodyFrame frame, ReadOnlySpan<ushort> working, KraidBackgroundArtwork art)
    {
        Ensure.NotNull(frame);
        Ensure.NotNull(art);
        Extent16<TilemapCells> size = KraidBackgroundDefinitions.BodyPlaneSize;
        var words = new ushort[size.X * size.Y];
        words.AsSpan().Fill(KraidBackgroundDefinitions.BlankTile);
        Page(frame.Top, working[..KraidBackgroundDefinitions.PageCells], words.AsSpan(0, KraidBackgroundDefinitions.PageCells));
        if (frame.Head is { } head)
            art.HeadWords(head).CopyTo(words);
        Page(frame.Bottom, working.Slice(KraidBackgroundDefinitions.LowerPageFirstCell, KraidBackgroundDefinitions.PageCells),
            words.AsSpan(LowerScreenWord, KraidBackgroundDefinitions.PageCells));
        return new BossBg2Tilemap(ImmutableArray.Create(words), size);
    }

    /// <summary>Writes what <paramref name="page"/> shows of <paramref name="body"/> into <paramref name="destination"/>.</summary>
    private static void Page(KraidBodyPage page, ReadOnlySpan<ushort> body, Span<ushort> destination)
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
                for (int word = 0; word < destination.Length; word++)
                    destination[word] = (ushort)(body[word] | SnesBgTilemapWord.PriorityMask);
                break;
            default:
                throw new InvalidDataException($"Undefined Kraid body page {page}.");
        }
    }

    /// <summary>Clears the BG2 priority bit from a Kraid background tile word.</summary>
    private static ushort WithoutPriority(int word) => (ushort)(word & ~SnesBgTilemapWord.PriorityMask);
}
