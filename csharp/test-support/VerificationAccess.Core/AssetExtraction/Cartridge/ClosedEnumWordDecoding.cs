namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Validated decoding of native sixteen-bit words that select a member of a closed enum, for the verification importers
/// and native snapshots; cartridge import decodes byte selectors through <see cref="ClosedEnumWords"/>.
/// </summary>
internal static class ClosedEnumWordDecoding
{
    /// <summary>Decodes <paramref name="word"/> or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(ushort word, string meaning)
        where TEnum : struct, Enum
    {
        // Converted by value, so the identity's own width does not matter (#1452).
        TEnum value = (TEnum)Enum.ToObject(typeof(TEnum), word);
        return Enum.IsDefined(value)
            ? value
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({typeof(TEnum).Name}).");
    }
}
