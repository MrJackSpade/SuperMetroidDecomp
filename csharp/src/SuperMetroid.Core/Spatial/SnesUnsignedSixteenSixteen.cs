namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned native 16.16 speed kept as the cartridge's two words: a whole word and a fraction word, each 0 through
/// 65535. Samus's speeds are magnitudes in this form; their direction is held separately.
/// </summary>
[EncodingOwner("unsigned 16.16 fixed-point number")]
public readonly record struct SnesUnsignedSixteenSixteen
{
    /// <summary>Holds the two words, each checked to lie within a native word.</summary>
    public SnesUnsignedSixteenSixteen(int whole, int fraction)
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
    public static SnesUnsignedSixteenSixteen FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>This speed with its whole word replaced.</summary>
    public SnesUnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);

    /// <summary>
    /// The cartridge's cap test: each word is compared with the cap's word on its own, and both differences must have
    /// the sign bit clear. This is not an unsigned 32-bit comparison; a fraction word of $8000 or more fails it even
    /// when the whole word is above the cap.
    /// </summary>
    public bool WordsAtLeast(SnesUnsignedSixteenSixteen cap) =>
        unchecked((short)(Whole - cap.Whole)) >= 0 && unchecked((short)(Fraction - cap.Fraction)) >= 0;
}
