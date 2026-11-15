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
        [EnemyFrameId.CeresSteamUpSecond] = new(-8, -23, 7, -2),
        [EnemyFrameId.CeresSteamUpThird] = new(-8, -32, 7, -8),
        [EnemyFrameId.CeresSteamUpFourth] = new(-8, -40, 7, -16),
        [EnemyFrameId.CeresSteamUpFifth] = new(-8, -40, 6, -24),
        [EnemyFrameId.CeresSteamUpSixth] = null,
        [EnemyFrameId.CeresSteamUpSeventh] = null,
        [EnemyFrameId.CeresSteamLeft] = new(-16, -8, -1, 7),
        [EnemyFrameId.CeresSteamLeftSecond] = new(-24, -8, -2, 7),
        [EnemyFrameId.CeresSteamLeftThird] = new(-32, -7, -9, 7),
        [EnemyFrameId.CeresSteamLeftFourth] = new(-40, -9, -17, 5),
        [EnemyFrameId.CeresSteamLeftFifth] = new(-40, -11, -26, 2),
        [EnemyFrameId.CeresSteamLeftSixth] = null,
        [EnemyFrameId.CeresSteamLeftSeventh] = null,
        [EnemyFrameId.CeresSteamDown] = new(-8, 0, 7, 14),
        [EnemyFrameId.CeresSteamDownSecond] = new(-8, 0, 7, 23),
        [EnemyFrameId.CeresSteamDownThird] = new(-8, 8, 7, 31),
        [EnemyFrameId.CeresSteamDownFourth] = new(-8, 15, 7, 39),
        [EnemyFrameId.CeresSteamDownFifth] = new(-8, 23, 6, 38),
        [EnemyFrameId.CeresSteamDownSixth] = null,
        [EnemyFrameId.CeresSteamDownSeventh] = null,
        [EnemyFrameId.CeresSteamRight] = new(0, -8, 15, 7),
        [EnemyFrameId.CeresSteamRightSecond] = new(1, -8, 23, 7),
        [EnemyFrameId.CeresSteamRightThird] = new(9, -8, 31, 7),
        [EnemyFrameId.CeresSteamRightFourth] = new(18, -9, 38, 5),
        [EnemyFrameId.CeresSteamRightFifth] = new(25, -11, 40, 3),
        [EnemyFrameId.CeresSteamRightSixth] = null,
        [EnemyFrameId.CeresSteamRightSeventh] = null,
    };
}
