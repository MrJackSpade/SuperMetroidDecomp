namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The Space Pirates' own reactions, which their frames' hitboxes name (#1759). Only the Pirates' own responses tell them
/// apart; a definition names only the engine's shared reactions.
/// </summary>
internal static class SpacePirateReactions
{
    /// <summary>The Pirates' touch reaction.</summary>
    internal static EnemyReaction Touch { get; } = EnemyReaction.OfSpecies("SpacePirate.Touch");

    /// <summary>The Pirates' shot reaction.</summary>
    internal static EnemyReaction Shot { get; } = EnemyReaction.OfSpecies("SpacePirate.Shot");

    /// <summary>The gold ninja Pirate's vulnerable hitboxes' shot reaction.</summary>
    internal static EnemyReaction GoldNinjaVulnerableHitboxShot { get; } = EnemyReaction.OfSpecies("SpacePirate.GoldNinjaVulnerableHitboxShot");

    /// <summary>The gold ninja Pirate's invincible hitboxes' shot reaction.</summary>
    internal static EnemyReaction GoldNinjaInvincibleHitboxShot { get; } = EnemyReaction.OfSpecies("SpacePirate.GoldNinjaInvincibleHitboxShot");
}
