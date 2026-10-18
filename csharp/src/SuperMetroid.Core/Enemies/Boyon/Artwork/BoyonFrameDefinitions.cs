using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon.Artwork;

/// <summary>
/// The presentation boundary of Boyon's programs (#641): the bank-$A2 spritemap each
/// <see cref="BoyonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BoyonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BoyonFrame frame) => frame.Pose switch
    {
        0 => 0x88da,
        1 => 0x88e1,
        2 => 0x88e8,
        3 => 0x88ef,
        4 => 0x88f6,
        5 => 0x88fd,
        6 => 0x8904,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Boyon pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(BoyonFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.BoyonIdle0,
        { Pose: 1 } => EnemyFrameId.BoyonIdle1,
        { Pose: 2 } => EnemyFrameId.BoyonIdle2,
        { Pose: 3 } => EnemyFrameId.BoyonBounce0,
        { Pose: 4 } => EnemyFrameId.BoyonBounce1,
        { Pose: 5 } => EnemyFrameId.BoyonBounce2,
        { Pose: 6 } => EnemyFrameId.BoyonBounce3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the BoyonFrame programs show."),
    };
}
