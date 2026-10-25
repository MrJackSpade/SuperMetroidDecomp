using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The presentation boundary of the Chozo statue's programs (#641): the bank-$AA spritemap each
/// <see cref="ChozoStatueFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ChozoStatueFrameDefinitions
{

    /// <summary>The installed frame each <see cref="ChozoStatueFrame"/> shows.</summary>
    internal static EnemyFrameTable<ChozoStatueFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ChozoStatuePose0 },
        { new(Pose: 1), EnemyFrameId.ChozoStatuePose1 },
        { new(Pose: 10), EnemyFrameId.ChozoStatuePose10 },
        { new(Pose: 11), EnemyFrameId.ChozoStatuePose11 },
        { new(Pose: 12), EnemyFrameId.ChozoStatuePose12 },
        { new(Pose: 13), EnemyFrameId.ChozoStatuePose13 },
        { new(Pose: 14), EnemyFrameId.ChozoStatuePose14 },
        { new(Pose: 15), EnemyFrameId.ChozoStatuePose15 },
        { new(Pose: 16), EnemyFrameId.ChozoStatuePose16 },
        { new(Pose: 17), EnemyFrameId.ChozoStatuePose17 },
        { new(Pose: 18), EnemyFrameId.ChozoStatuePose18 },
        { new(Pose: 19), EnemyFrameId.ChozoStatuePose19 },
        { new(Pose: 2), EnemyFrameId.ChozoStatuePose2 },
        { new(Pose: 20), EnemyFrameId.ChozoStatuePose20 },
        { new(Pose: 21), EnemyFrameId.ChozoStatuePose21 },
        { new(Pose: 22), EnemyFrameId.ChozoStatuePose22 },
        { new(Pose: 23), EnemyFrameId.ChozoStatuePose23 },
        { new(Pose: 24), EnemyFrameId.ChozoStatuePose24 },
        { new(Pose: 25), EnemyFrameId.ChozoStatuePose25 },
        { new(Pose: 3), EnemyFrameId.ChozoStatuePose3 },
        { new(Pose: 4), EnemyFrameId.ChozoStatuePose4 },
        { new(Pose: 5), EnemyFrameId.ChozoStatuePose5 },
        { new(Pose: 6), EnemyFrameId.ChozoStatuePose6 },
        { new(Pose: 7), EnemyFrameId.ChozoStatuePose7 },
        { new(Pose: 8), EnemyFrameId.ChozoStatuePose8 },
        { new(Pose: 9), EnemyFrameId.ChozoStatuePose9 },
    };
}
