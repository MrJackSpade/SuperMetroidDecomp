using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The reactions of Draygon's own that its frames' hitboxes name (#1759). Only Draygon's own behaviour tells them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static class DraygonReactions
{
    /// <summary>Draygon's touch reaction.</summary>
    internal static EnemyReaction Touch => SpeciesOwnReactions.DraygonTouch;

    /// <summary>Draygon's shot reaction.</summary>
    internal static EnemyReaction Shot => SpeciesOwnReactions.DraygonShot;

    /// <summary>The shot reactions whose bombs run his own stages around common bomb damage: his own shot reaction.</summary>
    internal static IReadOnlyList<EnemyReaction> WithOwnBombStages => [Shot];

    /// <summary>The body's private shot reactions: his own and the dud shot, which a normal bomb triggers as it does the common one.</summary>
    internal static IReadOnlyList<EnemyReaction> PrivateShots => [Shot, EnemyReaction.DudShot];
}
