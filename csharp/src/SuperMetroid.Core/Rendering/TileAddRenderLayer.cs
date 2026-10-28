using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rendering;

/// <summary>Adds a frontend tile layer's subscreen (#1403); optional coverage excludes transparent main-plane pixels. OBJ coordinates remain screen-relative.</summary>
/// <param name="Cells">The subscreen's cells and the tiles they show, a 32-by-32 map.</param>
/// <param name="MainCoverage">Optional main-plane layer whose opaque samples permit addition; null permits addition without a coverage mask.</param>
/// <param name="IncludeObjects">Whether to composite screen-relative OBJ pixels onto the subscreen using native BG/OBJ priority thresholds before adding its colors.</param>
/// <param name="MainObjects">Whether main-plane OBJ visibility and palettes 4..7 affect eligibility when a coverage layer is supplied; has no effect without that layer.</param>
/// <param name="VerticalScroll">Uniform vertical scroll in pixels when per-line scrolls are absent.</param>
[IndependentFlag("independent PPU settings of the layer: which object layers it adds")]
public sealed record TileAddRenderLayer(ScreenTileMap Cells,
    TileRenderLayer? MainCoverage = null, bool IncludeObjects = false,
    bool MainObjects = false, int VerticalScroll = 0) : RenderLayer
{
    /// <summary>The subscreen's cells and the tiles they show.</summary>
    /// <exception cref="ArgumentException">The map is not one screen.</exception>
    public ScreenTileMap Cells
    {
        get;
        init => field = TileViewportRenderLayer.RequireScreen(value);
    } = TileViewportRenderLayer.RequireScreen(Cells);

    /// <summary>Uniform vertical scroll in pixels, 0 through <see cref="TileViewportRenderLayer.MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, TileViewportRenderLayer.MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, TileViewportRenderLayer.MaximumScroll, VerticalScroll);

    /// <summary>Copied per-scanline scrolls; an empty array selects the uniform scroll.</summary>
    private Position16<BgPlanePixels>[] scrolls = [];
    /// <summary>Owned physical-line scrolls; empty means the uniform scroll.</summary>
    public ReadOnlySpan<Position16<BgPlanePixels>> Scrolls => scrolls;

    /// <summary>Returns a copied layer with independently owned per-line scrolls, leaving this descriptor and the caller's input untouched; these scrolls replace uniform background scrolling, not OBJ positions.</summary>
    /// <param name="value">Exactly 224 scroll pairs in visible output-row order; background source coordinates wrap within the 256-by-256 page.</param>
    /// <returns>A record copy retaining the other layer settings and a new scroll array.</returns>
    /// <exception cref="ArgumentException">The scroll count differs from the visible screen height.</exception>
    public TileAddRenderLayer WithScrolls(ReadOnlySpan<Position16<BgPlanePixels>> value)
    {
        Ensure.LengthEqual(value, ScreenGeometry.ScreenHeightPixels);
        var copy = this with { };
        copy.scrolls = value.ToArray();
        return copy;
    }
}
