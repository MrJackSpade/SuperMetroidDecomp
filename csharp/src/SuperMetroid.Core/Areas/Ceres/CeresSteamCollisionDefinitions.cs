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
    /// The 28 steam frames, seven poses per direction, each with one zero-offset component: the first five poses hold
    /// their plume's rectangle, the two hidden poses none. Each direction's rectangles are fitted to its drawing and are
    /// not exact rotations of another direction.
    /// </summary>
    internal static EnemyFrameCollision<CeresSteamCollisionComponent> Frames { get; } = new()
    {
        { EnemyFrameId.CeresSteamUp, [new(Offset16<RoomPixels>.Zero, [new(-8, -16, 7, -1)])] },
        { EnemyFrameId.CeresSteamUpSecond, [new(Offset16<RoomPixels>.Zero, [new(-8, -23, 7, -2)])] },
        { EnemyFrameId.CeresSteamUpThird, [new(Offset16<RoomPixels>.Zero, [new(-8, -32, 7, -8)])] },
        { EnemyFrameId.CeresSteamUpFourth, [new(Offset16<RoomPixels>.Zero, [new(-8, -40, 7, -16)])] },
        { EnemyFrameId.CeresSteamUpFifth, [new(Offset16<RoomPixels>.Zero, [new(-8, -40, 6, -24)])] },
        { EnemyFrameId.CeresSteamUpSixth, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamUpSeventh, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamLeft, [new(Offset16<RoomPixels>.Zero, [new(-16, -8, -1, 7)])] },
        { EnemyFrameId.CeresSteamLeftSecond, [new(Offset16<RoomPixels>.Zero, [new(-24, -8, -2, 7)])] },
        { EnemyFrameId.CeresSteamLeftThird, [new(Offset16<RoomPixels>.Zero, [new(-32, -7, -9, 7)])] },
        { EnemyFrameId.CeresSteamLeftFourth, [new(Offset16<RoomPixels>.Zero, [new(-40, -9, -17, 5)])] },
        { EnemyFrameId.CeresSteamLeftFifth, [new(Offset16<RoomPixels>.Zero, [new(-40, -11, -26, 2)])] },
        { EnemyFrameId.CeresSteamLeftSixth, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamLeftSeventh, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamDown, [new(Offset16<RoomPixels>.Zero, [new(-8, 0, 7, 14)])] },
        { EnemyFrameId.CeresSteamDownSecond, [new(Offset16<RoomPixels>.Zero, [new(-8, 0, 7, 23)])] },
        { EnemyFrameId.CeresSteamDownThird, [new(Offset16<RoomPixels>.Zero, [new(-8, 8, 7, 31)])] },
        { EnemyFrameId.CeresSteamDownFourth, [new(Offset16<RoomPixels>.Zero, [new(-8, 15, 7, 39)])] },
        { EnemyFrameId.CeresSteamDownFifth, [new(Offset16<RoomPixels>.Zero, [new(-8, 23, 6, 38)])] },
        { EnemyFrameId.CeresSteamDownSixth, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamDownSeventh, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamRight, [new(Offset16<RoomPixels>.Zero, [new(0, -8, 15, 7)])] },
        { EnemyFrameId.CeresSteamRightSecond, [new(Offset16<RoomPixels>.Zero, [new(1, -8, 23, 7)])] },
        { EnemyFrameId.CeresSteamRightThird, [new(Offset16<RoomPixels>.Zero, [new(9, -8, 31, 7)])] },
        { EnemyFrameId.CeresSteamRightFourth, [new(Offset16<RoomPixels>.Zero, [new(18, -9, 38, 5)])] },
        { EnemyFrameId.CeresSteamRightFifth, [new(Offset16<RoomPixels>.Zero, [new(25, -11, 40, 3)])] },
        { EnemyFrameId.CeresSteamRightSixth, [new(Offset16<RoomPixels>.Zero, [])] },
        { EnemyFrameId.CeresSteamRightSeventh, [new(Offset16<RoomPixels>.Zero, [])] },
    };

}
