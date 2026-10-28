using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>Tints the composed screen by adding a five-bit colour, each channel clamped at full.</summary>
/// <param name="Red">The red the tint adds, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Green">The green the tint adds, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
/// <param name="Blue">The blue the tint adds, 0 through <see cref="RgbColor.MaxChannel"/>.</param>
public sealed record ScreenTintLayer(int Red, int Green, int Blue) : DisplayLayer
{
    /// <summary>The red the tint adds.</summary>
    public int Red
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Red);

    /// <summary>The green the tint adds.</summary>
    public int Green
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Green);

    /// <summary>The blue the tint adds.</summary>
    public int Blue
    {
        get;
        init => field = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, value);
    } = Ensure.BetweenInclusive(0, RgbColor.MaxChannel, Blue);
}
