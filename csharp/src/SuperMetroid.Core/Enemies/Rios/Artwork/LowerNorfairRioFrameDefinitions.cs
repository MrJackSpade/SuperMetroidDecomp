using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Lower Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="LowerNorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class LowerNorfairRioFrameDefinitions
{

    /// <summary>The installed frame each <see cref="LowerNorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<LowerNorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.LowerNorfairRioPose0 },
        { new(Pose: 1), EnemyFrameId.LowerNorfairRioPose1 },
        { new(Pose: 10), EnemyFrameId.LowerNorfairRioPose10 },
        { new(Pose: 11), EnemyFrameId.LowerNorfairRioPose11 },
        { new(Pose: 12), EnemyFrameId.LowerNorfairRioPose12 },
        { new(Pose: 13), EnemyFrameId.LowerNorfairRioPose13 },
        { new(Pose: 14), EnemyFrameId.LowerNorfairRioPose14 },
        { new(Pose: 15), EnemyFrameId.LowerNorfairRioPose15 },
        { new(Pose: 16), EnemyFrameId.LowerNorfairRioPose16 },
        { new(Pose: 17), EnemyFrameId.LowerNorfairRioPose17 },
        { new(Pose: 2), EnemyFrameId.LowerNorfairRioPose2 },
        { new(Pose: 3), EnemyFrameId.LowerNorfairRioPose3 },
        { new(Pose: 4), EnemyFrameId.LowerNorfairRioPose4 },
        { new(Pose: 5), EnemyFrameId.LowerNorfairRioPose5 },
        { new(Pose: 6), EnemyFrameId.LowerNorfairRioPose6 },
        { new(Pose: 7), EnemyFrameId.LowerNorfairRioPose7 },
        { new(Pose: 8), EnemyFrameId.LowerNorfairRioPose8 },
        { new(Pose: 9), EnemyFrameId.LowerNorfairRioPose9 },
    };
}
