using System.Globalization;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How bright a displayed frame is (#1462, #2556): a step of the display's fade from <see cref="Black"/> to
/// <see cref="Full"/>, which the backend applies as the fraction of full light it stands for
/// (<see cref="FractionOfFull"/>). A frame has exactly one brightness. Where several dimmings apply to the same frame,
/// such as a scene's own fade under the session's screen fade, they are combined into that one brightness where the
/// frame is made, by <see cref="Combine"/>. Fades step it by whole steps (<see cref="Brighter"/>, <see cref="Darker"/>).
/// </summary>
public readonly record struct DisplayBrightness
{
    /// <summary>The steps of the fade from black to full light.</summary>
    private const int StepsToFull = 15;

    /// <summary>Holds the brightness <paramref name="stepsAboveBlack"/> steps above black.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step lies below black or above full.</exception>
    private DisplayBrightness(int stepsAboveBlack) => StepsAboveBlack = Ensure.BetweenInclusive(0, StepsToFull, stepsAboveBlack);

    /// <summary>A frame shown black.</summary>
    public static DisplayBrightness Black { get; } = new(0);

    /// <summary>A frame shown undimmed.</summary>
    public static DisplayBrightness Full { get; } = new(StepsToFull);

    /// <summary>How many whole steps the fade takes from black to full light.</summary>
    public static int StepsFromBlackToFull => StepsToFull;

    /// <summary>The brightness <paramref name="stepsAboveBlack"/> whole fade steps above black.</summary>
    /// <param name="stepsAboveBlack">The steps above black, from zero through <see cref="StepsFromBlackToFull"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step lies below black or above full.</exception>
    public static DisplayBrightness StepsAbove(int stepsAboveBlack) => new(stepsAboveBlack);

    /// <summary>How many whole fade steps this brightness lies above black.</summary>
    public int StepsAboveBlack { get; }

    /// <summary>Whether the frame is shown black.</summary>
    public bool IsBlack => StepsAboveBlack == 0;

    /// <summary>Whether the frame is shown undimmed.</summary>
    public bool IsFull => StepsAboveBlack == StepsToFull;

    /// <summary>The fraction of full light the brightness stands for, from zero for black to one for full.</summary>
    public float FractionOfFull => StepsAboveBlack / (float)StepsToFull;

    /// <summary>The brightness <paramref name="steps"/> whole steps brighter, never past full.</summary>
    /// <param name="steps">The steps brightened, zero or more.</param>
    public DisplayBrightness Brighter(int steps) => new(Math.Min(StepsToFull, StepsAboveBlack + Ensure.AtLeastZero(steps)));

    /// <summary>The brightness <paramref name="steps"/> whole steps darker, never past black.</summary>
    /// <param name="steps">The steps darkened, zero or more.</param>
    public DisplayBrightness Darker(int steps) => new(Math.Max(0, StepsAboveBlack - Ensure.AtLeastZero(steps)));

    /// <summary>
    /// The one brightness of several dimmings applied together. Each is a fraction of <see cref="Full"/>; the result is
    /// their product, rounded to the nearest whole step once, never once per dimming. Full leaves the others unchanged,
    /// and black gives black.
    /// </summary>
    /// <param name="levels">The dimmings; at least one.</param>
    /// <exception cref="ArgumentException">No dimming is given.</exception>
    public static DisplayBrightness Combine(params ReadOnlySpan<DisplayBrightness> levels)
    {
        if (levels.IsEmpty) throw new ArgumentException("A brightness combines at least one level.", nameof(levels));
        long product = 1, scale = 1;
        foreach (DisplayBrightness level in levels)
        {
            product *= level.StepsAboveBlack;
            scale *= StepsToFull;
        }
        // The product is scaled by one full fade per dimming; dividing by all but one leaves a step, rounded half up.
        long divisor = scale / StepsToFull;
        return new((int)((product * 2 + divisor) / (divisor * 2)));
    }

    /// <summary>The brightness spelled as its steps above black, as diagnostics and phase names show it.</summary>
    public override string ToString() => StepsAboveBlack.ToString(CultureInfo.InvariantCulture);
}
