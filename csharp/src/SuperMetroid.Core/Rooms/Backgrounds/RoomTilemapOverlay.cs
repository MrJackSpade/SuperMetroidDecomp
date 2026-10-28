using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;
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
    public const int WordCount = 2 * ScreenGeometry.TilemapScreenCellCount;

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
}
