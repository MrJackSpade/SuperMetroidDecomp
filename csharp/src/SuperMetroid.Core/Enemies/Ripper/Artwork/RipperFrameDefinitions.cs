using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper.Artwork;

/// <summary>
/// The presentation boundary of the Ripper family's programs (#641): the bank-$A2 spritemap each
/// <see cref="RipperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RipperFrameDefinitions
{

    /// <summary>The installed frame each <see cref="RipperFrame"/> shows.</summary>
    internal static EnemyFrameTable<RipperFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.RipperSharedLeft0 },
        { new(Pose: 1), EnemyFrameId.RipperSharedLeft1 },
        { new(Pose: 10), EnemyFrameId.RipperRight1 },
        { new(Pose: 11), EnemyFrameId.RipperRight2 },
        { new(Pose: 2), EnemyFrameId.RipperSharedLeft2 },
        { new(Pose: 3), EnemyFrameId.RipperSharedRight0 },
        { new(Pose: 4), EnemyFrameId.RipperSharedRight1 },
        { new(Pose: 5), EnemyFrameId.RipperSharedRight2 },
        { new(Pose: 6), EnemyFrameId.RipperLeft0 },
        { new(Pose: 7), EnemyFrameId.RipperLeft1 },
        { new(Pose: 8), EnemyFrameId.RipperLeft2 },
        { new(Pose: 9), EnemyFrameId.RipperRight0 },
    };
}
