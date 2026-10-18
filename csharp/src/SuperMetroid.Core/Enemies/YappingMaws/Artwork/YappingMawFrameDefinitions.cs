using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws.Artwork;

/// <summary>
/// The presentation boundary of the Yapping Maw's programs (#641): the bank-$A8 spritemap each
/// <see cref="YappingMawFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class YappingMawFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(YappingMawFrame frame) => frame.Pose switch
    {
        0 => 0xa8ee,
        1 => 0xa909,
        2 => 0xa91a,
        3 => 0xa935,
        4 => 0xa946,
        5 => 0xa961,
        6 => 0xa972,
        7 => 0xa98d,
        8 => 0xa99e,
        9 => 0xa9b9,
        10 => 0xa9ca,
        11 => 0xa9e5,
        12 => 0xa9f6,
        13 => 0xaa11,
        14 => 0xaa22,
        15 => 0xaa3d,
        16 => 0xaa4e,
        17 => 0xaa69,
        18 => 0xaa7a,
        19 => 0xaa95,
        20 => 0xaaa6,
        21 => 0xaac1,
        22 => 0xaad2,
        23 => 0xaaed,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Yapping Maw pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(YappingMawFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.YappingMawA8A8ee,
        { Pose: 1 } => EnemyFrameId.YappingMawA8A909,
        { Pose: 10 } => EnemyFrameId.YappingMawA8A9ca,
        { Pose: 11 } => EnemyFrameId.YappingMawA8A9e5,
        { Pose: 12 } => EnemyFrameId.YappingMawA8A9f6,
        { Pose: 13 } => EnemyFrameId.YappingMawA8Aa11,
        { Pose: 14 } => EnemyFrameId.YappingMawA8Aa22,
        { Pose: 15 } => EnemyFrameId.YappingMawA8Aa3d,
        { Pose: 16 } => EnemyFrameId.YappingMawA8Aa4e,
        { Pose: 17 } => EnemyFrameId.YappingMawA8Aa69,
        { Pose: 18 } => EnemyFrameId.YappingMawA8Aa7a,
        { Pose: 19 } => EnemyFrameId.YappingMawA8Aa95,
        { Pose: 2 } => EnemyFrameId.YappingMawA8A91a,
        { Pose: 20 } => EnemyFrameId.YappingMawA8Aaa6,
        { Pose: 21 } => EnemyFrameId.YappingMawA8Aac1,
        { Pose: 22 } => EnemyFrameId.YappingMawA8Aad2,
        { Pose: 23 } => EnemyFrameId.YappingMawA8Aaed,
        { Pose: 3 } => EnemyFrameId.YappingMawA8A935,
        { Pose: 4 } => EnemyFrameId.YappingMawA8A946,
        { Pose: 5 } => EnemyFrameId.YappingMawA8A961,
        { Pose: 6 } => EnemyFrameId.YappingMawA8A972,
        { Pose: 7 } => EnemyFrameId.YappingMawA8A98d,
        { Pose: 8 } => EnemyFrameId.YappingMawA8A99e,
        { Pose: 9 } => EnemyFrameId.YappingMawA8A9b9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the YappingMawFrame programs show."),
    };
}
