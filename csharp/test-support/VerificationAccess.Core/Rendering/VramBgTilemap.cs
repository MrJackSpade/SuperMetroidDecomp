using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A fixture's background tilemap in VRAM, read by scroll position with the tilemap's wrap: the tilemap the X-ray
/// reveal's copy reads in place of a room's BG1 plane.
/// </summary>
[PpuLayoutBoundary("exact hardware test: reads a fixture's BG tilemap (BGnSC) from VRAM")]
internal sealed class VramBgTilemap(SnesVram vram, int tilemapWord, Extent16<TilemapCells> mapSize) : IBgTileSource
{
    /// <summary>The tilemap's screen arrangement.</summary>
    private readonly BgTilemapGeometry geometry = new(mapSize);

    /// <inheritdoc />
    public bool TryTileAt(Position16<BgPlanePixels> scrolled, out SnesBgTilemapWord tile)
    {
        int x = geometry.WrapX(scrolled.X), y = geometry.WrapY(scrolled.Y);
        tile = vram.ReadWord(geometry.EntryWordAddress(tilemapWord,
            new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y))));
        return true;
    }
}
