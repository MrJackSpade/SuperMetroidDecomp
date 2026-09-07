namespace SuperMetroid.Core.Rendering;

/// <summary>Adds an unscrolled 2-bpp subscreen; optional 4-bpp coverage excludes transparent main-plane pixels.</summary>
public sealed record BgSubscreenAddRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null) : RenderLayer;
