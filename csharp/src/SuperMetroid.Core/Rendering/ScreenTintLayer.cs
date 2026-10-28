using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>Adds fixed five-bit RGB to the composed screen, saturating each component.</summary>
/// <param name="Red">Fixed red intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Green">Fixed green intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Blue">Fixed blue intensity, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
public sealed record ScreenTintLayer(int Red, int Green, int Blue) : DisplayLayer
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
