namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Decoding and validation shared by <see cref="EnemyPropertyWord"/> and <see cref="EnemyExtraPropertyWord"/>.
/// </summary>
public static class EnemyPropertyFlagExtensions
{

    /// <summary>Reads the typed high-byte flags of a native property word; an unnamed high-byte flag is corrupt.</summary>
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

    /// <summary>Rejects low-byte values before changing the named high-byte property flags.</summary>
    internal static void ValidateKnown(EnemyProperties flags)
    {
        if (((ushort)flags & ~EnemyPropertyMasks.KnownFlags) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyProperties accepts only named high-byte flags.");
        }
    }

    /// <summary>The extra-property flags whose engine behavior is translated.</summary>
    internal const EnemyExtraProperties TranslatedExtraFlags =
        EnemyExtraProperties.UsesExtendedSpritemap | EnemyExtraProperties.NewInstructionFrame;

    /// <summary>Rejects extra-property bits whose behavior has not been translated.</summary>
    internal static void ValidateKnown(EnemyExtraProperties flags)
    {
        if ((flags & ~TranslatedExtraFlags) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyExtraProperties accepts only proven flags.");
        }
    }
}
