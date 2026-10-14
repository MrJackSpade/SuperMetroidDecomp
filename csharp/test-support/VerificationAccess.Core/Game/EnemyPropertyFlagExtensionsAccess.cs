using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="EnemyPropertyFlagExtensions"/> members production does not use.</summary>
internal static class EnemyPropertyFlagExtensionsAccess
{
    /// <summary>
    /// Reads the typed high-byte flags while retaining the family-specific low byte in the
    /// caller's raw word. This is the explicit escape hatch for lossless cartridge records.
    /// </summary>
    internal static EnemyProperties ReadFlagsChecked(this ushort word)
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

    /// <summary>Tests whether any requested named high-byte property is set.</summary>
    internal static bool HasAny(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) != 0;

    /// <summary>Tests whether all requested named high-byte properties are set.</summary>
    internal static bool HasAll(this ushort word, EnemyProperties flags) =>
        (word.ReadFlagsChecked() & flags) == flags;

    /// <summary>Validates and tests the requested extra-property bits in a native word.</summary>
    internal static bool HasAll(this ushort word, EnemyExtraProperties flags)
    {
        EnemyPropertyFlagExtensions.ValidateKnown(flags);
        return ((EnemyExtraProperties)word & flags) == flags;
    }
}
