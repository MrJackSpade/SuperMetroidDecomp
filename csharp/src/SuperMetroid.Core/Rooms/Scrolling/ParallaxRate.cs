namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The share of the foreground's movement a parallax background moves by (#2046): a fraction strictly between nothing
/// and the whole, exact to a 256th, so the background always trails the foreground.
/// </summary>
public sealed record ParallaxRate
{
    /// <summary>The 256ths in the whole of the foreground's movement.</summary>
    private const int Whole = 256;

    /// <summary>The rate in 256ths of the foreground's movement, 1 through 255.</summary>
    private readonly int _twoHundredFiftySixths;

    /// <summary>The rate of <paramref name="twoHundredFiftySixths"/> 256ths, already checked.</summary>
    private ParallaxRate(int twoHundredFiftySixths) => _twoHundredFiftySixths = twoHundredFiftySixths;

    /// <summary>
    /// The rate <paramref name="numerator"/> over <paramref name="denominator"/> of the foreground's movement.
    /// </summary>
    /// <param name="numerator">The fraction's numerator.</param>
    /// <param name="denominator">The fraction's denominator, positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The fraction is not a whole number of 256ths, or is not strictly between nothing and the whole.
    /// </exception>
    public static ParallaxRate Of(int numerator, int denominator)
    {
        int scaled = checked(numerator * Whole);
        Ensure.BetweenInclusive(1, Whole, denominator);
        if (scaled % denominator != 0)
            throw new ArgumentOutOfRangeException(nameof(denominator), denominator,
                $"A parallax rate of {numerator}/{denominator} is not a whole number of 256ths.");
        return new(Ensure.BetweenInclusive(1, Whole - 1, scaled / denominator));
    }

    /// <summary>
    /// The background position that trails <paramref name="foregroundPosition"/> at this rate: the product truncated to a
    /// whole pixel. A rate under the whole keeps the result within the foreground's coordinate range.
    /// </summary>
    /// <param name="foregroundPosition">The foreground's position on the axis, not negative.</param>
    internal int Trail(int foregroundPosition) =>
        checked((int)(checked((uint)foregroundPosition) * (uint)_twoHundredFiftySixths / Whole));

    /// <summary>The rate as a fraction in lowest terms.</summary>
    public override string ToString()
    {
        int numerator = _twoHundredFiftySixths;
        int denominator = Whole;
        while (numerator % 2 == 0)
        {
            numerator /= 2;
            denominator /= 2;
        }

        return $"{numerator}/{denominator}";
    }
}
