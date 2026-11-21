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

    /// <summary>Retains the gradients the import validated.</summary>
    /// <param name="variants">The gradients in order of their zoom ranges, from no zoom up.</param>
    /// <param name="ownSize">The gradient the floor shows at its own size.</param>
    internal TitleGradientPresentation(ScreenGradient[] variants, ScreenGradient ownSize)
    {
        this.variants = variants;
        this.ownSize = ownSize;
    }

    /// <summary>
    /// Returns the gradient the title shows at <paramref name="zoom"/> (#1781): the gradient whose zoom range, each
    /// <see cref="TitleGradientFormat.ZoomRangeWidth"/> wide from no zoom up, holds the zoom, and at the floor's own size
    /// its own-size gradient.
    /// </summary>
    /// <param name="zoom">The title floor's zoom, no larger than the floor's own size.</param>
    /// <returns>The selected gradient, covering the whole screen.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="zoom"/> shows the floor smaller than its own size, which the title never does: its final zoom stops
    /// at the floor's own size.
    /// </exception>
    public ScreenGradient Resolve(FloorZoom zoom)
    {
        if (zoom == FloorZoom.OwnSize)
            return ownSize;
        if (zoom > FloorZoom.OwnSize)
        {
            throw new ArgumentOutOfRangeException(nameof(zoom), zoom,
                "The title never zooms its floor out past the floor's own size.");
        }
        // The ranges follow one another from no zoom up, each one range width past the last, and together end at the
        // floor's own size, so a zoom below it lies in exactly one.
        FloorZoom rangeEnd = FloorZoom.FromFloorPixelsPerScreenPixel(0);
        foreach (ScreenGradient variant in variants)
        {
            rangeEnd = rangeEnd.Widened(TitleGradientFormat.ZoomRangeWidth);
            if (zoom < rangeEnd)
                return variant;
        }
        throw new InvalidOperationException(
            $"The title's {variants.Length} gradient zoom ranges end at {rangeEnd}, below zoom {zoom}.");
    }
}
