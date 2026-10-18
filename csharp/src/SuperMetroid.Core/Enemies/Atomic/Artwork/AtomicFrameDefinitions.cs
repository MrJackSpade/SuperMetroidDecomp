using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Atomic.Artwork;

/// <summary>
/// The presentation boundary of Atomic's programs (#641): the bank-$A8 spritemap each
/// <see cref="AtomicFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class AtomicFrameDefinitions
{

    /// <summary>The installed frame each <see cref="AtomicFrame"/> shows.</summary>
    internal static EnemyFrameTable<AtomicFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.AtomicUpRight0 },
        { new(Pose: 1), EnemyFrameId.AtomicUpRight1 },
        { new(Pose: 10), EnemyFrameId.AtomicUpLeft4 },
        { new(Pose: 11), EnemyFrameId.AtomicUpLeft5 },
        { new(Pose: 2), EnemyFrameId.AtomicUpRight2 },
        { new(Pose: 3), EnemyFrameId.AtomicUpRight3 },
        { new(Pose: 4), EnemyFrameId.AtomicUpRight4 },
        { new(Pose: 5), EnemyFrameId.AtomicUpRight5 },
        { new(Pose: 6), EnemyFrameId.AtomicUpLeft0 },
        { new(Pose: 7), EnemyFrameId.AtomicUpLeft1 },
        { new(Pose: 8), EnemyFrameId.AtomicUpLeft2 },
        { new(Pose: 9), EnemyFrameId.AtomicUpLeft3 },
    };
}
