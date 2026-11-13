using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// physical collision for all four Ceres steam directions. Each of the
/// 28 frames has one zero-offset component. Its sprite-piece reference is deliberately
/// absent: installed sprite edits cannot change the damaging plume geometry.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Every plume rectangle's touch reaction: the steam scalds Samus.</summary>
    internal const EnemyReaction TouchReaction = EnemyReaction.CeresSteamTouch;

    /// <summary>Every plume rectangle's shot reaction: the steam ignores shots.</summary>
    internal const EnemyReaction ShotReaction = EnemyReaction.Ignore;

    /// <summary>
    /// The 21 hitbox lists: the shared empty list selected by both hidden poses, then five plume rectangles per
    /// direction. Each direction's rectangles are fitted to its drawing and are not exact rotations of another direction.
    /// </summary>
    internal static readonly EnemyHitboxRectangle[][] Lists =
    [
        [],
        [new(-8, -16, 7, -1)],
        [new(-8, -23, 7, -2)],
        [new(-8, -32, 7, -8)],
        [new(-8, -40, 7, -16)],
        [new(-8, -40, 6, -24)],
        [new(-16, -8, -1, 7)],
        [new(-24, -8, -2, 7)],
        [new(-32, -7, -9, 7)],
        [new(-40, -9, -17, 5)],
        [new(-40, -11, -26, 2)],
        [new(-8, 0, 7, 14)],
        [new(-8, 0, 7, 23)],
        [new(-8, 8, 7, 31)],
        [new(-8, 15, 7, 39)],
        [new(-8, 23, 6, 38)],
        [new(0, -8, 15, 7)],
        [new(1, -8, 23, 7)],
        [new(9, -8, 31, 7)],
        [new(18, -9, 38, 5)],
        [new(25, -11, 40, 3)],
    ];

    /// <summary>
    /// The 28 steam frames, seven poses per direction, each with one zero-offset component: the first five poses select
    /// their direction's plume list, the two hidden poses the empty list.
    /// </summary>
    internal static EnemyFrameCollision<CeresSteamCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CeresSteamUp, [new(Offset16<RoomPixels>.Zero, Lists[1])] },
        { EnemyFrameId.CeresSteamUpSecond, [new(Offset16<RoomPixels>.Zero, Lists[2])] },
        { EnemyFrameId.CeresSteamUpThird, [new(Offset16<RoomPixels>.Zero, Lists[3])] },
        { EnemyFrameId.CeresSteamUpFourth, [new(Offset16<RoomPixels>.Zero, Lists[4])] },
        { EnemyFrameId.CeresSteamUpFifth, [new(Offset16<RoomPixels>.Zero, Lists[5])] },
        { EnemyFrameId.CeresSteamUpSixth, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamUpSeventh, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamLeft, [new(Offset16<RoomPixels>.Zero, Lists[6])] },
        { EnemyFrameId.CeresSteamLeftSecond, [new(Offset16<RoomPixels>.Zero, Lists[7])] },
        { EnemyFrameId.CeresSteamLeftThird, [new(Offset16<RoomPixels>.Zero, Lists[8])] },
        { EnemyFrameId.CeresSteamLeftFourth, [new(Offset16<RoomPixels>.Zero, Lists[9])] },
        { EnemyFrameId.CeresSteamLeftFifth, [new(Offset16<RoomPixels>.Zero, Lists[10])] },
        { EnemyFrameId.CeresSteamLeftSixth, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamLeftSeventh, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamDown, [new(Offset16<RoomPixels>.Zero, Lists[11])] },
        { EnemyFrameId.CeresSteamDownSecond, [new(Offset16<RoomPixels>.Zero, Lists[12])] },
        { EnemyFrameId.CeresSteamDownThird, [new(Offset16<RoomPixels>.Zero, Lists[13])] },
        { EnemyFrameId.CeresSteamDownFourth, [new(Offset16<RoomPixels>.Zero, Lists[14])] },
        { EnemyFrameId.CeresSteamDownFifth, [new(Offset16<RoomPixels>.Zero, Lists[15])] },
        { EnemyFrameId.CeresSteamDownSixth, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamDownSeventh, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamRight, [new(Offset16<RoomPixels>.Zero, Lists[16])] },
        { EnemyFrameId.CeresSteamRightSecond, [new(Offset16<RoomPixels>.Zero, Lists[17])] },
        { EnemyFrameId.CeresSteamRightThird, [new(Offset16<RoomPixels>.Zero, Lists[18])] },
        { EnemyFrameId.CeresSteamRightFourth, [new(Offset16<RoomPixels>.Zero, Lists[19])] },
        { EnemyFrameId.CeresSteamRightFifth, [new(Offset16<RoomPixels>.Zero, Lists[20])] },
        { EnemyFrameId.CeresSteamRightSixth, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamRightSeventh, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
    };

}
