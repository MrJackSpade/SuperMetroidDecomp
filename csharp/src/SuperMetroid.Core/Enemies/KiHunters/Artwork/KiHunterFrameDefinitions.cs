using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters.Artwork;

/// <summary>
/// The presentation boundary of the Ki-Hunter's programs (#641): the bank-$A8 spritemap each
/// <see cref="KiHunterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KiHunterFrameDefinitions
{

    /// <summary>The installed frame each <see cref="KiHunterFrame"/> shows.</summary>
    internal static EnemyFrameTable<KiHunterFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.KiHunterPose0 },
        { new(Pose: 1), EnemyFrameId.KiHunterPose1 },
        { new(Pose: 10), EnemyFrameId.KiHunterPose10 },
        { new(Pose: 11), EnemyFrameId.KiHunterPose11 },
        { new(Pose: 12), EnemyFrameId.KiHunterPose12 },
        { new(Pose: 13), EnemyFrameId.KiHunterPose13 },
        { new(Pose: 14), EnemyFrameId.KiHunterPose14 },
        { new(Pose: 15), EnemyFrameId.KiHunterPose15 },
        { new(Pose: 16), EnemyFrameId.KiHunterPose16 },
        { new(Pose: 17), EnemyFrameId.KiHunterPose17 },
        { new(Pose: 18), EnemyFrameId.KiHunterPose18 },
        { new(Pose: 19), EnemyFrameId.KiHunterPose19 },
        { new(Pose: 2), EnemyFrameId.KiHunterPose2 },
        { new(Pose: 20), EnemyFrameId.KiHunterPose20 },
        { new(Pose: 21), EnemyFrameId.KiHunterPose21 },
        { new(Pose: 22), EnemyFrameId.KiHunterPose22 },
        { new(Pose: 23), EnemyFrameId.KiHunterPose23 },
        { new(Pose: 24), EnemyFrameId.KiHunterPose24 },
        { new(Pose: 25), EnemyFrameId.KiHunterPose25 },
        { new(Pose: 26), EnemyFrameId.KiHunterPose26 },
        { new(Pose: 27), EnemyFrameId.KiHunterPose27 },
        { new(Pose: 28), EnemyFrameId.KiHunterPose28 },
        { new(Pose: 29), EnemyFrameId.KiHunterPose29 },
        { new(Pose: 3), EnemyFrameId.KiHunterPose3 },
        { new(Pose: 30), EnemyFrameId.KiHunterPose30 },
        { new(Pose: 31), EnemyFrameId.KiHunterPose31 },
        { new(Pose: 32), EnemyFrameId.KiHunterPose32 },
        { new(Pose: 33), EnemyFrameId.KiHunterPose33 },
        { new(Pose: 34), EnemyFrameId.KiHunterPose34 },
        { new(Pose: 35), EnemyFrameId.KiHunterPose35 },
        { new(Pose: 36), EnemyFrameId.KiHunterPose36 },
        { new(Pose: 37), EnemyFrameId.KiHunterPose37 },
        { new(Pose: 38), EnemyFrameId.KiHunterPose38 },
        { new(Pose: 39), EnemyFrameId.KiHunterPose39 },
        { new(Pose: 4), EnemyFrameId.KiHunterPose4 },
        { new(Pose: 40), EnemyFrameId.KiHunterPose40 },
        { new(Pose: 5), EnemyFrameId.KiHunterPose5 },
        { new(Pose: 6), EnemyFrameId.KiHunterPose6 },
        { new(Pose: 7), EnemyFrameId.KiHunterPose7 },
        { new(Pose: 8), EnemyFrameId.KiHunterPose8 },
        { new(Pose: 9), EnemyFrameId.KiHunterPose9 },
    };
}
