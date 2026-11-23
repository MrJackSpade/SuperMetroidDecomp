using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The reactions of Mother Brain's own that its frames' hitboxes name (#1759). Only Mother Brain's own behaviour tells them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static partial class MotherBrainReactions
{
    /// <summary>The body's shot reaction.</summary>
    internal static EnemyReaction BodyShot => SpeciesOwnReactions.MotherBrainBodyShot;

    /// <summary>The head's shot reaction.</summary>
    internal static EnemyReaction HeadShot => SpeciesOwnReactions.MotherBrainHeadShot;

    /// <summary>The head's touch reaction.</summary>
    internal static EnemyReaction HeadTouch => SpeciesOwnReactions.MotherBrainHeadTouch;
}
