namespace SuperMetroid.Core.Spatial;

/// <summary>A signed native 8.8 velocity word, retained losslessly in cartridge form.</summary>
/// <param name="RawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
[SnesEncodingOwner]
public readonly record struct SnesSignedEightEight(ushort RawValue)
{
    /// <summary>The raw word read as its signed two's-complement value.</summary>
    public short SignedRawValue => unchecked((short)RawValue);

    /// <summary>Creates a value from a signed raw 8.8 word without changing its bits.</summary>
    public static SnesSignedEightEight FromSignedRaw(short rawValue) =>
        new(unchecked((ushort)rawValue));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SnesSignedSixteenSixteen ToSixteenSixteenDelta() =>
        SnesSignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}
