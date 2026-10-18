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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(ChozoStatueFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.ChozoStatueAaE7dd,
        { Pose: 1 } => EnemyFrameId.ChozoStatueAaE839,
        { Pose: 10 } => EnemyFrameId.ChozoStatueAaEbd9,
        { Pose: 11 } => EnemyFrameId.ChozoStatueAaEc49,
        { Pose: 12 } => EnemyFrameId.ChozoStatueAaEcb9,
        { Pose: 13 } => EnemyFrameId.ChozoStatueAaEd15,
        { Pose: 14 } => EnemyFrameId.ChozoStatueAaEd71,
        { Pose: 15 } => EnemyFrameId.ChozoStatueAaEdcd,
        { Pose: 16 } => EnemyFrameId.ChozoStatueAaEe29,
        { Pose: 17 } => EnemyFrameId.ChozoStatueAaEfd8,
        { Pose: 18 } => EnemyFrameId.ChozoStatueAaF034,
        { Pose: 19 } => EnemyFrameId.ChozoStatueAaF08b,
        { Pose: 2 } => EnemyFrameId.ChozoStatueAaE890,
        { Pose: 20 } => EnemyFrameId.ChozoStatueAaF0e2,
        { Pose: 21 } => EnemyFrameId.ChozoStatueAaF4b4,
        { Pose: 22 } => EnemyFrameId.ChozoStatueAaF510,
        { Pose: 23 } => EnemyFrameId.ChozoStatueAaF56c,
        { Pose: 24 } => EnemyFrameId.ChozoStatueAaF5c8,
        { Pose: 25 } => EnemyFrameId.ChozoStatueAaF624,
        { Pose: 3 } => EnemyFrameId.ChozoStatueAaE8e7,
        { Pose: 4 } => EnemyFrameId.ChozoStatueAaE943,
        { Pose: 5 } => EnemyFrameId.ChozoStatueAaE9ae,
        { Pose: 6 } => EnemyFrameId.ChozoStatueAaEa1e,
        { Pose: 7 } => EnemyFrameId.ChozoStatueAaEa8e,
        { Pose: 8 } => EnemyFrameId.ChozoStatueAaEafe,
        { Pose: 9 } => EnemyFrameId.ChozoStatueAaEb69,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the ChozoStatueFrame programs show."),
    };
}
