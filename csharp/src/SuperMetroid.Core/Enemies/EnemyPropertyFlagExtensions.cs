namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Semantic operations over raw enemy words. Keeping the underlying slot fields as
/// <see cref="ushort"/> preserves their one-to-one WRAM/debugger representation.
/// </summary>
public static class EnemyPropertyFlagExtensions
{

    /// <summary>
    /// Reads the typed high-byte flags while retaining the family-specific low byte in the
    /// caller's raw word. This is the explicit escape hatch for lossless cartridge records.
    /// </summary>
    public static EnemyProperties ReadFlagsChecked(this ushort word)
    {
        ushort highByte = unchecked((ushort)(SnesLittleEndian.HighByteInPlace(word)));
        EnemyProperties flags = (EnemyProperties)highByte;
        if ((highByte & ~(ushort)(EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.BlocksPlasmaBeam |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.RespawnIfKilled |
                EnemyProperties.SolidToSamus)) != 0)
        {
            throw new InvalidDataException(
                $"Enemy property word ${word:X4} contains an unnamed high-byte flag.");
        }

        return flags;
    }

    /// <summary>Tests whether a raw enemy-property word contains at least one requested named flag.</summary>
    public static bool HasAny(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) != 0;

    /// <summary>Sets named enemy-property bits while preserving all other native word data.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains low-byte or undefined bits.</exception>
    public static ushort With(this ushort word, EnemyProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word | (ushort)flags));
    }

    /// <summary>Clears named enemy-property bits while preserving all other native word data.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains low-byte or undefined bits.</exception>
    public static ushort Without(this ushort word, EnemyProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word & ~(ushort)flags));
    }

    /// <summary>Clears and sets named bits while preserving family-specific low-byte data.</summary>
    public static ushort Replace(
        this ushort word,
        EnemyProperties clear,
        EnemyProperties set) =>
        word.Without(clear).With(set);

    /// <summary>Tests whether a raw extra-property word contains at least one requested translated flag.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static bool HasAny(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return ((EnemyExtraProperties)word & flags) != 0;
    }

    /// <summary>Sets translated extra-property bits while preserving every other native bit.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static ushort With(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word | (ushort)flags));
    }

    /// <summary>Clears translated extra-property bits while preserving every other native bit.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="flags"/> contains an untranslated bit.</exception>
    public static ushort Without(this ushort word, EnemyExtraProperties flags)
    {
        ValidateKnown(flags);
        return unchecked((ushort)(word & ~(ushort)flags));
    }

    /// <summary>
    /// Preserves explicitly named, still-untranslated extra-property bits without allowing
    /// a caller to smuggle a proven flag through a numeric mask. This keeps ROM state
    /// lossless while forcing understood behavior through <see cref="EnemyExtraProperties"/>.
    /// </summary>
    public static ushort WithUntranslatedExtraBits(
        this ushort word,
        ushort rawBits,
        string sourceContext)
    {
        const ushort knownMask =
            (ushort)(EnemyExtraProperties.UsesExtendedSpritemap |
                EnemyExtraProperties.NewInstructionFrame);
        if ((rawBits & knownMask) != 0)
        {
            throw new ArgumentException(
                $"{sourceContext} attempted to write named enemy extra-property bits " +
                $"through the raw escape hatch: ${rawBits:X4}.",
                nameof(rawBits));
        }

        return unchecked((ushort)(word | rawBits));
    }

    /// <summary>Rejects low-byte values before changing the named high-byte property flags.</summary>
    private static void ValidateKnown(EnemyProperties flags)
    {
        if (((ushort)flags & ~EnemyPropertyMasks.KnownFlags) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyProperties accepts only named high-byte flags.");
        }
    }

    /// <summary>Rejects extra-property bits whose behavior has not been translated.</summary>
    internal static void ValidateKnown(EnemyExtraProperties flags)
    {
        const EnemyExtraProperties known =
            EnemyExtraProperties.UsesExtendedSpritemap |
            EnemyExtraProperties.NewInstructionFrame;
        if ((flags & ~known) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyExtraProperties accepts only proven flags.");
        }
    }
}
