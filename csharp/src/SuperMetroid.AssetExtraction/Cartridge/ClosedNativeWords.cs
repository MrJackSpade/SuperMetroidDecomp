using System.Runtime.CompilerServices;

namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>
/// Validated decoding, at import, of native words that select a member of a closed enum. Gameplay never decodes a native
/// word into an identity; only cartridge import and the verification importers use this.
/// </summary>
internal static class ClosedNativeWords
{
    /// <summary>Decodes a native byte selector or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(byte value, string meaning)
        where TEnum : struct, Enum
    {
        TEnum decoded = Unsafe.BitCast<byte, TEnum>(value);
        return Enum.IsDefined(decoded)
            ? decoded
            : throw new InvalidDataException($"${value:X2} is not a {meaning} ({typeof(TEnum).Name}).");
    }
}
