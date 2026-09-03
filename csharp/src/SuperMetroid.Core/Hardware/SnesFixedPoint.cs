namespace SuperMetroid.Core.Hardware;

/// <summary>A signed native 8.8 velocity word, retained losslessly in cartridge form.</summary>
public readonly record struct SnesSignedEightEight(ushort RawValue)
{
    /// <summary>Signed whole-pixel byte.</summary>
    public sbyte WholePixels => unchecked((sbyte)(RawValue >> 8));

    /// <summary>Unsigned 1/256-pixel fraction byte.</summary>
    public byte Fraction => unchecked((byte)RawValue);

    /// <summary>Creates a value from a signed raw 8.8 word without changing its bits.</summary>
    public static SnesSignedEightEight FromSignedRaw(short rawValue) =>
        new(unchecked((ushort)rawValue));

    /// <summary>Creates an exact whole-pixel velocity and rejects values outside 8.8 range.</summary>
    public static SnesSignedEightEight FromWholePixels(int pixels) =>
        new(unchecked((ushort)(checked((sbyte)pixels) << 8)));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SnesSignedSixteenSixteen ToSixteenSixteenDelta() =>
        SnesSignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);

    /// <summary>Adds raw 1/256-pixel units with native 16-bit wraparound.</summary>
    public SnesSignedEightEight AddRawWrapping(int rawDelta) =>
        new(unchecked((ushort)(RawValue + rawDelta)));
}

/// <summary>A signed native 16.16 displacement with explicit whole and fractional halves.</summary>
public readonly record struct SnesSignedSixteenSixteen
{
    private SnesSignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    public int RawValue { get; }
    public short WholePixels => unchecked((short)(RawValue >> 16));
    public ushort Fraction => unchecked((ushort)RawValue);

    /// <summary>Preserves an existing signed 32-bit native fixed-point value.</summary>
    public static SnesSignedSixteenSixteen FromRaw(int rawValue) => new(rawValue);

    /// <summary>Combines the native signed whole word and unsigned fractional word.</summary>
    public static SnesSignedSixteenSixteen FromParts(short wholePixels, ushort fraction) =>
        new(unchecked((wholePixels << 16) | fraction));

    /// <summary>Creates an exact whole-pixel delta with checked signed-word conversion.</summary>
    public static SnesSignedSixteenSixteen FromWholePixels(int pixels) =>
        FromParts(checked((short)pixels), 0);

    /// <summary>Adds two deltas with the 65816's native 32-bit pair wraparound.</summary>
    public SnesSignedSixteenSixteen AddWrapping(SnesSignedSixteenSixteen other) =>
        new(unchecked(RawValue + other.RawValue));

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
public readonly record struct SnesFixedPosition(ushort Whole, ushort Fraction);
