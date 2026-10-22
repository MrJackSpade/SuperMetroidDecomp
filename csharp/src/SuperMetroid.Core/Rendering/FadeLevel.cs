namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One fade the screen passes through after its scene is drawn (#1403): a brightness from black to full. A frame's fades
/// apply in order, each rounding its own result, so they are kept apart rather than multiplied together.
/// </summary>
public readonly record struct FadeLevel
{
    /// <summary>The brightness of an unfaded screen.</summary>
    public const int Full = 15;

    /// <summary>A fade to <paramref name="brightness"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The brightness is outside black to full.</exception>
    public FadeLevel(int brightness) => Brightness = Ensure.BetweenInclusive(0, Full, brightness);

    /// <summary>The brightness, from black (zero) to <see cref="Full"/>.</summary>
    public int Brightness { get; }
}
