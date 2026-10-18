using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Atomic.Artwork;

/// <summary>
/// The presentation boundary of Atomic's programs (#641): the bank-$A8 spritemap each
/// <see cref="AtomicFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class AtomicFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(AtomicFrame frame) => frame.Pose switch
    {
        0 => 0xe489,
        1 => 0xe49f,
        2 => 0xe4b5,
        3 => 0xe4cb,
        4 => 0xe4e1,
        5 => 0xe4f2,
        6 => 0xe508,
        7 => 0xe51e,
        8 => 0xe534,
        9 => 0xe54a,
        10 => 0xe560,
        11 => 0xe571,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Atomic pose."),
    };

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
