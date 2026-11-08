namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to enemy contact, processing and presence queries production does not use.</summary>
internal static class EnemyStateFlagExtensionsAccess
{
    /// <summary>Tests whether all requested contact flags are set.</summary>
    internal static bool HasAll(this EnemyContact contact, EnemyContact flags) => (contact & flags) == flags;

    /// <summary>Tests whether all requested processing flags are set.</summary>
    internal static bool HasAll(this EnemyProcessing processing, EnemyProcessing flags) => (processing & flags) == flags;

    /// <summary>Tests whether all requested presence flags are set.</summary>
    internal static bool HasAll(this EnemyPresence presence, EnemyPresence flags) => (presence & flags) == flags;
}
