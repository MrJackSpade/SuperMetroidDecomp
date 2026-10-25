using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres steam vents' programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresSteamFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresSteamFrameDefinitions
{

    /// <summary>The installed frame each <see cref="CeresSteamFrame"/> shows.</summary>
    internal static EnemyFrameTable<CeresSteamFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.CeresSteamPose0 },
        { new(Pose: 1), EnemyFrameId.CeresSteamPose1 },
        { new(Pose: 10), EnemyFrameId.CeresSteamPose10 },
        { new(Pose: 11), EnemyFrameId.CeresSteamPose11 },
        { new(Pose: 12), EnemyFrameId.CeresSteamPose12 },
        { new(Pose: 13), EnemyFrameId.CeresSteamPose13 },
        { new(Pose: 14), EnemyFrameId.CeresSteamPose14 },
        { new(Pose: 15), EnemyFrameId.CeresSteamPose15 },
        { new(Pose: 16), EnemyFrameId.CeresSteamPose16 },
        { new(Pose: 17), EnemyFrameId.CeresSteamPose17 },
        { new(Pose: 18), EnemyFrameId.CeresSteamPose18 },
        { new(Pose: 19), EnemyFrameId.CeresSteamPose19 },
        { new(Pose: 2), EnemyFrameId.CeresSteamPose2 },
        { new(Pose: 20), EnemyFrameId.CeresSteamPose20 },
        { new(Pose: 21), EnemyFrameId.CeresSteamPose21 },
        { new(Pose: 22), EnemyFrameId.CeresSteamPose22 },
        { new(Pose: 23), EnemyFrameId.CeresSteamPose23 },
        { new(Pose: 24), EnemyFrameId.CeresSteamPose24 },
        { new(Pose: 25), EnemyFrameId.CeresSteamPose25 },
        { new(Pose: 26), EnemyFrameId.CeresSteamPose26 },
        { new(Pose: 27), EnemyFrameId.CeresSteamPose27 },
        { new(Pose: 3), EnemyFrameId.CeresSteamPose3 },
        { new(Pose: 4), EnemyFrameId.CeresSteamPose4 },
        { new(Pose: 5), EnemyFrameId.CeresSteamPose5 },
        { new(Pose: 6), EnemyFrameId.CeresSteamPose6 },
        { new(Pose: 7), EnemyFrameId.CeresSteamPose7 },
        { new(Pose: 8), EnemyFrameId.CeresSteamPose8 },
        { new(Pose: 9), EnemyFrameId.CeresSteamPose9 },
    };
}
