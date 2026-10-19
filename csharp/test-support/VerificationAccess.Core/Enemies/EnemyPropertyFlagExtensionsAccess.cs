namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to enemy property-word queries production does not use.</summary>
internal static class EnemyPropertyFlagExtensionsAccess
{
    /// <summary>Tests whether all requested named high-byte properties are set.</summary>
    internal static bool HasAll(this EnemyPropertyWord word, EnemyProperties flags) => (word.Flags & flags) == flags;
}
