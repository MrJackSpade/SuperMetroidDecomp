using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The title's editable gradients, one for each of the sixteen zoom ranges, each described by its stops (#1468).
/// The gradients contain no game-data locations, animation streams, or gameplay state.
/// </summary>
public sealed class TitleGradientPresentation
{
    /// <summary>Sixteen independently authored gradients in order of their zoom ranges, from no zoom up.</summary>
    private readonly ScreenGradient[] variants;

    /// <summary>Retains the gradients the import validated.</summary>
    internal TitleGradientPresentation(ScreenGradient[] variants) => this.variants = variants;

    /// <summary>
    /// Returns the gradient the title shows at <paramref name="zoom"/> (#1781): the gradient whose zoom range, each
    /// <see cref="TitleGradientFormat.ZoomRangeWidth"/> wide from no zoom up, holds the zoom, and at the floor's own size
    /// <see cref="TitleGradientFormat.OwnSizeVariant"/>.
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
            return variants[TitleGradientFormat.OwnSizeVariant];
        if (zoom > FloorZoom.OwnSize)
        {
            throw new ArgumentOutOfRangeException(nameof(zoom), zoom,
                "The title never zooms its floor out past the floor's own size.");
        }
        return variants[zoom.TwoHundredFiftySixths / TitleGradientFormat.ZoomRangeWidth];
    }
}
