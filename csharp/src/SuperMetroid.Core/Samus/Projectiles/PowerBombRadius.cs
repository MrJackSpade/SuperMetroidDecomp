namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Power Bomb's radius, or the change of radius one update of its blast makes, in whole pixels and 256ths of a pixel:
/// from nothing up to 255 and 255/256 pixels. Lengths add, subtract and compare as plain numbers.
/// </summary>
/// <remarks>
/// No retail explosion leaves that range, so no wrap is kept and a length outside it is refused. The stages that grow a
/// radius without testing the sum end long before it could pass 255 pixels: the white flash at 146 pixels, the yellow
/// blast at 134 pixels and Crystal Flash's bubble at 32 pixels. The flash slows by half a pixel an update for at most
/// four updates from 48 pixels an update, so its speed never drops below nothing. The one place the range's end is
/// load-bearing is the growth the shape stages refuse when it would reach 256 pixels: the compatibility rule
/// <see cref="PowerBombGrowthHold"/>. The Power Bomb radius check runs every update of a Power Bomb and of a
/// Crystal Flash against the original's arithmetic to confirm it.
/// </remarks>
public readonly record struct PowerBombRadius : IComparable<PowerBombRadius>
{
    /// <summary>The 256ths of a pixel in one pixel.</summary>
    public const int FractionsPerPixel = 256;

    /// <summary>The whole pixels no Power Bomb length reaches: those its blast's growth holds short of.</summary>
    public const int UnreachedPixels = PowerBombGrowthHold.HoldingPixels;

    /// <summary>Stores a range-checked length.</summary>
    private PowerBombRadius(int wholePixels, int fraction)
    {
        WholePixels = wholePixels;
        Fraction = fraction;
    }

    /// <summary>The whole pixels of the length, 0 through 255.</summary>
    public int WholePixels { get; }

    /// <summary>The 256ths of a pixel past <see cref="WholePixels"/>, 0 through 255.</summary>
    public int Fraction { get; }

    /// <summary>A zero radius or a stationary speed.</summary>
    public static PowerBombRadius Zero => default;

    /// <summary>A length of <paramref name="wholePixels"/> and <paramref name="fraction"/> 256ths of a pixel.</summary>
    /// <param name="wholePixels">The whole pixels, 0 through 255.</param>
    /// <param name="fraction">The 256ths of a pixel, 0 through 255.</param>
    /// <exception cref="ArgumentOutOfRangeException">A part is outside its range.</exception>
    public static PowerBombRadius FromPixels(int wholePixels, int fraction) =>
        new(Ensure.BetweenInclusive(0, UnreachedPixels - 1, wholePixels), Ensure.BetweenInclusive(0, FractionsPerPixel - 1, fraction));

    /// <summary>A whole number of pixels, 0 through 255, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixels"/> is outside 0 through 255.</exception>
    public static PowerBombRadius FromWholePixels(int pixels) => FromPixels(pixels, 0);

    /// <summary>A length shorter than a pixel: <paramref name="fraction"/> 256ths of a pixel, 0 through 255.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is outside 0 through 255.</exception>
    public static PowerBombRadius FromFraction(int fraction) => FromPixels(0, fraction);

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

    /// <summary>Whether <paramref name="left"/> is no longer than <paramref name="right"/>.</summary>
    public static bool operator <=(PowerBombRadius left, PowerBombRadius right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> is no shorter than <paramref name="right"/>.</summary>
    public static bool operator >=(PowerBombRadius left, PowerBombRadius right) => left.CompareTo(right) >= 0;

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

    /// <summary>The length counted in 256ths of a pixel, the unit its arithmetic adds in.</summary>
    private int InFractions => WholePixels * FractionsPerPixel + Fraction;

    /// <summary>The length of <paramref name="fractions"/> 256ths of a pixel.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The length is negative or reaches 256 pixels.</exception>
    private static PowerBombRadius Of(int fractions)
    {
        int checkedFractions = Ensure.BetweenInclusive(0, UnreachedPixels * FractionsPerPixel - 1, fractions);
        return new(checkedFractions / FractionsPerPixel, checkedFractions % FractionsPerPixel);
    }
}
