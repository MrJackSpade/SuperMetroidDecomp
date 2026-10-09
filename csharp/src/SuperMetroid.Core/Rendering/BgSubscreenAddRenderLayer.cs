namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a 2-bpp or vertically scrolled 4-bpp subscreen; optional coverage excludes transparent main-plane pixels. OBJ coordinates remain screen-relative.</summary>
/// <param name="TilemapWord">Subscreen BG tilemap base in VRAM words, using a 32-by-32 character-cell page.</param>
/// <param name="CharacterWord">Subscreen character-data base in VRAM words, interpreted at the selected bit depth.</param>
/// <param name="MainCoverage">Optional main-plane BG descriptor whose opaque samples permit addition; null permits addition without a BG coverage mask.</param>
/// <param name="FourBpp">True to sample four-bit BG characters, false for two-bit characters; pen zero remains transparent.</param>
/// <param name="IncludeObjects">Whether to composite screen-relative OBJ pixels onto the subscreen using native BG/OBJ priority thresholds before adding its colors.</param>
/// <param name="MainObjects">Whether main-plane OBJ visibility and palettes 4..7 affect eligibility when a coverage descriptor is supplied; has no effect without that descriptor.</param>
/// <param name="VerticalScroll">Uniform vertical scroll in pixels when per-line registers are absent; nonzero values require four-bit sampling.</param>
public sealed record BgSubscreenAddRenderLayer(ushort TilemapWord, ushort CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null, bool FourBpp = false, bool IncludeObjects = false,
    bool MainObjects = false, ushort VerticalScroll = 0) : RenderLayer
{
    /// <summary>Optional owned per-scanline background scroll table; empty uses the uniform scroll fields.</summary>
    private BackgroundLineScroll[] scrolls = [];
    /// <summary>Owned physical-line registers; empty means the uniform scroll fields.</summary>
    public ReadOnlySpan<BackgroundLineScroll> Scrolls => scrolls;

    /// <summary>Returns a copied layer with independently owned per-line scroll registers, leaving this descriptor and the caller's input untouched; these registers replace uniform background scrolling, not OBJ positions.</summary>
    /// <param name="value">Exactly 224 X/Y pixel-scroll pairs in visible output-row order; background source coordinates wrap within the 256-by-256 page.</param>
    /// <returns>A record copy retaining the other layer settings and a new scroll array; its registers apply to either bit depth.</returns>
    /// <exception cref="ArgumentException">The register count differs from the visible screen height.</exception>
    public BgSubscreenAddRenderLayer WithScrolls(ReadOnlySpan<BackgroundLineScroll> value)
    {
        Ensure.LengthEqual(value, Hardware.SnesPpuLayout.ScreenHeightPixels);
        var copy = this with { };
        copy.scrolls = value.ToArray();
        return copy;
    }
}
