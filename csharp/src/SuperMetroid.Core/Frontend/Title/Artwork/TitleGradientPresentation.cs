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

    /// <summary>The 256ths of a floor pixel per screen pixel of zoom each gradient covers: a sixteenth of a floor pixel.</summary>
    private const int ZoomPerVariant = 16;

    /// <summary>
    /// Returns the gradient the title shows at <paramref name="zoom"/>: the sixteenth of a floor pixel per screen pixel
    /// the zoom's fraction lies in chooses it, so the choice repeats with every whole floor pixel.
    /// </summary>
    /// <param name="zoom">The title floor's zoom.</param>
    /// <returns>The selected gradient, covering the whole screen.</returns>
    public ScreenGradient Resolve(FloorZoom zoom) =>
        variants[zoom.TwoHundredFiftySixths / ZoomPerVariant % TitleGradientFormat.VariantCount];
}
