namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How bright a displayed frame is (#1462): a level from 0 (black) through <see cref="Full"/>. A frame has exactly one
/// brightness. Where several dimmings apply to the same frame, such as a scene's own fade under the session's screen
/// fade, they are combined into that one brightness where the frame is made, by <see cref="Combine"/>.
/// </summary>
public static class DisplayBrightness
{
    /// <summary>The brightness of a frame shown undimmed.</summary>
    public const int Full = 15;

    /// <summary>Returns <paramref name="level"/> once it is known to lie from black through full.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The level is below black or above full.</exception>
    public static int Require(int level) => Ensure.BetweenInclusive(0, Full, level);

    /// <summary>
    /// The one brightness of several dimmings applied together. Each level is a fraction of <see cref="Full"/>; the
    /// result is their product scaled back to 0 through <see cref="Full"/> and rounded to the nearest level once, never
    /// once per dimming. Full leaves the other levels unchanged, and any black level gives black.
    /// </summary>
    /// <param name="levels">The dimmings, each from black through full; at least one.</param>
    /// <exception cref="ArgumentException">No level is given.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A level is below black or above full.</exception>
    public static int Combine(params ReadOnlySpan<int> levels)
    {
        if (levels.IsEmpty) throw new ArgumentException("A brightness combines at least one level.", nameof(levels));
        long product = 1, scale = 1;
        foreach (int level in levels)
        {
            product *= Require(level);
            scale *= Full;
        }
        // The product is scaled by one Full per level; dividing by all but one leaves a level, rounded half up.
        long divisor = scale / Full;
        return (int)((product * 2 + divisor) / (divisor * 2));
    }
}
