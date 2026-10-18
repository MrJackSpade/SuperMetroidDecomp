using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skree.Artwork;

/// <summary>
/// The presentation boundary of the Skree's and Metaree's programs (#641): the bank-$A3 spritemap each
/// <see cref="SkreeMetareeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SkreeMetareeFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SkreeMetareeFrame frame) => frame.Pose switch
    {
        0 => 0x8b65,
        1 => 0x8b94,
        2 => 0x8bb9,
        3 => 0x8bde,
        4 => 0x8bea,
        5 => 0xc842,
        6 => 0xc862,
        7 => 0xc878,
        8 => 0xc884,
        9 => 0xc89a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Skree or Metaree pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(SkreeMetareeFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MetareeIdle0,
        { Pose: 1 } => EnemyFrameId.MetareePrepare1,
        { Pose: 2 } => EnemyFrameId.MetareeIdle1,
        { Pose: 3 } => EnemyFrameId.MetareeIdle2,
        { Pose: 4 } => EnemyFrameId.MetareeIdle3,
        { Pose: 5 } => EnemyFrameId.SkreeIdle0,
        { Pose: 6 } => EnemyFrameId.SkreePrepare1,
        { Pose: 7 } => EnemyFrameId.SkreeIdle1,
        { Pose: 8 } => EnemyFrameId.SkreeIdle2,
        { Pose: 9 } => EnemyFrameId.SkreeIdle3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the SkreeMetareeFrame programs show."),
    };
}
