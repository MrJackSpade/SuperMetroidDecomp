using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The reactions of the Torizos's own that its frames' hitboxes name (#1759). Only the Torizos's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class TorizoReactions
{
    /// <summary>The Torizos' touch reaction.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("Torizo.Touch");

    /// <summary>Bomb Torizo's shot reaction, which Golden Torizo's ordinary body hitboxes also name.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("Torizo.Shot");

    /// <summary>The shot reaction of the frames that stand up and sit down.</summary>
    internal static EnemyReaction StandUpSitDownShot { get; } = EnemyReaction.OfSpecies("Torizo.StandUpSitDownShot");

    /// <summary>Golden Torizo's own shot reaction, which its contact takes by its radius.</summary>
    internal static EnemyReaction GoldenShot { get; } = EnemyReaction.OfSpecies("Torizo.GoldenShot");
}
