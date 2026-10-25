using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Five-bit fixed color and CGADSUB control for a physical title scanline.</summary>
/// <param name="Red">Fixed red intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Green">Fixed green intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Blue">Fixed blue intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Control">The layers the fixed color applies to and whether it is subtracted.</param>
public readonly record struct TitleGradientLine(int Red, int Green, int Blue, ColorBlend Control)
{
    /// <summary>Fixed red intensity.</summary>
    public int Red
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Red);

    /// <summary>Fixed green intensity.</summary>
    public int Green
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Green);

    /// <summary>Fixed blue intensity.</summary>
    public int Blue
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Blue);
}
