using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend background layer's VRAM tilemap drawn from its characters' pre-coloured tiles (#1392): every position shows
/// its word's tile, transparent where the tile is.
/// </summary>
/// <param name="vram">The VRAM image holding the tilemap.</param>
/// <param name="tilemapWord">The tilemap's first VRAM word.</param>
/// <param name="mapSize">The tilemap's size in cells.</param>
/// <param name="pictures">What each character the words name shows.</param>
[PpuLayoutBoundary("backend: a frontend layer's tilemap words, read from VRAM")]
internal sealed class ScreenTilemapSource(SnesVram vram, int tilemapWord, Extent16<TilemapCells> mapSize, ScreenLayerPictures pictures) : IBgSource
{
    /// <summary>The VRAM image holding the tilemap.</summary>
    internal SnesVram Vram { get; } = Ensure.NotNull(vram);

    /// <summary>The depth of the layer's characters.</summary>
    internal BgBitDepth Depth => pictures.Depth;

    /// <summary>The tilemap's layout.</summary>
    private readonly BgTilemapGeometry geometry = new(mapSize);

    /// <inheritdoc />
    public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
    {
        int x = geometry.WrapX(scrolled.X), y = geometry.WrapY(scrolled.Y);
        SnesBgTilemapWord word = Vram.ReadWord(geometry.EntryWordAddress(tilemapWord,
            new Position16<TilemapCells>((ushort)BgCharacterGrid.CharacterOf(x), (ushort)BgCharacterGrid.CharacterOf(y))));
        sample = pictures.Sample(word, new Position16<CharacterPixels>(
            (ushort)BgCharacterGrid.PixelWithinCharacter(x), (ushort)BgCharacterGrid.PixelWithinCharacter(y)));
        return true;
    }
}
