namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Power Bomb's radius, or the change of radius one update of its blast makes, in whole pixels and fractions of a
/// pixel: from nothing up to the finest fraction short of the length's whole range. Lengths add, subtract and compare as
/// plain numbers.
/// </summary>
/// <remarks>
/// No explosion leaves that range, so no wrap is kept and a length outside it is refused. The stages that grow a
/// radius without testing the sum end long before it could pass the range: the white flash, the yellow blast and
/// Crystal Flash's bubble each stop well short of it. The flash slows for only a few updates from its starting speed,
/// so its speed never drops below nothing. The one place the range's end is load-bearing is the growth the shape
/// stages refuse when it would pass the range: the compatibility rule
/// <see cref="PowerBombGrowthHold"/>.
/// </remarks>
public readonly record struct PowerBombRadius : IComparable<PowerBombRadius>
{
    /// <summary>The fractions of a pixel in one pixel.</summary>
    internal const int FractionsPerPixel = 256;

    /// <summary>The whole pixels no Power Bomb length reaches: those its blast's growth holds short of.</summary>
    public const int UnreachedPixels = PowerBombGrowthHold.HoldingPixels;

    /// <summary>The whole pixels of radius in each band that selects one of the blast's glow colours.</summary>
    public const int GlowColorBandPixels = 8;

    /// <summary>Stores a range-checked length.</summary>
    private PowerBombRadius(int wholePixels, int fraction)
    {
        WholePixels = wholePixels;
        Fraction = fraction;
    }

    /// <summary>The whole pixels of the length, 0 through 255.</summary>
    public int WholePixels { get; }

    /// <summary>The fractions of a pixel past <see cref="WholePixels"/>, less than
    /// <see cref="FractionsPerPixel"/>.</summary>
    public int Fraction { get; }

    /// <summary>
    /// The band of <see cref="GlowColorBandPixels"/> whole pixels the radius reaches, 0 through 31: the step of a glow
    /// colour sequence a blast of this radius shows.
    /// </summary>
    public int GlowColorBand => WholePixels / GlowColorBandPixels;

    /// <summary>A zero radius or a stationary speed.</summary>
    public static PowerBombRadius Zero => default;

    /// <summary>A length of <paramref name="wholePixels"/> and <paramref name="fraction"/> fractions of a
    /// pixel.</summary>
    /// <param name="wholePixels">The whole pixels, less than 256.</param>
    /// <param name="fraction">The fractions of a pixel, less than <see cref="FractionsPerPixel"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">A part is outside its range.</exception>
    internal static PowerBombRadius FromPixels(int wholePixels, int fraction) =>
        new(Ensure.BetweenInclusive(0, UnreachedPixels - 1, wholePixels), Ensure.BetweenInclusive(0, FractionsPerPixel - 1, fraction));

    /// <summary>A whole number of pixels, 0 through 255, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixels"/> is outside 0 through 255.</exception>
    public static PowerBombRadius FromWholePixels(int pixels) => FromPixels(pixels, 0);

    /// <summary>
    /// A length of <paramref name="numerator"/> <paramref name="denominator"/>ths of a pixel (#2555), the denominator a power
    /// of two from 1 through 256 so the fraction is kept exactly: half a pixel, or three sixteenths of a pixel.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The denominator is no power of two from 1 through 256, or the length reaches 256 pixels.
    /// </exception>
    public static PowerBombRadius FromPixelFraction(int numerator, int denominator) =>
        Of(PowerOfTwoFraction.InFinestUnits(numerator, denominator, FractionsPerPixel));

    /// <summary>The sum of two lengths.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sum reaches 256 pixels.</exception>
    public static PowerBombRadius operator +(PowerBombRadius left, PowerBombRadius right) =>
        Of(left.InFractions + right.InFractions);

    /// <summary>The difference of two lengths.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="right"/> is longer than <paramref name="left"/>.</exception>
    public static PowerBombRadius operator -(PowerBombRadius left, PowerBombRadius right) =>
        Of(left.InFractions - right.InFractions);

    /// <summary>Whether <paramref name="left"/> is the shorter length.</summary>
    public static bool operator <(PowerBombRadius left, PowerBombRadius right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> is the longer length.</summary>
    public static bool operator >(PowerBombRadius left, PowerBombRadius right) => left.CompareTo(right) > 0;

    /// <summary>
    /// The whole pixels this radius would reach grown by <paramref name="speed"/>, counted past 255: what
    /// <see cref="PowerBombGrowthHold"/> tests before the radius grows.
    /// </summary>
    /// <param name="speed">The length the radius grows by.</param>
    public int WholePixelsGrownBy(PowerBombRadius speed) => (InFractions + speed.InFractions) / FractionsPerPixel;

    /// <inheritdoc />
    public int CompareTo(PowerBombRadius other) => InFractions.CompareTo(other.InFractions);

    /// <inheritdoc />
    public override string ToString() => $"{WholePixels} {Fraction}/256 px";

    /// <summary>The length counted in fractions of a pixel, the unit its arithmetic adds in.</summary>
    private int InFractions => WholePixels * FractionsPerPixel + Fraction;

    /// <summary>The length of <paramref name="fractions"/> fractions of a pixel.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The length is negative or reaches 256 pixels.</exception>
    private static PowerBombRadius Of(int fractions)
    {
        int checkedFractions = Ensure.BetweenInclusive(0, UnreachedPixels * FractionsPerPixel - 1, fractions);
        return new(checkedFractions / FractionsPerPixel, checkedFractions % FractionsPerPixel);
    }
}
