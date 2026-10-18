using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Zoa.Artwork;

/// <summary>
/// The presentation boundary of Zoa's programs (#641): the bank-$A3 spritemap each
/// <see cref="ZoaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ZoaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ZoaFrame frame) => frame.Pose switch
    {
        0 => 0xb55f,
        1 => 0xb566,
        2 => 0xb56d,
        3 => 0xb574,
        4 => 0xb57b,
        5 => 0xb582,
        6 => 0xb589,
        7 => 0xb590,
        8 => 0xb597,
        9 => 0xb59e,
        10 => 0xb5a5,
        11 => 0xb5ac,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Zoa pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(ZoaFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.ZoaShootLeft0,
        { Pose: 1 } => EnemyFrameId.ZoaShootLeft1,
        { Pose: 10 } => EnemyFrameId.ZoaRiseRight0,
        { Pose: 11 } => EnemyFrameId.ZoaRiseRight2,
        { Pose: 2 } => EnemyFrameId.ZoaShootLeft2,
        { Pose: 3 } => EnemyFrameId.ZoaRiseLeft1,
        { Pose: 4 } => EnemyFrameId.ZoaRiseLeft0,
        { Pose: 5 } => EnemyFrameId.ZoaRiseLeft2,
        { Pose: 6 } => EnemyFrameId.ZoaShootRight0,
        { Pose: 7 } => EnemyFrameId.ZoaShootRight1,
        { Pose: 8 } => EnemyFrameId.ZoaShootRight2,
        { Pose: 9 } => EnemyFrameId.ZoaRiseRight1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the ZoaFrame programs show."),
    };
}
