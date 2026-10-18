using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.PipeBugs.Artwork;

/// <summary>
/// The presentation boundary of the Pipe Bugs' programs (#641): the bank-$B3 spritemap each
/// <see cref="PipeBugFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PipeBugFrameDefinitions
{

    /// <summary>The installed frame each <see cref="PipeBugFrame"/> shows.</summary>
    internal static EnemyFrameTable<PipeBugFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.PipeBrinstarNormalLeft0 },
        { new(Pose: 1), EnemyFrameId.PipeBrinstarNormalLeft1 },
        { new(Pose: 10), EnemyFrameId.PipeBrinstarStrongShootLeft0 },
        { new(Pose: 11), EnemyFrameId.PipeBrinstarStrongShootLeft1 },
        { new(Pose: 12), EnemyFrameId.PipeBrinstarStrongShootLeft2 },
        { new(Pose: 13), EnemyFrameId.PipeBrinstarStrongRiseLeft0 },
        { new(Pose: 14), EnemyFrameId.PipeBrinstarStrongRiseLeft1 },
        { new(Pose: 15), EnemyFrameId.PipeBrinstarStrongRiseLeft2 },
        { new(Pose: 16), EnemyFrameId.PipeBrinstarStrongShootRight0 },
        { new(Pose: 17), EnemyFrameId.PipeBrinstarStrongShootRight1 },
        { new(Pose: 18), EnemyFrameId.PipeBrinstarStrongShootRight2 },
        { new(Pose: 19), EnemyFrameId.PipeBrinstarStrongRiseRight0 },
        { new(Pose: 2), EnemyFrameId.PipeBrinstarNormalLeft2 },
        { new(Pose: 20), EnemyFrameId.PipeBrinstarStrongRiseRight1 },
        { new(Pose: 21), EnemyFrameId.PipeBrinstarStrongRiseRight2 },
        { new(Pose: 22), EnemyFrameId.PipeNorfairLeft0 },
        { new(Pose: 23), EnemyFrameId.PipeNorfairLeft1 },
        { new(Pose: 24), EnemyFrameId.PipeNorfairLeft2 },
        { new(Pose: 25), EnemyFrameId.PipeNorfairLeft3 },
        { new(Pose: 26), EnemyFrameId.PipeNorfairLeft4 },
        { new(Pose: 27), EnemyFrameId.PipeNorfairRight0 },
        { new(Pose: 28), EnemyFrameId.PipeNorfairRight1 },
        { new(Pose: 29), EnemyFrameId.PipeNorfairRight2 },
        { new(Pose: 3), EnemyFrameId.PipeBrinstarNormalLeft3 },
        { new(Pose: 30), EnemyFrameId.PipeNorfairRight3 },
        { new(Pose: 31), EnemyFrameId.PipeNorfairRight4 },
        { new(Pose: 32), EnemyFrameId.PipeYellowFlyLeft0 },
        { new(Pose: 33), EnemyFrameId.PipeYellowFlyLeft1 },
        { new(Pose: 34), EnemyFrameId.PipeYellowFlyLeft2 },
        { new(Pose: 35), EnemyFrameId.PipeYellowArcLeft0 },
        { new(Pose: 36), EnemyFrameId.PipeYellowArcLeft1 },
        { new(Pose: 37), EnemyFrameId.PipeYellowArcLeft2 },
        { new(Pose: 38), EnemyFrameId.PipeYellowFlyRight0 },
        { new(Pose: 39), EnemyFrameId.PipeYellowFlyRight1 },
        { new(Pose: 4), EnemyFrameId.PipeBrinstarNormalLeft4 },
        { new(Pose: 40), EnemyFrameId.PipeYellowFlyRight2 },
        { new(Pose: 41), EnemyFrameId.PipeYellowArcRight0 },
        { new(Pose: 42), EnemyFrameId.PipeYellowArcRight1 },
        { new(Pose: 43), EnemyFrameId.PipeYellowArcRight2 },
        { new(Pose: 5), EnemyFrameId.PipeBrinstarNormalRight0 },
        { new(Pose: 6), EnemyFrameId.PipeBrinstarNormalRight1 },
        { new(Pose: 7), EnemyFrameId.PipeBrinstarNormalRight2 },
        { new(Pose: 8), EnemyFrameId.PipeBrinstarNormalRight3 },
        { new(Pose: 9), EnemyFrameId.PipeBrinstarNormalRight4 },
    };
}
