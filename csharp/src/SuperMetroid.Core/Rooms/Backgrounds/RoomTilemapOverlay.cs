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

    /// <summary>Takes the words, <see cref="Unwritten"/> marking positions the level data shows, and their pictures.</summary>
    private RoomTilemapOverlay(ImmutableArray<int> words, ImmutableArray<BgTilePicture?> pictures = default)
    {
        Words = words;
        Pictures = pictures;
    }

    /// <summary>The written words by tilemap word, or <see cref="Unwritten"/> where the background shows.</summary>
    public ImmutableArray<int> Words { get; }

    /// <summary>
    /// Each written word's picture in the room's tile word sheet, null where none is written or a word draws from its
    /// character; default before the words' pictures are resolved (#1392).
    /// </summary>
    public ImmutableArray<BgTilePicture?> Pictures { get; }

    /// <summary>The overlay drawn from <paramref name="pictures"/>, one for each tilemap word.</summary>
    /// <exception cref="ArgumentException">There is not one picture for each tilemap word.</exception>
    public RoomTilemapOverlay WithPictures(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length != WordCount)
            throw new ArgumentException($"An overlay needs a picture slot for each of its {WordCount} words.", nameof(pictures));
        return new RoomTilemapOverlay(Words, pictures);
    }

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
        int index = IndexAt(scrolled, mapSize);
        int word = index < WordCount ? Words[index] : Unwritten;
        tile = new SnesBgTilemapWord(unchecked((ushort)word));
        return word != Unwritten;
    }

    /// <summary>
    /// What the overlay shows at a scrolled position of a tilemap laid out as <paramref name="mapSize"/>: its word's picture
    /// once resolved, otherwise its word's character; false where the background shows.
    /// </summary>
    public bool TrySampleAt(Position16<BgPlanePixels> scrolled, Extent16<TilemapCells> mapSize, out BgSample sample)
    {
        if (!TryTileAt(scrolled, mapSize, out SnesBgTilemapWord tile))
        {
            sample = default;
            return false;
        }
        if (!Pictures.IsDefault && Pictures[IndexAt(scrolled, mapSize)] is { } picture)
        {
            sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>(
                (ushort)(picture.Corner.X + scrolled.X % SnesPpuLayout.BackgroundTileSizePixels),
                (ushort)(picture.Corner.Y + scrolled.Y % SnesPpuLayout.BackgroundTileSizePixels)));
            return true;
        }
        sample = BgSample.OfTile(BgTileSample.At(tile, scrolled));
        return true;
    }

    /// <summary>The tilemap word at a scrolled position of a tilemap laid out as <paramref name="mapSize"/>, past the overlay beyond two screens.</summary>
    private static int IndexAt(Position16<BgPlanePixels> scrolled, Extent16<TilemapCells> mapSize)
    {
        int column = scrolled.X / SnesPpuLayout.BackgroundTileSizePixels % mapSize.X;
        int row = scrolled.Y / SnesPpuLayout.BackgroundTileSizePixels % mapSize.Y;
        int screen = row / ScreenCells * (mapSize.X / ScreenCells) + column / ScreenCells;
        return screen * SnesPpuLayout.TilemapPageWordCount + row % ScreenCells * ScreenCells + column % ScreenCells;
    }
}
