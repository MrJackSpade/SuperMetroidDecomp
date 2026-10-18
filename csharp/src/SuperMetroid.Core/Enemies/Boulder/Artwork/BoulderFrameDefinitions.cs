using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boulder.Artwork;

/// <summary>
/// The presentation boundary of the Boulder's programs (#641): the bank-$A6 spritemap each
/// <see cref="BoulderFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BoulderFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BoulderFrame frame) => frame.Pose switch
    {
        0 => 0x8a59,
        1 => 0x8a6f,
        2 => 0x8a85,
        3 => 0x8a9b,
        4 => 0x8ab1,
        5 => 0x8ac7,
        6 => 0x8add,
        7 => 0x8af3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Boulder pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(BoulderFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.BoulderRoll0,
        { Pose: 1 } => EnemyFrameId.BoulderRoll1,
        { Pose: 2 } => EnemyFrameId.BoulderRoll2,
        { Pose: 3 } => EnemyFrameId.BoulderRoll3,
        { Pose: 4 } => EnemyFrameId.BoulderRoll4,
        { Pose: 5 } => EnemyFrameId.BoulderRoll5,
        { Pose: 6 } => EnemyFrameId.BoulderRoll6,
        { Pose: 7 } => EnemyFrameId.BoulderRoll7,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the BoulderFrame programs show."),
    };
}
