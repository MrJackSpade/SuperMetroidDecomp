using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The reactions of Mother Brain's own that its frames' hitboxes name (#1759). Only Mother Brain's handlers tell them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class MotherBrainReactions
{
    /// <summary>The body's shot reaction.</summary>
    internal static EnemyReaction BodyShot { get; } = EnemyReaction.OfSpecies("MotherBrain.BodyShot");

    /// <summary>The head's shot reaction.</summary>
    internal static EnemyReaction HeadShot { get; } = EnemyReaction.OfSpecies("MotherBrain.HeadShot");

    /// <summary>The head's touch reaction.</summary>
    internal static EnemyReaction HeadTouch { get; } = EnemyReaction.OfSpecies("MotherBrain.HeadTouch");
}
