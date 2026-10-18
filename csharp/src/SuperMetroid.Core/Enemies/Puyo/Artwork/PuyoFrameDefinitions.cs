using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo.Artwork;

/// <summary>
/// The presentation boundary of Puyo's programs (#641): the bank-$A2 spritemap each
/// <see cref="PuyoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class PuyoFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PuyoFrame frame) => frame.Pose switch
    {
        0 => 0x9df6,
        1 => 0x9e02,
        2 => 0x9e0e,
        3 => 0x9e1a,
        4 => 0x9e26,
        5 => 0x9e37,
        6 => 0x9e4d,
        7 => 0x9e5e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Puyo pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(PuyoFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.PuyoGround0,
        { Pose: 1 } => EnemyFrameId.PuyoGround1,
        { Pose: 2 } => EnemyFrameId.PuyoGround2,
        { Pose: 3 } => EnemyFrameId.PuyoAir0,
        { Pose: 4 } => EnemyFrameId.PuyoAir1,
        { Pose: 5 } => EnemyFrameId.PuyoAir2,
        { Pose: 6 } => EnemyFrameId.PuyoAir3,
        { Pose: 7 } => EnemyFrameId.PuyoAir4,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the PuyoFrame programs show."),
    };
}
