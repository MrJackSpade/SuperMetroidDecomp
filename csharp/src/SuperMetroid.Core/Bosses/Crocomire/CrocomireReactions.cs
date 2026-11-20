using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The reactions of Crocomire's own that its frames' hitboxes name (#1759). Only Crocomire's own behaviour tells them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class CrocomireReactions
{
    /// <summary>The claws' touch reaction, which pushes Samus back.</summary>
    internal static EnemyReaction ClawTouch { get; } = EnemyReaction.OfSpecies("Crocomire.ClawTouch");

    /// <summary>
    /// The shot reaction of his raised and high mouth and his lower body, which absorbs the shot: it neither hurts him nor
    /// raises dust, and nothing else of his changes with it (#1939, #1620).
    /// </summary>
    internal static EnemyReaction AbsorbedShot { get; } = EnemyReaction.OfSpecies("Crocomire.AbsorbedShot");

    /// <summary>
    /// The shot reaction of his body and his corpse, which raises dust where the shot strikes.
    /// </summary>
    internal static EnemyReaction DustShot { get; } = EnemyReaction.OfSpecies("Crocomire.DustShot");

    /// <summary>The open mouth's shot reaction.</summary>
    internal static EnemyReaction MouthShot { get; } = EnemyReaction.OfSpecies("Crocomire.MouthShot");
}
