namespace SuperMetroid.Core.Rendering;

/// <summary>A scrolled 4-bpp plane using tilemap and character base VRAM word addresses.</summary>
/// <param name="TilemapWord">Tilemap base VRAM word address.</param>
/// <param name="CharacterWord">4-bpp character base VRAM word address.</param>
/// <param name="Scroll">The BGnHOFS/BGnVOFS register pair, before tilemap wrapping.</param>
/// <param name="MapSize">Tilemap size in eight-pixel cells, 32 or 64 on each axis.</param>
/// <param name="Priority">Selects only tiles of one priority, or null for both.</param>
/// <param name="Pictures">The pre-coloured tiles its characters show, which it draws from (#1392); null while it draws from VRAM.</param>
public sealed record Bg4BppRenderLayer(int TilemapWord, int CharacterWord,
    Position16<BgPlanePixels> Scroll, Extent16<TilemapCells> MapSize, bool? Priority, ScreenLayerPictures? Pictures = null) : RenderLayer
{
    /// <summary>Tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int TilemapWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(TilemapWord);

    /// <summary>4-bpp character base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int CharacterWord
    {
        get;
        init => field = VramWordRange.Require(value);
    } = VramWordRange.Require(CharacterWord);
}
