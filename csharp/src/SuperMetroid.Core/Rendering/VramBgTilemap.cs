using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A background tilemap in captured VRAM, sampled by scroll position with the tilemap's wrap: the HUD, the X-ray reveal,
/// and BG2 tilemaps their owners load (library backgrounds, the sky, boss bodies) until those get their own layers.
/// </summary>
[PpuLayoutBoundary("backend: software BG tilemap fetch (BGnSC) from captured VRAM")]
internal sealed class VramBgTilemap(SnesVram vram, int tilemapWord, Extent16<TilemapCells> mapSize) : IBgTileSource
{
    /// <summary>The tilemap's screen arrangement.</summary>
    private readonly BgTilemapGeometry geometry = new(mapSize);

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgTileSample sample)
    {
        int x = geometry.WrapX(scrolled.X), y = geometry.WrapY(scrolled.Y);
        SnesBgTilemapWord tile = vram.ReadWord(geometry.EntryWordAddress(tilemapWord,
            new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y))));
        sample = new BgTileSample(tile, new Position16<CharacterPixels>(
            (ushort)BgCharacterGrid.PixelWithinCharacter(x), (ushort)BgCharacterGrid.PixelWithinCharacter(y)));
        return true;
    }
}
