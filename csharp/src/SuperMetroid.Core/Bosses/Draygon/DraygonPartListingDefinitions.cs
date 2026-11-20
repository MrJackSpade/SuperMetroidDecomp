using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>How Draygon's room lists his parts (#2298, #2556): his eye, tail and arms with his body.</summary>
internal static class DraygonPartListingDefinitions
{
    /// <summary>His body, then his eye, his tail and his arms.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } =
        new([EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail, EnemyDefinitionId.DraygonArms]);
}
