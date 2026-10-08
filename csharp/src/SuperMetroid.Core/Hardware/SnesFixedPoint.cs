namespace SuperMetroid.Core.Hardware;

/// <summary>A signed native 8.8 velocity word, retained losslessly in cartridge form.</summary>
/// <param name="RawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
public readonly record struct SnesSignedEightEight(ushort RawValue)
{
    /// <summary>Creates a value from a signed raw 8.8 word without changing its bits.</summary>
    public static SnesSignedEightEight FromSignedRaw(short rawValue) =>
        new(unchecked((ushort)rawValue));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SnesSignedSixteenSixteen ToSixteenSixteenDelta() =>
        SnesSignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}

/// <summary>A signed native 16.16 displacement with explicit whole and fractional halves.</summary>
public readonly record struct SnesSignedSixteenSixteen
{
    private SnesSignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    /// <summary>Gets the signed native word containing sixteen whole and sixteen fractional bits.</summary>
    public int RawValue { get; }

    /// <summary>Preserves an existing signed 32-bit native fixed-point value.</summary>
    public static SnesSignedSixteenSixteen FromRaw(int rawValue) => new(rawValue);

    /// <summary>Combines the native signed whole word and unsigned fractional word.</summary>
    public static SnesSignedSixteenSixteen FromParts(short wholePixels, ushort fraction) =>
        new(unchecked((wholePixels << 16) | fraction));

    /// <summary>Adds this delta to an unsigned wrapped 16.16 world-position pair.</summary>
    public SnesFixedPosition AddTo(ushort wholePosition, ushort subposition)
    {
        uint position = ((uint)wholePosition << 16) | subposition;
        position = unchecked(position + (uint)RawValue);
        return new SnesFixedPosition(
            unchecked((ushort)(position >> 16)),
            unchecked((ushort)position));
    }
}

/// <summary>The two WRAM words produced after applying a fixed-point displacement.</summary>
/// <param name="Whole">The unsigned whole-position word, wrapping at the native sixteen-bit boundary.</param>
/// <param name="Fraction">The unsigned fractional-position word representing fractions of one pixel.</param>
public readonly record struct SnesFixedPosition(ushort Whole, ushort Fraction);
