using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The reactions of Phantoon's own that its frames' hitboxes name (#1759). Only Phantoon's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class PhantoonReactions
{
    /// <summary>Phantoon's body touch reaction.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Phantoon.Touch");

    /// <summary>Phantoon's body shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Phantoon.Shot");
}
