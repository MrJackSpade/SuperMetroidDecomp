namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 8.8 fixed-point velocity: eight whole bits and eight fraction bits, kept exactly.</summary>
/// <param name="RawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
[EncodingOwner("signed 8.8 fixed-point number")]
public readonly record struct SignedEightEight(ushort RawValue)
{
    /// <summary>The raw word read as its signed two's-complement value.</summary>
    public short SignedRawValue => unchecked((short)RawValue);

    /// <summary>Creates a value from a signed raw 8.8 word, -32768 through 32767, without changing its bits.</summary>
    public static SignedEightEight FromSignedRaw(int rawValue) =>
        new(unchecked((ushort)Ensure.BetweenInclusive(short.MinValue, short.MaxValue, rawValue)));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SignedSixteenSixteen ToSixteenSixteenDelta() =>
        SignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}
