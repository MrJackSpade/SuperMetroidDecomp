using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// The title's editable gradients, one for each of the sixteen zoom buckets, each described by its stops (#1468).
/// The gradients contain no original-program locations, instruction streams, or gameplay state.
/// </summary>
public sealed class TitleGradientPresentation
{
    /// <summary>Sixteen independently authored gradients indexed by zoom bucket.</summary>
    private readonly ScreenGradient[] variants;

    /// <summary>Retains the gradients the import validated.</summary>
    internal TitleGradientPresentation(ScreenGradient[] variants) => this.variants = variants;

    /// <summary>Returns the gradient the title shows at <paramref name="zoom"/>.</summary>
    /// <param name="zoom">The title floor's zoom, whose bucket (<see cref="TitleGradientFormat.ZoomBucket"/>) selects a gradient.</param>
    /// <returns>The selected gradient, covering the whole screen.</returns>
    public ScreenGradient Resolve(FloorZoom zoom) => variants[TitleGradientFormat.ZoomBucket.Of(zoom.TwoHundredFiftySixths)];
}
