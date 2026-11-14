using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The reactions of Spore Spawn's own that its frames' hitboxes name (#1759). Only Spore Spawn's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class SporeSpawnReactions
{
    /// <summary>Spore Spawn's touch reaction.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("SporeSpawn.Touch");

    /// <summary>Spore Spawn's shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("SporeSpawn.Shot");
}
