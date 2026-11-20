namespace Engine.Shared;

/// <summary>
/// An exact distance along one axis: whole pixels plus a fraction of a pixel. Gameplay positions, speeds and
/// distances are made of these and never of floating-point numbers, so the same inputs always give the same result.
/// </summary>
/// <remarks>
/// The whole part counts pixels rightward or downward and may be negative. The fraction is always a non-negative
/// part of one pixel on top of it, so minus a quarter of a pixel is whole -1 with a fraction of three quarters.
/// </remarks>
public readonly struct Pixels : IEquatable<Pixels>, IComparable<Pixels>
{
    /// <summary>How many equal parts one pixel is divided into.</summary>
    public const int FractionsPerPixel = 65536;

    /// <summary>The distance in fractions of a pixel.</summary>
    private readonly long _fractions;

    private Pixels(long fractions) => _fractions = fractions;

    /// <summary>No distance.</summary>
    public static Pixels Zero => default;

    /// <summary>A whole number of pixels.</summary>
    public static Pixels FromWhole(int whole) => new((long)whole * FractionsPerPixel);

    /// <summary>Whole pixels plus a fraction of one more, the fraction counted in <see cref="FractionsPerPixel"/>ths.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The fraction is not from 0 up to, not including, a whole pixel.</exception>
    public static Pixels FromParts(int whole, int fraction)
    {
        if (fraction < 0 || fraction >= FractionsPerPixel)
            throw new ArgumentOutOfRangeException(nameof(fraction), fraction, $"A fraction is 0 through {FractionsPerPixel - 1}.");
        return new((long)whole * FractionsPerPixel + fraction);
    }

    /// <summary>
    /// A distance from a number of pixels such as 1.25, taken to the nearest part of a pixel. This is how a value shown
    /// in the editor, which can only show ordinary numbers, becomes exact; every part of a pixel the game can hold is a
    /// number the editor shows without loss.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is not finite or is too large.</exception>
    public static Pixels FromNumber(double pixels)
    {
        if (!double.IsFinite(pixels) || Math.Abs(pixels) > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pixels), pixels, "Not a number of pixels the game can hold.");
        return new((long)Math.Round(pixels * FractionsPerPixel, MidpointRounding.AwayFromZero));
    }

    /// <summary>The distance as a number of pixels, exactly, for showing in the editor.</summary>
    public double ToNumber() => (double)_fractions / FractionsPerPixel;

    /// <summary>The whole pixels, rounded toward the left or top: the pixel this distance lies in.</summary>
    public int Whole => checked((int)((_fractions - Fraction) / FractionsPerPixel));

    /// <summary>The part of a pixel beyond <see cref="Whole"/>, 0 up to, not including, a whole pixel.</summary>
    public int Fraction => (int)(((_fractions % FractionsPerPixel) + FractionsPerPixel) % FractionsPerPixel);

    /// <summary>Whether this is a distance leftward or upward.</summary>
    public bool IsNegative => _fractions < 0;

    /// <summary>Whether this is no distance at all.</summary>
    public bool IsZero => _fractions == 0;

    /// <summary>This distance with its whole pixels replaced and its fraction kept.</summary>
    public Pixels WithWhole(int whole) => FromParts(whole, Fraction);

    /// <summary>This distance with its fraction replaced and its whole pixels kept.</summary>
    public Pixels WithFraction(int fraction) => FromParts(Whole, fraction);

    /// <summary>The size of this distance, without its direction.</summary>
    public Pixels Abs() => new(Math.Abs(_fractions));

    /// <summary>The distance as a floating-point number of pixels, for drawing only. Nothing in gameplay reads it.</summary>
    public float ToDisplay() => (float)((double)_fractions / FractionsPerPixel);

    public static Pixels operator +(Pixels left, Pixels right) => new(checked(left._fractions + right._fractions));

    public static Pixels operator -(Pixels left, Pixels right) => new(checked(left._fractions - right._fractions));

    public static Pixels operator -(Pixels value) => new(checked(-value._fractions));

    public static Pixels operator *(Pixels value, int times) => new(checked(value._fractions * times));

    public static bool operator ==(Pixels left, Pixels right) => left._fractions == right._fractions;

    public static bool operator !=(Pixels left, Pixels right) => left._fractions != right._fractions;

    public static bool operator <(Pixels left, Pixels right) => left._fractions < right._fractions;

    public static bool operator >(Pixels left, Pixels right) => left._fractions > right._fractions;

    public static bool operator <=(Pixels left, Pixels right) => left._fractions <= right._fractions;

    public static bool operator >=(Pixels left, Pixels right) => left._fractions >= right._fractions;

    /// <inheritdoc/>
    public bool Equals(Pixels other) => _fractions == other._fractions;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Pixels other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _fractions.GetHashCode();

    /// <inheritdoc/>
    public int CompareTo(Pixels other) => _fractions.CompareTo(other._fractions);

    /// <summary>The distance as "whole+fraction/65536", for messages.</summary>
    public override string ToString() => $"{Whole}+{Fraction}/{FractionsPerPixel}";
}
