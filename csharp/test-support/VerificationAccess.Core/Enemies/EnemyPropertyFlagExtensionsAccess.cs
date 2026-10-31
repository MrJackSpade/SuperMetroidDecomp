namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to enemy property queries production does not use.</summary>
internal static class EnemyPropertyFlagExtensionsAccess
{
    /// <summary>Tests whether all requested properties are set.</summary>
    internal static bool HasAll(this EnemyProperties properties, EnemyProperties flags) => (properties & flags) == flags;
}
