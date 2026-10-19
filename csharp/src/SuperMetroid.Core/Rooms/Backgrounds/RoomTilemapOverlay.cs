using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Tilemap words a room's actor wrote over its BG1 tilemap, keyed by BG1 tilemap word: the Ceres escape warning. They
/// sit at fixed tilemap positions, over the level data, until the next room. Immutable; each write publishes a copy.
/// </summary>
public sealed class RoomTilemapOverlay
{
    /// <summary>Tilemap words in the two-screen BG1 tilemap.</summary>
    public const int WordCount = 2 * SnesPpuLayout.TilemapPageWordCount;

    /// <summary>Rows in a tilemap screen.</summary>
    private const int ScreenRows = 32;

    /// <summary>Cells per tilemap screen row.</summary>
    private const int ScreenCells = SnesPpuLayout.TilemapPageWordCount / ScreenRows;

    /// <summary>Marks a tilemap word the overlay leaves to the level data.</summary>
    public const int Unwritten = -1;

    /// <summary>The overlay with no words.</summary>
    public static RoomTilemapOverlay Empty { get; } = new([.. Enumerable.Repeat(Unwritten, WordCount)]);

    /// <summary>Takes the words, <see cref="Unwritten"/> marking positions the level data shows.</summary>
    private RoomTilemapOverlay(ImmutableArray<int> words) => Words = words;

    /// <summary>The written words by BG1 tilemap word, or <see cref="Unwritten"/> where the level data shows.</summary>
    public ImmutableArray<int> Words { get; }

    /// <summary>A copy with <paramref name="written"/> written from tilemap word <paramref name="firstWord"/> onward.</summary>
    /// <param name="firstWord">The first BG1 tilemap word written.</param>
    /// <param name="written">The tilemap words, in tilemap order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The words run past the tilemap.</exception>
    public RoomTilemapOverlay With(int firstWord, ReadOnlySpan<ushort> written)
    {
        if (firstWord < 0 || firstWord + written.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(firstWord), firstWord, $"{written.Length} words do not fit the BG1 tilemap there.");
        int[] copy = [.. Words];
        for (int index = 0; index < written.Length; index++)
            copy[firstWord + index] = written[index];
        return new([.. copy]);
    }

    /// <summary>The written tile at a scrolled position; false where the level data shows.</summary>
    /// <param name="scrolled">The BG1 scroll register plus the screen pixel.</param>
    /// <param name="tile">The written tilemap word.</param>
    public bool TryTileAt(Position16<BgPlanePixels> scrolled, out SnesBgTilemapWord tile)
    {
        int column = scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % (2 * ScreenCells);
        int row = scrolled.Y / SnesPpuLayout.BackgroundTileSizePixels % ScreenRows;
        int word = Words[column / ScreenCells * SnesPpuLayout.TilemapPageWordCount + row * ScreenCells + column % ScreenCells];
        tile = new SnesBgTilemapWord(unchecked((ushort)word));
        return word != Unwritten;
    }
}
