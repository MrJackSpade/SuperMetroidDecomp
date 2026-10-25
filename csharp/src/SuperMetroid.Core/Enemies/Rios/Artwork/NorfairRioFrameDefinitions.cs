using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="NorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NorfairRioFrameDefinitions
{

    /// <summary>The installed frame each <see cref="NorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<NorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NorfairRioPose0 },
        { new(Pose: 1), EnemyFrameId.NorfairRioPose1 },
        { new(Pose: 10), EnemyFrameId.NorfairRioPose10 },
        { new(Pose: 11), EnemyFrameId.NorfairRioPose11 },
        { new(Pose: 12), EnemyFrameId.NorfairRioPose12 },
        { new(Pose: 13), EnemyFrameId.NorfairRioPose13 },
        { new(Pose: 14), EnemyFrameId.NorfairRioPose14 },
        { new(Pose: 15), EnemyFrameId.NorfairRioPose15 },
        { new(Pose: 16), EnemyFrameId.NorfairRioPose16 },
        { new(Pose: 17), EnemyFrameId.NorfairRioPose17 },
        { new(Pose: 18), EnemyFrameId.NorfairRioPose18 },
        { new(Pose: 19), EnemyFrameId.NorfairRioPose19 },
        { new(Pose: 2), EnemyFrameId.NorfairRioPose2 },
        { new(Pose: 3), EnemyFrameId.NorfairRioPose3 },
        { new(Pose: 4), EnemyFrameId.NorfairRioPose4 },
        { new(Pose: 5), EnemyFrameId.NorfairRioPose5 },
        { new(Pose: 6), EnemyFrameId.NorfairRioPose6 },
        { new(Pose: 7), EnemyFrameId.NorfairRioPose7 },
        { new(Pose: 8), EnemyFrameId.NorfairRioPose8 },
        { new(Pose: 9), EnemyFrameId.NorfairRioPose9 },
    };
}
