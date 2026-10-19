using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy BG2 image as an accepted NMI published it (#1353): its words in the hardware's screen order, shown on BG2
/// through the map size BG2's registers select, as the uploaded image was.
/// </summary>
/// <param name="words">The image's words, row by row, 32 per row: screen order for any 32-wide or two-screen map.</param>
public sealed class EnemyBg2ImageSnapshot(ImmutableArray<ushort> words)
{
    /// <summary>The image's words, row by row, 32 per row.</summary>
    public ImmutableArray<ushort> Words { get; } = words;

    /// <summary>The last tilemap shown, by map size.</summary>
    private BossBg2Tilemap? shown;

    /// <summary>The image as a BG2 tilemap of <paramref name="mapSize"/>.</summary>
    /// <exception cref="InvalidOperationException">The map reaches past the image.</exception>
    public BossBg2Tilemap Tilemap(Extent16<TilemapCells> mapSize)
    {
        if (shown is { } cached && cached.Size == mapSize)
            return cached;
        int cells = mapSize.X * mapSize.Y;
        if (cells > Words.Length)
            throw new InvalidOperationException($"A {mapSize.X}x{mapSize.Y} BG2 map reaches past the {Words.Length}-word enemy BG2 image.");
        return shown = new BossBg2Tilemap(Words[..cells], mapSize);
    }
}
