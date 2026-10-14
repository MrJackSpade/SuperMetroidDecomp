using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Programs;

/// <summary>Validated decoding of native words that select a member of a closed enum.</summary>
public static class ClosedNativeWords
{
    /// <summary>Decodes <paramref name="word"/> or throws, naming <paramref name="meaning"/>.</summary>
    public static TEnum Decode<TEnum>(ushort word, string meaning)
        where TEnum : struct, Enum
    {
        TEnum value = Unsafe.BitCast<ushort, TEnum>(word);
        return Enum.IsDefined(value)
            ? value
            : throw new InvalidDataException($"${word:X4} is not a {meaning} ({typeof(TEnum).Name}).");
    }

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
