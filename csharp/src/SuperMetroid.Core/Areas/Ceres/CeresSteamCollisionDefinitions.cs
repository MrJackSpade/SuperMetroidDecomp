using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Bank-$A6 physical collision for all four Ceres steam directions. Each of the
/// 28 extended frames has one zero-offset component. Its OAM pointer is deliberately
/// absent: installed sprite edits cannot change the damaging plume geometry.
/// </summary>
internal static class CeresSteamCollisionDefinitions
{
    /// <summary>Native bank of the extended maps at $A6:F142-$F250.</summary>
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
        { EnemyFrameId.CeresSteamPose0, [new(Offset16<RoomPixels>.Zero, Lists[1])] },
        { EnemyFrameId.CeresSteamPose1, [new(Offset16<RoomPixels>.Zero, Lists[2])] },
        { EnemyFrameId.CeresSteamPose2, [new(Offset16<RoomPixels>.Zero, Lists[3])] },
        { EnemyFrameId.CeresSteamPose3, [new(Offset16<RoomPixels>.Zero, Lists[4])] },
        { EnemyFrameId.CeresSteamPose4, [new(Offset16<RoomPixels>.Zero, Lists[5])] },
        { EnemyFrameId.CeresSteamPose5, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose6, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose7, [new(Offset16<RoomPixels>.Zero, Lists[6])] },
        { EnemyFrameId.CeresSteamPose8, [new(Offset16<RoomPixels>.Zero, Lists[7])] },
        { EnemyFrameId.CeresSteamPose9, [new(Offset16<RoomPixels>.Zero, Lists[8])] },
        { EnemyFrameId.CeresSteamPose10, [new(Offset16<RoomPixels>.Zero, Lists[9])] },
        { EnemyFrameId.CeresSteamPose11, [new(Offset16<RoomPixels>.Zero, Lists[10])] },
        { EnemyFrameId.CeresSteamPose12, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose13, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose14, [new(Offset16<RoomPixels>.Zero, Lists[11])] },
        { EnemyFrameId.CeresSteamPose15, [new(Offset16<RoomPixels>.Zero, Lists[12])] },
        { EnemyFrameId.CeresSteamPose16, [new(Offset16<RoomPixels>.Zero, Lists[13])] },
        { EnemyFrameId.CeresSteamPose17, [new(Offset16<RoomPixels>.Zero, Lists[14])] },
        { EnemyFrameId.CeresSteamPose18, [new(Offset16<RoomPixels>.Zero, Lists[15])] },
        { EnemyFrameId.CeresSteamPose19, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose20, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose21, [new(Offset16<RoomPixels>.Zero, Lists[16])] },
        { EnemyFrameId.CeresSteamPose22, [new(Offset16<RoomPixels>.Zero, Lists[17])] },
        { EnemyFrameId.CeresSteamPose23, [new(Offset16<RoomPixels>.Zero, Lists[18])] },
        { EnemyFrameId.CeresSteamPose24, [new(Offset16<RoomPixels>.Zero, Lists[19])] },
        { EnemyFrameId.CeresSteamPose25, [new(Offset16<RoomPixels>.Zero, Lists[20])] },
        { EnemyFrameId.CeresSteamPose26, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
        { EnemyFrameId.CeresSteamPose27, [new(Offset16<RoomPixels>.Zero, Lists[0])] },
    };

}
