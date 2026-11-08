namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A gradient drawn over the composed layers below it (#1468): on each line of its band, the covering stop's colour
/// darkens the floor and the backdrop, or brightens them and the frontmost sprites that take part in blends, as the stop's
/// blend says, each channel held within none and full.
/// </summary>
public sealed record ScreenGradientLayer : DisplayLayer
{
    /// <summary>The gradient the layer draws.</summary>
    public ScreenGradient Gradient { get; }

    /// <summary>Draws <paramref name="gradient"/>.</summary>
    /// <param name="gradient">The gradient drawn.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gradient"/> is null.</exception>
    /// <exception cref="ArgumentException">A stop's blend is not a defined gradient blend.</exception>
    public ScreenGradientLayer(ScreenGradient gradient)
    {
        Ensure.NotNull(gradient);
        foreach (GradientStop stop in gradient.Stops)
        {
            if (!Enum.IsDefined(stop.Blend))
                throw new ArgumentException("A gradient stop blends in an undefined way.", nameof(gradient));
        }
        Gradient = gradient;
    }
}
