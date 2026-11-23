using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The title's editable gradients, one for each zoom range (<see cref="TitleGradientFormat.VariantCount"/>), each described by its stops (#1468).
/// The gradients contain no game-data locations, animation scripts, or gameplay state.
/// </summary>
public sealed partial class TitleGradientPresentation
{
    /// <summary>The independently authored gradients in order of their zoom ranges, from no zoom up.</summary>
    private readonly ScreenGradient[] variants;

    /// <summary>The gradient the floor shows at its own size, where the final zoom ends and the idle title screen stays.</summary>
    private readonly ScreenGradient ownSize;

    /// <summary>
    /// Retains the gradients the import validated: one for each of the <see cref="TitleGradientFormat.VariantCount"/> zoom
    /// ranges, which together reach the floor's own size, and the gradient the floor shows at its own size.
    /// </summary>
    /// <param name="variants">The gradients in order of their zoom ranges, from no zoom up.</param>
    /// <param name="ownSize">The gradient the floor shows at its own size.</param>
    /// <exception cref="ArgumentException">The gradients are not one for each zoom range.</exception>
    internal TitleGradientPresentation(ScreenGradient[] variants, ScreenGradient ownSize)
    {
        this.variants = Ensure.NotNull(variants).Length == TitleGradientFormat.VariantCount
            ? variants
            : throw new ArgumentException($"The title has {TitleGradientFormat.VariantCount} gradients, one for each zoom range, not {variants.Length}.", nameof(variants));
        this.ownSize = ownSize;
    }

    /// <summary>
    /// Returns the gradient the title shows at <paramref name="titleZoom"/> (#1781): the gradient whose zoom range, each
    /// <see cref="TitleGradientFormat.ZoomRangeWidth"/> wide from no zoom up, holds the zoom, and at the floor's own size
    /// its own-size gradient.
    /// </summary>
    /// <param name="titleZoom">The title floor's zoom, no larger than the floor's own size by type.</param>
    /// <returns>The selected gradient, covering the whole screen.</returns>
    public ScreenGradient Resolve(TitleFloorZoom titleZoom)
    {
        if (titleZoom.IsOwnSize)
            return ownSize;
        FloorZoom zoom = titleZoom.FloorZoom;
        // The ranges follow one another from no zoom up, each one range width past the last, and together end at the
        // floor's own size (one for each gradient, which the constructor requires), so a zoom below it lies in exactly
        // one: past every earlier range, in the last (#2553).
        FloorZoom rangeEnd = FloorZoom.FromFloorPixelsPerScreenPixel(0);
        foreach (ScreenGradient variant in variants.AsSpan()[..^1])
        {
            rangeEnd = rangeEnd.Widened(TitleGradientFormat.ZoomRangeWidth);
            if (zoom < rangeEnd)
                return variant;
        }
        return variants[^1];
    }
}
