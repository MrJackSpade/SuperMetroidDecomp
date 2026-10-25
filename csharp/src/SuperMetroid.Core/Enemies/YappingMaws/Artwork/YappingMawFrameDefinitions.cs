using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws.Artwork;

/// <summary>
/// The presentation boundary of the Yapping Maw's programs (#641): the bank-$A8 spritemap each
/// <see cref="YappingMawFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class YappingMawFrameDefinitions
{

    /// <summary>The installed frame each <see cref="YappingMawFrame"/> shows.</summary>
    internal static EnemyFrameTable<YappingMawFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.YappingMawPose0 },
        { new(Pose: 1), EnemyFrameId.YappingMawPose1 },
        { new(Pose: 10), EnemyFrameId.YappingMawPose10 },
        { new(Pose: 11), EnemyFrameId.YappingMawPose11 },
        { new(Pose: 12), EnemyFrameId.YappingMawPose12 },
        { new(Pose: 13), EnemyFrameId.YappingMawPose13 },
        { new(Pose: 14), EnemyFrameId.YappingMawPose14 },
        { new(Pose: 15), EnemyFrameId.YappingMawPose15 },
        { new(Pose: 16), EnemyFrameId.YappingMawPose16 },
        { new(Pose: 17), EnemyFrameId.YappingMawPose17 },
        { new(Pose: 18), EnemyFrameId.YappingMawPose18 },
        { new(Pose: 19), EnemyFrameId.YappingMawPose19 },
        { new(Pose: 2), EnemyFrameId.YappingMawPose2 },
        { new(Pose: 20), EnemyFrameId.YappingMawPose20 },
        { new(Pose: 21), EnemyFrameId.YappingMawPose21 },
        { new(Pose: 22), EnemyFrameId.YappingMawPose22 },
        { new(Pose: 23), EnemyFrameId.YappingMawPose23 },
        { new(Pose: 3), EnemyFrameId.YappingMawPose3 },
        { new(Pose: 4), EnemyFrameId.YappingMawPose4 },
        { new(Pose: 5), EnemyFrameId.YappingMawPose5 },
        { new(Pose: 6), EnemyFrameId.YappingMawPose6 },
        { new(Pose: 7), EnemyFrameId.YappingMawPose7 },
        { new(Pose: 8), EnemyFrameId.YappingMawPose8 },
        { new(Pose: 9), EnemyFrameId.YappingMawPose9 },
    };
}
