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

    /// <summary>The Pirates' own shot reactions: the Pirate shot and the gold Ninja's two body reactions.</summary>
    internal static IReadOnlyList<EnemyReaction> Own { get; } = [Shot, GoldNinjaVulnerableHitboxShot, GoldNinjaInvincibleHitboxShot];

    /// <summary>The reactions that run a gold Ninja's ordinary shot reaction: the Pirate shot and the common one.</summary>
    internal static IReadOnlyList<EnemyReaction> GoldNinjaOrdinary { get; } = [Shot, EnemyReaction.OrdinaryShot];

    /// <summary>The armour reactions that reflect or ignore every weapon: the gold Ninja's invincible body.</summary>
    internal static IReadOnlyList<EnemyReaction> InvincibleArmour { get; } = [GoldNinjaInvincibleHitboxShot];

    /// <summary>The reactions whose damage to a Pirate but the gold Ninja shows the enemy-hit burst: the common one and the Pirates' own shot.</summary>
    internal static IReadOnlyList<EnemyReaction> ShowingHitBurst { get; } = [EnemyReaction.OrdinaryShot, Shot];
}
