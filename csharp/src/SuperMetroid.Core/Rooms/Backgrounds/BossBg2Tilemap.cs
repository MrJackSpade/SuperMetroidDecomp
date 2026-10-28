using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A boss body's BG2 tilemap (#1352): the tiles a boss shows on BG2 in the hardware's 32x32-screen order, which the room's
/// body sheet pictures (#1392). The boss publishes it from its body frame; nothing is written into VRAM.
/// </summary>
public sealed class BossBg2Tilemap
{
    /// <summary>Creates a tilemap of <paramref name="size"/> from <paramref name="tiles"/> in screen order.</summary>
    /// <exception cref="ArgumentException">The tiles do not fill the tilemap.</exception>
    public BossBg2Tilemap(ImmutableArray<BackgroundTile> tiles, Extent16<TilemapCells> size)
    {
        Ensure.LengthEqual(tiles.AsSpan(), size.X * size.Y);
        Tiles = tiles;
        Size = size;
    }

    /// <summary>The tiles in the hardware's screen order: each 32x32 screen's rows, screens left to right then down.</summary>
    public ImmutableArray<BackgroundTile> Tiles { get; }

    /// <summary>The tilemap's size in tiles.</summary>
    public Extent16<TilemapCells> Size { get; }
}
