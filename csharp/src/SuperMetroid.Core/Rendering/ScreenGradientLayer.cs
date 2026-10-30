using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gradient drawn over the composed layers below it (#1468): on each line of its band, the covering stop's colour is
/// subtracted from the floor and the backdrop, or added to them and to the frontmost sprites that blend, each five-bit
/// channel clamped.
/// </summary>
public sealed record ScreenGradientLayer : DisplayLayer
{
    /// <summary>The gradient the layer draws.</summary>
    public ScreenGradient Gradient { get; }

    /// <summary>Draws <paramref name="gradient"/>, whose stops each use one of the two blends the layer draws.</summary>
    /// <param name="gradient">The gradient drawn.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gradient"/> is null.</exception>
    /// <exception cref="ArgumentException">A stop blends in a way the layer does not draw.</exception>
    public ScreenGradientLayer(ScreenGradient gradient)
    {
        Ensure.NotNull(gradient);
        foreach (GradientStop stop in gradient.Stops)
        {
            if (!IsDrawnBlend(stop.Blend))
                throw new ArgumentException("A gradient stop blends in an unsupported way.", nameof(gradient));
        }
        Gradient = gradient;
    }

    /// <summary>The two blends the layer draws: subtracting from the floor and backdrop, or adding to them and the sprites.</summary>
    private static bool IsDrawnBlend(ColorBlend blend) => blend is
        (ColorBlend.Subtract | ColorBlend.Backdrop | ColorBlend.Bg1) or
        (ColorBlend.Backdrop | ColorBlend.Sprites | ColorBlend.Bg1);
}
