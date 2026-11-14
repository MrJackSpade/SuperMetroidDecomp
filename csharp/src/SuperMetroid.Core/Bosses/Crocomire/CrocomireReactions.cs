using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The reactions of Crocomire's own that its frames' hitboxes name (#1759). Only Crocomire's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class CrocomireReactions
{
    /// <summary>The claws' touch reaction, which pushes Samus back.</summary>
    internal static EnemyReaction ClawTouch { get; } = EnemyReaction.OfSpecies("Crocomire.ClawTouch");

    /// <summary>The shot reaction that counts the hit in the fight's flags without other effect.</summary>
    internal static EnemyReaction HitCountShot { get; } = EnemyReaction.OfSpecies("Crocomire.HitCountShot");

    /// <summary>The shot reaction of the hitboxes that raise dust where the shot strikes.</summary>
    internal static EnemyReaction DustHitboxShot { get; } = EnemyReaction.OfSpecies("Crocomire.DustHitboxShot");

    /// <summary>The open mouth's shot reaction.</summary>
    internal static EnemyReaction MouthShot { get; } = EnemyReaction.OfSpecies("Crocomire.MouthShot");

    /// <summary>The alternate dust hitboxes' shot reaction.</summary>
    internal static EnemyReaction AlternateDustHitboxShot { get; } = EnemyReaction.OfSpecies("Crocomire.AlternateDustHitboxShot");
}
