namespace SuperMetroid.Core.Rendering;

/// <summary>Adds fixed five-bit RGB to the composed screen, saturating each component.</summary>
/// <param name="Red">Fixed red intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</param>
/// <param name="Green">Fixed green intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</param>
/// <param name="Blue">Fixed blue intensity, 0 through <see cref="Bgr555.MaxChannel"/>.</param>
public sealed record FixedColorAddRenderLayer(int Red, int Green, int Blue) : RenderLayer
{
    /// <summary>Fixed red intensity.</summary>
    public int Red
    {
        get => red;
        init => red = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, value);
    }

    /// <summary>Validated store of <see cref="Red"/>.</summary>
    private readonly int red = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, Red);

    /// <summary>Fixed green intensity.</summary>
    public int Green
    {
        get => green;
        init => green = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, value);
    }

    /// <summary>Validated store of <see cref="Green"/>.</summary>
    private readonly int green = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, Green);

    /// <summary>Fixed blue intensity.</summary>
    public int Blue
    {
        get => blue;
        init => blue = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, value);
    }

    /// <summary>Validated store of <see cref="Blue"/>.</summary>
    private readonly int blue = Ensure.BetweenInclusive(0, Bgr555.MaxChannel, Blue);
}
