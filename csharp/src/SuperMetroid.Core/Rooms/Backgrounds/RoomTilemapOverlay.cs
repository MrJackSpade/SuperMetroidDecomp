using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Tiles a room's actor wrote over a background, keyed by tilemap position: the Ceres escape warning over BG1, and the
/// escape typewriter's text over BG1 (Ceres) or BG2 (Zebes). They sit at fixed tilemap positions, over what the
/// background otherwise shows, until the next room. Immutable; each write publishes a copy.
/// </summary>
public sealed class RoomTilemapOverlay
{
    /// <summary>Tilemap positions in a two-screen tilemap.</summary>
    public const int WordCount = 2 * ScreenGeometry.TilemapScreenCellCount;

    /// <summary>The overlay with no tiles.</summary>
    public static RoomTilemapOverlay Empty { get; } = new([.. Enumerable.Repeat((BackgroundTile?)null, WordCount)]);

    /// <summary>Takes the tiles, null marking positions the level data shows.</summary>
    private RoomTilemapOverlay(ImmutableArray<BackgroundTile?> tiles) => Tiles = tiles;

    /// <summary>The written tiles by tilemap position, or null where the background shows.</summary>
    public ImmutableArray<BackgroundTile?> Tiles { get; }

    /// <summary>A copy with <paramref name="written"/> written from tilemap position <paramref name="first"/> onward.</summary>
    /// <param name="first">The first tilemap position written.</param>
    /// <param name="written">The tiles, in tilemap order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tiles run past the tilemap.</exception>
    public RoomTilemapOverlay With(int first, ReadOnlySpan<BackgroundTile> written)
    {
        if (first < 0 || first + written.Length > WordCount)
            throw new ArgumentOutOfRangeException(nameof(first), first, $"{written.Length} tiles do not fit a two-screen tilemap there.");
        BackgroundTile?[] copy = [.. Tiles];
        for (int index = 0; index < written.Length; index++)
            copy[first + index] = written[index];
        return new([.. copy]);
    }
}
