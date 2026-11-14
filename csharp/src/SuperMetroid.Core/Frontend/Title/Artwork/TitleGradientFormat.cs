namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>The title's gradients: how many there are and the zoom range each covers.</summary>
public static class TitleGradientFormat
{
    /// <summary>Sixteen gradients, one for each zoom range, in order of zoom from no zoom up.</summary>
    public const int VariantCount = 16;

    /// <summary>
    /// How wide a zoom range each gradient covers, in 256ths of a floor pixel per screen pixel (#1781): the sixteen ranges
    /// together cover every zoom from no zoom up to just below the floor's own size.
    /// </summary>
    public const int ZoomRangeWidth = 16;

    /// <summary>
    /// The gradient the floor shows at its own size, where the final zoom ends and the idle title screen stays: the first,
    /// the gradient of the zoom range that starts at no zoom.
    /// </summary>
    public const int OwnSizeVariant = 0;
}
