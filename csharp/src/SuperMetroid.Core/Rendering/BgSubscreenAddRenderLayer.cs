namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a 2-bpp or vertically scrolled 4-bpp subscreen; optional coverage excludes transparent main-plane pixels. OBJ coordinates remain screen-relative.</summary>
public sealed record BgSubscreenAddRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null, bool FourBpp = false, bool IncludeObjects = false,
    bool MainObjects = false, ushort VerticalScroll = 0) : RenderLayer
{
    private BackgroundLineScroll[] scrolls = [];
    /// <summary>Owned physical-line registers; empty means the uniform scroll fields.</summary>
    public ReadOnlySpan<BackgroundLineScroll> Scrolls => scrolls;

    public BgSubscreenAddRenderLayer WithScrolls(ReadOnlySpan<BackgroundLineScroll> value)
    {
        Ensure.LengthEqual(value, Hardware.SnesPpuLayout.ScreenHeightPixels);
        var copy = this with { };
        copy.scrolls = value.ToArray();
        return copy;
    }
}
