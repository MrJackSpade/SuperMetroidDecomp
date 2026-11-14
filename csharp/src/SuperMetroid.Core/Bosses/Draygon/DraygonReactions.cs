using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The reactions of Draygon's own that its frames' hitboxes name (#1759). Only Draygon's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class DraygonReactions
{
    /// <summary>Draygon's touch reaction.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Draygon.Touch");

    /// <summary>Draygon's shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Draygon.Shot");
}
