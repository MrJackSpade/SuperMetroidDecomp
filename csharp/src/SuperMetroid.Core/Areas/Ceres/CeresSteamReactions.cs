using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The reactions of Ceres steam's own that its frames' hitboxes name (#1759). Only Ceres steam's own behaviour tells them apart; a
/// definition names only the engine's shared reactions.
/// </summary>
internal static partial class CeresSteamReactions
{
    /// <summary>The steam's touch reaction, which every visible steam frame's hitbox names.</summary>
    internal static EnemyReaction Touch => SpeciesOwnReactions.CeresSteamTouch;
}
