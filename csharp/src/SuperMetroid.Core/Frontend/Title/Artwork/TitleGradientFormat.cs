namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>The number of gradients the title selects from by zoom.</summary>
public static class TitleGradientFormat
{
    /// <summary>Sixteen variants, one for each zoom bucket the title selects from.</summary>
    public const int VariantCount = 16;

    /// <summary>
    /// The zoom bucket the title floor's zoom selects its gradient by (#1619): the zoom divided by 16, modulo 16, sixteen buckets of sixteen
    /// zoom steps each, so every other bit of the zoom leaves the gradient unchanged.
    /// </summary>
    public static readonly UpdateCounterField ZoomBucket = new(4, 4);
}
