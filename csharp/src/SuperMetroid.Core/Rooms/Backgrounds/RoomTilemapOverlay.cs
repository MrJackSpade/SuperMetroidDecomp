using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Tilemap words a room's actor wrote over a background, keyed by tilemap word: the Ceres escape warning over BG1, and
/// the escape typewriter's text over BG1 (Ceres) or BG2 (Zebes). They sit at fixed tilemap positions, over what the
/// background otherwise shows, until the next room. Immutable; each write publishes a copy.
/// </summary>
public sealed class RoomTilemapOverlay
{
    /// <summary>Tilemap words in a two-screen tilemap.</summary>
    public const int WordCount = 2 * SnesPpuLayout.TilemapPageWordCount;

    /// <summary>Rows, and cells per row, in a tilemap screen.</summary>
    private const int ScreenCells = SnesPpuLayout.TilemapPageWidthInTiles;

    /// <summary>Marks a tilemap word the overlay leaves to the level data.</summary>
    public const int Unwritten = -1;

    /// <summary>The overlay with no words.</summary>
    public static RoomTilemapOverlay Empty { get; } = new([.. Enumerable.Repeat(Unwritten, WordCount)]);

    /// <summary>Takes the words, <see cref="Unwritten"/> marking positions the level data shows.</summary>
    private RoomTilemapOverlay(ImmutableArray<int> words) => Words = words;

    /// <summary>The written words by tilemap word, or <see cref="Unwritten"/> where the background shows.</summary>
    public ImmutableArray<int> Words { get; }

    /// <summary>A copy with <paramref name="written"/> written from tilemap word <paramref name="firstWord"/> onward.</summary>
    /// <param name="firstWord">The first tilemap word written.</param>
    /// <param name="written">The tilemap words, in tilemap order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The words run past the tilemap.</exception>
    public RoomTilemapOverlay With(int firstWord, ReadOnlySpan<ushort> written)
    {
        if (firstWord < 0 || firstWord + written.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(firstWord), firstWord, $"{written.Length} words do not fit a two-screen tilemap there.");
        int[] copy = [.. Words];
        for (int index = 0; index < written.Length; index++)
            copy[firstWord + index] = written[index];
        return new([.. copy]);
    }

    /// <summary>The written tile at a scrolled position of a tilemap laid out as <paramref name="mapSize"/>; false where the background shows.</summary>
    /// <param name="scrolled">The scroll register plus the screen pixel.</param>
    /// <param name="mapSize">The background's tilemap size: screens of 32 by 32 cells, left to right then top to bottom.</param>
    /// <param name="tile">The written tilemap word.</param>
    public bool TryTileAt(Position16<BgPlanePixels> scrolled, Extent16<TilemapCells> mapSize, out SnesBgTilemapWord tile)
    {
        int column = scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % mapSize.X;
        int row = scrolled.Y / SnesPpuLayout.BackgroundTileSizePixels % mapSize.Y;
        int screen = row / ScreenCells * (mapSize.X / ScreenCells) + column / ScreenCells;
        int index = screen * SnesPpuLayout.TilemapPageWordCount + row % ScreenCells * ScreenCells + column % ScreenCells;
        int word = index < WordCount ? Words[index] : Unwritten;
        tile = new SnesBgTilemapWord(unchecked((ushort)word));
        return word != Unwritten;
    }
}
