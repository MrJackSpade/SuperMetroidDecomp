namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy.properties2 (+$10) as a typed value (#1323): the <see cref="EnemyExtraProperties"/> flags whose engine behavior
/// is translated, plus every other bit kept verbatim as <see cref="UntranslatedBits"/> (Crocomire's $0400, the grapple
/// attach bit, and the growing shutter's initializer selector, which it consumes and clears).
/// </summary>
public readonly record struct EnemyExtraPropertyWord
{
    /// <summary>Creates an extra-property value from translated flags and untranslated bits.</summary>
    /// <exception cref="ArgumentException">The two parts overlap or <paramref name="flags"/> is not translated.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="untranslatedBits"/> is not a word.</exception>
    public EnemyExtraPropertyWord(EnemyExtraProperties flags, int untranslatedBits = 0)
    {
        EnemyPropertyFlagExtensions.ValidateKnown(flags);
        _ = Ensure.BetweenInclusive(0, ushort.MaxValue, untranslatedBits);
        if (EnemyPropertyWordCodec.CarriesTranslatedExtraFlag(checked((ushort)untranslatedBits)))
            throw new ArgumentException($"Untranslated bits ${untranslatedBits:X4} include translated flags.", nameof(untranslatedBits));
        Flags = flags;
        UntranslatedBits = untranslatedBits;
    }

    /// <summary>The translated flags.</summary>
    public EnemyExtraProperties Flags { get; }

    /// <summary>
    /// Every other bit, verbatim, 0 through 65535. Every producer supplies a word (the constructor rejects anything
    /// else, and decoding masks the stored word), only bitwise operations combine them, and the stored word places them
    /// unchanged beside the flags.
    /// </summary>
    public int UntranslatedBits { get; }

    /// <summary>A stored word split into translated flags and untranslated bits.</summary>
    public static EnemyExtraPropertyWord FromWord(int word) => EnemyPropertyWordCodec.DecodeExtra(checked((ushort)word));

    /// <summary>The stored word.</summary>
    public ushort ToWord() => EnemyPropertyWordCodec.Encode(this);

    /// <summary>Whether any of <paramref name="flags"/> is set.</summary>
    public bool HasAny(EnemyExtraProperties flags) => (Flags & flags) != EnemyExtraProperties.None;

    /// <summary>This value with <paramref name="flags"/> set.</summary>
    public EnemyExtraPropertyWord With(EnemyExtraProperties flags) => new(Flags | flags, checked((ushort)UntranslatedBits));

    /// <summary>This value with <paramref name="flags"/> cleared.</summary>
    public EnemyExtraPropertyWord Without(EnemyExtraProperties flags) => new(Flags & ~flags, checked((ushort)UntranslatedBits));

    /// <summary>
    /// This value with explicitly named, still-untranslated bits set. A translated flag cannot be smuggled through
    /// this escape hatch.
    /// </summary>
    public EnemyExtraPropertyWord WithUntranslatedBits(int bits, string sourceContext) =>
        EnemyPropertyWordCodec.CarriesTranslatedExtraFlag(checked((ushort)bits))
            ? throw new ArgumentException(
                $"{sourceContext} attempted to write translated extra-property bits through the raw escape hatch: ${bits:X4}.",
                nameof(bits))
            : new(Flags, checked((ushort)(UntranslatedBits | bits)));
}
