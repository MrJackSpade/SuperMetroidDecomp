using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How Crocomire's room lists his parts (#2556): his tongue right after his body.</summary>
internal static class CrocomirePartListingDefinitions
{
    /// <summary>His body, then his tongue.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } = new([EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue]);
}
