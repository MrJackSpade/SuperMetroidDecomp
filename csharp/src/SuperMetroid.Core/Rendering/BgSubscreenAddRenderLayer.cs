namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a 2-bpp or vertically scrolled 4-bpp subscreen; optional coverage excludes transparent main-plane pixels. OBJ coordinates remain screen-relative.</summary>
/// <param name="TilemapWord">Subscreen BG tilemap base VRAM word address, using a 32-by-32 character-cell page.</param>
/// <param name="CharacterWord">Subscreen character-data base VRAM word address, interpreted at the selected bit depth.</param>
/// <param name="MainCoverage">Optional main-plane BG descriptor whose opaque samples permit addition; null permits addition without a BG coverage mask.</param>
/// <param name="FourBpp">True to sample four-bit BG characters, false for two-bit characters; pen zero remains transparent.</param>
/// <param name="IncludeObjects">Whether to composite screen-relative OBJ pixels onto the subscreen using native BG/OBJ priority thresholds before adding its colors.</param>
/// <param name="MainObjects">Whether main-plane OBJ visibility and palettes 4..7 affect eligibility when a coverage descriptor is supplied; has no effect without that descriptor.</param>
/// <param name="VerticalScroll">Uniform vertical scroll in pixels when per-line registers are absent; nonzero values require four-bit sampling.</param>
public sealed record BgSubscreenAddRenderLayer(int TilemapWord, int CharacterWord,
    Bg4BppRenderLayer? MainCoverage = null, bool FourBpp = false, bool IncludeObjects = false,
    bool MainObjects = false, int VerticalScroll = 0) : RenderLayer
{
    /// <summary>Subscreen BG tilemap base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int TilemapWord
    {
        get => tilemapWord;
        init => tilemapWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="TilemapWord"/>.</summary>
    private readonly int tilemapWord = VramWordRange.Require(TilemapWord);

    /// <summary>Subscreen character-data base VRAM word address, 0 through <c>VramWordRange.MaximumWord</c>.</summary>
    public int CharacterWord
    {
        get => characterWord;
        init => characterWord = VramWordRange.Require(value);
    }

    /// <summary>Validated store of <see cref="CharacterWord"/>.</summary>
    private readonly int characterWord = VramWordRange.Require(CharacterWord);

    /// <summary>Uniform vertical scroll in pixels, 0 through <see cref="Bg2BppViewportRenderLayer.MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get => verticalScroll;
        init => verticalScroll = Ensure.BetweenInclusive(0, Bg2BppViewportRenderLayer.MaximumScroll, value);
    }

    /// <summary>Validated store of <see cref="VerticalScroll"/>.</summary>
    private readonly int verticalScroll = Ensure.BetweenInclusive(0, Bg2BppViewportRenderLayer.MaximumScroll, VerticalScroll);

    /// <summary>Copied per-scanline BG scroll registers; an empty array selects the uniform scroll properties.</summary>
    private Position16<BgPlanePixels>[] scrolls = [];
    /// <summary>Owned physical-line registers; empty means the uniform scroll fields.</summary>
    public ReadOnlySpan<Position16<BgPlanePixels>> Scrolls => scrolls;

    /// <summary>Returns a copied layer with independently owned per-line scroll registers, leaving this descriptor and the caller's input untouched; these registers replace uniform background scrolling, not OBJ positions.</summary>
    /// <param name="value">Exactly 224 scroll-register pairs in visible output-row order; background source coordinates wrap within the 256-by-256 page.</param>
    /// <returns>A record copy retaining the other layer settings and a new scroll array; its registers apply to either bit depth.</returns>
    /// <exception cref="ArgumentException">The register count differs from the visible screen height.</exception>
    public BgSubscreenAddRenderLayer WithScrolls(ReadOnlySpan<Position16<BgPlanePixels>> value)
    {
        Ensure.LengthEqual(value, Hardware.SnesPpuLayout.ScreenHeightPixels);
        var copy = this with { };
        copy.scrolls = value.ToArray();
        return copy;
    }
}
