namespace SuperMetroid.Core.Rendering;

/// <summary>A scrolled 4-bpp plane using native tilemap and character word addresses.</summary>
/// <param name="TilemapWord">Tilemap base in VRAM words.</param>
/// <param name="CharacterWord">4-bpp character base in VRAM words.</param>
/// <param name="Scroll">The BGnHOFS/BGnVOFS register pair, before tilemap wrapping.</param>
/// <param name="MapSize">Tilemap size in eight-pixel cells, 32 or 64 on each axis.</param>
/// <param name="Priority">Selects only tiles of one priority, or null for both.</param>
public sealed record Bg4BppRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Position16<BgPlanePixels> Scroll, Extent16<TilemapCells> MapSize, bool? Priority) : RenderLayer;
