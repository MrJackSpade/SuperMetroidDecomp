using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo.Artwork;

/// <summary>
/// The presentation boundary of the Chozo statue's programs (#641): the bank-$AA spritemap each
/// <see cref="ChozoStatueFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ChozoStatueFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ChozoStatueFrame frame) => frame.Pose switch
    {
        0 => 0xe7dd,
        1 => 0xe839,
        2 => 0xe890,
        3 => 0xe8e7,
        4 => 0xe943,
        5 => 0xe9ae,
        6 => 0xea1e,
        7 => 0xea8e,
        8 => 0xeafe,
        9 => 0xeb69,
        10 => 0xebd9,
        11 => 0xec49,
        12 => 0xecb9,
        13 => 0xed15,
        14 => 0xed71,
        15 => 0xedcd,
        16 => 0xee29,
        17 => 0xefd8,
        18 => 0xf034,
        19 => 0xf08b,
        20 => 0xf0e2,
        21 => 0xf4b4,
        22 => 0xf510,
        23 => 0xf56c,
        24 => 0xf5c8,
        25 => 0xf624,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Chozo statue pose."),
    };

    /// <summary>The installed frame each <see cref="ChozoStatueFrame"/> shows.</summary>
    internal static EnemyFrameTable<ChozoStatueFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.ChozoStatueAaE7dd },
        { new(Pose: 1), EnemyFrameId.ChozoStatueAaE839 },
        { new(Pose: 10), EnemyFrameId.ChozoStatueAaEbd9 },
        { new(Pose: 11), EnemyFrameId.ChozoStatueAaEc49 },
        { new(Pose: 12), EnemyFrameId.ChozoStatueAaEcb9 },
        { new(Pose: 13), EnemyFrameId.ChozoStatueAaEd15 },
        { new(Pose: 14), EnemyFrameId.ChozoStatueAaEd71 },
        { new(Pose: 15), EnemyFrameId.ChozoStatueAaEdcd },
        { new(Pose: 16), EnemyFrameId.ChozoStatueAaEe29 },
        { new(Pose: 17), EnemyFrameId.ChozoStatueAaEfd8 },
        { new(Pose: 18), EnemyFrameId.ChozoStatueAaF034 },
        { new(Pose: 19), EnemyFrameId.ChozoStatueAaF08b },
        { new(Pose: 2), EnemyFrameId.ChozoStatueAaE890 },
        { new(Pose: 20), EnemyFrameId.ChozoStatueAaF0e2 },
        { new(Pose: 21), EnemyFrameId.ChozoStatueAaF4b4 },
        { new(Pose: 22), EnemyFrameId.ChozoStatueAaF510 },
        { new(Pose: 23), EnemyFrameId.ChozoStatueAaF56c },
        { new(Pose: 24), EnemyFrameId.ChozoStatueAaF5c8 },
        { new(Pose: 25), EnemyFrameId.ChozoStatueAaF624 },
        { new(Pose: 3), EnemyFrameId.ChozoStatueAaE8e7 },
        { new(Pose: 4), EnemyFrameId.ChozoStatueAaE943 },
        { new(Pose: 5), EnemyFrameId.ChozoStatueAaE9ae },
        { new(Pose: 6), EnemyFrameId.ChozoStatueAaEa1e },
        { new(Pose: 7), EnemyFrameId.ChozoStatueAaEa8e },
        { new(Pose: 8), EnemyFrameId.ChozoStatueAaEafe },
        { new(Pose: 9), EnemyFrameId.ChozoStatueAaEb69 },
    };
}
