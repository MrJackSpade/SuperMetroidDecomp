namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="EnemyPropertyFlagExtensions"/> members production does not use.</summary>
internal static class EnemyPropertyFlagExtensionsAccess
{
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
