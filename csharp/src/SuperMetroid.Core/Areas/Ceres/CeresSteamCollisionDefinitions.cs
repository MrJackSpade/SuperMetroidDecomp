using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where the Ceres steam scalds, in all four directions (#1954): each of the 28 steam frames maps to its plume's
/// rectangle about the vent, or to none while the plume is hidden. The rectangles are fixed: installed sprite edits
/// cannot change the damaging plume.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Every plume rectangle's touch reaction: the steam scalds Samus.</summary>
    internal static readonly EnemyReaction TouchReaction = CeresSteamReactions.Touch;

    /// <summary>Every plume rectangle's shot reaction: the steam ignores shots.</summary>
    internal static readonly EnemyReaction ShotReaction = EnemyReaction.Ignore;

    /// <summary>
    /// The 28 steam frames, seven poses per direction: the first five poses' plume rectangles about the vent, and none for
    /// the two hidden poses. Each direction's rectangles are fitted to its drawing and are not exact rotations of another
    /// direction.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, EnemyHitboxRectangle?> Plumes { get; } = new Dictionary<EnemyFrameId, EnemyHitboxRectangle?>
    {
        [EnemyFrameId.CeresSteamUp] = new(-8, -16, 7, -1),
        [EnemyFrameId.CeresSteamUpJet22] = new(-8, -23, 7, -2),
        [EnemyFrameId.CeresSteamUpJet31] = new(-8, -32, 7, -8),
        [EnemyFrameId.CeresSteamUpJet40] = new(-8, -40, 7, -16),
        [EnemyFrameId.CeresSteamUpDrifting33] = new(-8, -40, 6, -24),
        [EnemyFrameId.CeresSteamUpDrifting27] = null,
        [EnemyFrameId.CeresSteamUpDrifting16] = null,
        [EnemyFrameId.CeresSteamLeft] = new(-16, -8, -1, 7),
        [EnemyFrameId.CeresSteamLeftJet24] = new(-24, -8, -2, 7),
        [EnemyFrameId.CeresSteamLeftJet32] = new(-32, -7, -9, 7),
        [EnemyFrameId.CeresSteamLeftJet40] = new(-40, -9, -17, 5),
        [EnemyFrameId.CeresSteamLeftDrifting32] = new(-40, -11, -26, 2),
        [EnemyFrameId.CeresSteamLeftDrifting26] = null,
        [EnemyFrameId.CeresSteamLeftDrifting16] = null,
        [EnemyFrameId.CeresSteamDown] = new(-8, 0, 7, 14),
        [EnemyFrameId.CeresSteamDownJet22] = new(-8, 0, 7, 23),
        [EnemyFrameId.CeresSteamDownJet31] = new(-8, 8, 7, 31),
        [EnemyFrameId.CeresSteamDownJet39] = new(-8, 15, 7, 39),
        [EnemyFrameId.CeresSteamDownDrifting31] = new(-8, 23, 6, 38),
        [EnemyFrameId.CeresSteamDownDrifting24] = null,
        [EnemyFrameId.CeresSteamDownDrifting16] = null,
        [EnemyFrameId.CeresSteamRight] = new(0, -8, 15, 7),
        [EnemyFrameId.CeresSteamRightJet24] = new(1, -8, 23, 7),
        [EnemyFrameId.CeresSteamRightJet32] = new(9, -8, 31, 7),
        [EnemyFrameId.CeresSteamRightJet40] = new(18, -9, 38, 5),
        [EnemyFrameId.CeresSteamRightDrifting32] = new(25, -11, 40, 3),
        [EnemyFrameId.CeresSteamRightDrifting26] = null,
        [EnemyFrameId.CeresSteamRightDrifting16] = null,
    };
}
