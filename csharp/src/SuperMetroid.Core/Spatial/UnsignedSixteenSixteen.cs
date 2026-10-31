namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned 16.16 fixed-point speed kept as its two halves: a whole part and a fraction, each 0 through 65535.
/// Samus's speeds are magnitudes in this form; their direction is held separately.
/// </summary>
[EncodingOwner("unsigned 16.16 fixed-point number")]
public readonly record struct UnsignedSixteenSixteen
{
    /// <summary>Holds the two words, each checked to lie within a native word.</summary>
    public UnsignedSixteenSixteen(int whole, int fraction)
    {
        Whole = Ensure.BetweenInclusive(0, ushort.MaxValue, whole);
        Fraction = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
    }

    /// <summary>The whole word, 0 through 65535.</summary>
    [IntegerQuantity("whole-pixel part of an unsigned distance")]
    public int Whole { get; }

    /// <summary>The fraction word, 0 through 65535.</summary>
    public int Fraction { get; }

    /// <summary>Both words as one unsigned 32-bit value, the whole word high.</summary>
    public uint RawValue => ((uint)Whole << 16) | (uint)Fraction;

    /// <summary>Splits an unsigned 32-bit value into its whole and fraction words.</summary>
    public static UnsignedSixteenSixteen FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>A speed of <paramref name="sixteenths"/> sixteenths of a pixel, which must not be negative.</summary>
    public static UnsignedSixteenSixteen FromSixteenthsOfPixel(int sixteenths) => FromRaw(checked((uint)sixteenths * 4096u));

    /// <summary>
    /// Adds two values: a fraction sum that passes a whole moves the whole part by one, and the whole part wraps at its
    /// word.
    /// </summary>
    public static UnsignedSixteenSixteen operator +(UnsignedSixteenSixteen left, UnsignedSixteenSixteen right) =>
        FromRaw(unchecked(left.RawValue + right.RawValue));

    /// <summary>This speed with its whole word replaced.</summary>
    public UnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);

    /// <summary>
    /// The cartridge's cap test: each word is compared with the cap's word on its own, and both differences must have
    /// the sign bit clear. This is not an unsigned 32-bit comparison; a fraction word of $8000 or more fails it even
    /// when the whole word is above the cap.
    /// </summary>
    public bool WordsAtLeast(UnsignedSixteenSixteen cap) =>
        unchecked((short)(Whole - cap.Whole)) >= 0 && unchecked((short)(Fraction - cap.Fraction)) >= 0;
}
