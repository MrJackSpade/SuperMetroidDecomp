namespace SuperMetroid.Core.Rendering;

/// <summary>A 256-pixel-wide window into a wrapping 32-row 2-bpp tilemap.</summary>
public sealed record Bg2BppViewportRenderLayer(ushort TilemapWord, ushort CharacterWord,
    ushort VerticalScroll, bool TransparentColorZero, bool? Priority) : RenderLayer;
