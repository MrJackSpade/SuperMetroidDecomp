namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned native 16.16 speed kept as the cartridge's two words: a whole word and a fraction word, each 0 through
/// 65535. Samus's speeds are magnitudes in this form; their direction is held separately.
/// </summary>
[SnesEncodingOwner]
public readonly record struct SnesUnsignedSixteenSixteen
{
    /// <summary>Holds the two words, each checked to lie within a native word.</summary>
    public SnesUnsignedSixteenSixteen(int whole, int fraction)
    {
        Whole = Ensure.BetweenInclusive(0, ushort.MaxValue, whole);
        Fraction = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
    }

    /// <summary>The whole word, 0 through 65535.</summary>
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
}
