using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;

/// <summary>
/// The presentation boundary of Oum's programs (#641): the bank-$A2 spritemap each
/// <see cref="MaridiaLargeSnailFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MaridiaLargeSnailFrameDefinitions
{

    /// <summary>The installed frame each <see cref="MaridiaLargeSnailFrame"/> shows.</summary>
    internal static EnemyFrameTable<MaridiaLargeSnailFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.OumPose0 },
        { new(Pose: 1), EnemyFrameId.OumPose1 },
        { new(Pose: 10), EnemyFrameId.OumPose10 },
        { new(Pose: 11), EnemyFrameId.OumPose11 },
        { new(Pose: 12), EnemyFrameId.OumPose12 },
        { new(Pose: 13), EnemyFrameId.OumPose13 },
        { new(Pose: 14), EnemyFrameId.OumPose14 },
        { new(Pose: 15), EnemyFrameId.OumPose15 },
        { new(Pose: 16), EnemyFrameId.OumPose16 },
        { new(Pose: 17), EnemyFrameId.OumPose17 },
        { new(Pose: 18), EnemyFrameId.OumPose18 },
        { new(Pose: 19), EnemyFrameId.OumPose19 },
        { new(Pose: 2), EnemyFrameId.OumPose2 },
        { new(Pose: 20), EnemyFrameId.OumPose20 },
        { new(Pose: 21), EnemyFrameId.OumPose21 },
        { new(Pose: 22), EnemyFrameId.OumPose22 },
        { new(Pose: 23), EnemyFrameId.OumPose23 },
        { new(Pose: 24), EnemyFrameId.OumPose24 },
        { new(Pose: 25), EnemyFrameId.OumPose25 },
        { new(Pose: 26), EnemyFrameId.OumPose26 },
        { new(Pose: 27), EnemyFrameId.OumPose27 },
        { new(Pose: 28), EnemyFrameId.OumPose28 },
        { new(Pose: 29), EnemyFrameId.OumPose29 },
        { new(Pose: 3), EnemyFrameId.OumPose3 },
        { new(Pose: 4), EnemyFrameId.OumPose4 },
        { new(Pose: 5), EnemyFrameId.OumPose5 },
        { new(Pose: 6), EnemyFrameId.OumPose6 },
        { new(Pose: 7), EnemyFrameId.OumPose7 },
        { new(Pose: 8), EnemyFrameId.OumPose8 },
        { new(Pose: 9), EnemyFrameId.OumPose9 },
    };
}
