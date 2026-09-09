namespace SuperMetroid.Core.Rendering;

/// <summary>Adds an unscrolled 2-bpp or 4-bpp subscreen; optional coverage excludes transparent main-plane pixels.</summary>
public sealed record BgSubscreenAddRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null, bool FourBpp = false, bool IncludeObjects = false,
    bool MainObjects = false) : RenderLayer;
