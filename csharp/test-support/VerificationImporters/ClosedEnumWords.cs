namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Validated decoding of native words that select a member of a closed enum. Gameplay never decodes a native word into
/// an identity; only the verification importers use this.
/// </summary>
internal static class ClosedEnumWords
{
    /// <summary>Decodes a native byte selector or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(int value, string meaning)
        where TEnum : struct, Enum
    {
        if ((uint)value > byte.MaxValue)
            throw new InvalidDataException($"${value:X} is not a {meaning} byte ({typeof(TEnum).Name}).");
        // Converted by value, so the identity's own width does not matter (#1452).
        TEnum decoded = (TEnum)Enum.ToObject(typeof(TEnum), value);
        return Enum.IsDefined(decoded)
            ? decoded
            : throw new InvalidDataException($"${value:X2} is not a {meaning} ({typeof(TEnum).Name}).");
    }
}
