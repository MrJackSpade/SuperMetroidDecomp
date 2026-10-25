namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The named flag sets and the validation shared by <see cref="EnemyPropertyWord"/> and <see cref="EnemyExtraPropertyWord"/>.
/// </summary>
public static class EnemyPropertyFlagExtensions
{
    /// <summary>Every engine property flag the cartridge defines; together they fill the property word's high byte.</summary>
    internal const EnemyProperties NamedProperties =
        EnemyProperties.Invisible |
        EnemyProperties.Deleted |
        EnemyProperties.IgnoreSamusCollision |
        EnemyProperties.ProcessOffScreen |
        EnemyProperties.BlocksPlasmaBeam |
        EnemyProperties.ProcessInstructions |
        EnemyProperties.RespawnIfKilled |
        EnemyProperties.SolidToSamus;

    /// <summary>Rejects low-byte values before changing the named high-byte property flags.</summary>
    internal static void ValidateKnown(EnemyProperties flags)
    {
        if ((flags & ~NamedProperties) != EnemyProperties.None)
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
        if ((flags & ~TranslatedExtraFlags) != EnemyExtraProperties.None)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flags), flags, "EnemyExtraProperties accepts only proven flags.");
        }
    }
}
