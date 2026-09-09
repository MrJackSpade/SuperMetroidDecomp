namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a 2-bpp or vertically scrolled 4-bpp subscreen; optional coverage excludes transparent main-plane pixels. OBJ coordinates remain screen-relative.</summary>
public sealed record BgSubscreenAddRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null, bool FourBpp = false, bool IncludeObjects = false,
    bool MainObjects = false, ushort VerticalScroll = 0) : RenderLayer;
