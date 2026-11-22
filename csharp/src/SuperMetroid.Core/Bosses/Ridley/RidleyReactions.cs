using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The reactions of Ridley's own that its frames' hitboxes name (#1759). Only Ridley's own behaviour tells them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static partial class RidleyReactions
{
    /// <summary>The touch reaction Ridley's frames' hitboxes name.</summary>
    internal static EnemyReaction FrameHitboxTouch { get; } = EnemyReaction.OfSpecies("Ridley.FrameHitboxTouch");

    /// <summary>Ridley's shot reaction.</summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.RidleyShot;
}
