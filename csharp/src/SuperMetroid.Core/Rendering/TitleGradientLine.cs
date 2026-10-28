using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>The five-bit colour one screen line of the title gradient blends in, and how it blends.</summary>
/// <param name="Red">The line's red, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Green">The line's green, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Blue">The line's blue, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Control">The layers the colour blends with and whether it is subtracted.</param>
public readonly record struct TitleGradientLine(int Red, int Green, int Blue, ColorBlend Control)
{
    /// <summary>The line's red.</summary>
    public int Red
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Red);

    /// <summary>The line's green.</summary>
    public int Green
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Green);

    /// <summary>The line's blue.</summary>
    public int Blue
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Blue);
}
