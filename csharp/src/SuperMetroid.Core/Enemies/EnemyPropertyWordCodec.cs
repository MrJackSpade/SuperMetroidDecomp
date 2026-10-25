namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The codec between the typed enemy property values (<see cref="EnemyPropertyWord"/> and
/// <see cref="EnemyExtraPropertyWord"/>, with their <see cref="EnemyProperties"/> and <see cref="EnemyExtraProperties"/>
/// flags) and the two property words the cartridge composes them into. The words stay exact because the population
/// records still carry them as decoded words, the slot's aliased enemy-RAM reads return them by offset, and the death
/// path publishes the respawn flag as the scratch word the power-bomb walker then reads as its radius. This type is the
/// one place either flag set becomes or leaves a word; the destination is population and slot state holding the typed
/// values directly, which leaves this conversion in the import pipeline only.
/// </summary>
[EncodingOwner("enemy property and extra-property words")]
internal static class EnemyPropertyWordCodec
{
    /// <summary>The property flags as the word bits the cartridge stores them in.</summary>
    internal static ushort ToNativeWord(this EnemyProperties flags) => (ushort)flags;

    /// <summary>A property value as its native word: the engine flags in the high byte, the family data in the low byte.</summary>
    internal static ushort Encode(EnemyPropertyWord value) => checked((ushort)((ushort)value.Flags | value.FamilyData));

    /// <summary>A native property word as a typed value; an unnamed high-byte flag is corrupt.</summary>
    /// <exception cref="InvalidDataException">The high byte holds a flag <see cref="EnemyProperties"/> does not name.</exception>
    internal static EnemyPropertyWord Decode(ushort word)
    {
        var flags = (EnemyProperties)SnesLittleEndian.HighByteInPlace(word);
        if ((flags & ~EnemyPropertyFlagExtensions.NamedProperties) != 0)
        {
            throw new InvalidDataException(
                $"Enemy property word ${word:X4} contains an unnamed high-byte flag.");
        }

        return new EnemyPropertyWord(flags, SnesLittleEndian.LowByte(word));
    }

    /// <summary>An extra-property value as its native word: the translated flags beside the untranslated bits.</summary>
    internal static ushort Encode(EnemyExtraPropertyWord value) =>
        checked((ushort)((ushort)value.Flags | value.UntranslatedBits));

    /// <summary>Whether a word of bits kept verbatim beside the extra-property flags would carry a translated flag.</summary>
    internal static bool CarriesTranslatedExtraFlag(ushort bits) =>
        ((EnemyExtraProperties)bits & EnemyPropertyFlagExtensions.TranslatedExtraFlags) != EnemyExtraProperties.None;

    /// <summary>A native extra-property word split into its translated flags and every other bit, verbatim.</summary>
    internal static EnemyExtraPropertyWord DecodeExtra(ushort word) => new(
        (EnemyExtraProperties)word & EnemyPropertyFlagExtensions.TranslatedExtraFlags,
        unchecked((ushort)(word & ~(ushort)EnemyPropertyFlagExtensions.TranslatedExtraFlags)));
}
