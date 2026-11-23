namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>The title's gradients: how many there are and the zoom range each covers.</summary>
public static partial class TitleGradientFormat
{
    /// <summary>Sixteen gradients, one for each zoom range, in order of zoom from no zoom up.</summary>
    public const int VariantCount = 16;

    /// <summary>
    /// How wide a zoom range each gradient covers (#1781): the sixteen ranges
    /// together cover every zoom from no zoom up to just below the floor's own size.
    /// </summary>
    public static FloorZoomStep ZoomRangeWidth { get; } = FloorZoomStep.FromFloorPixelsPerScreenPixel(1, 16);

}
