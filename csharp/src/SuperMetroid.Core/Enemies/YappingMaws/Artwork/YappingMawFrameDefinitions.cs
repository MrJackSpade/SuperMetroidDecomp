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

    /// <summary>The installed frame each <see cref="YappingMawFrame"/> shows.</summary>
    internal static EnemyFrameTable<YappingMawFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.YappingMawA8A8ee },
        { new(Pose: 1), EnemyFrameId.YappingMawA8A909 },
        { new(Pose: 10), EnemyFrameId.YappingMawA8A9ca },
        { new(Pose: 11), EnemyFrameId.YappingMawA8A9e5 },
        { new(Pose: 12), EnemyFrameId.YappingMawA8A9f6 },
        { new(Pose: 13), EnemyFrameId.YappingMawA8Aa11 },
        { new(Pose: 14), EnemyFrameId.YappingMawA8Aa22 },
        { new(Pose: 15), EnemyFrameId.YappingMawA8Aa3d },
        { new(Pose: 16), EnemyFrameId.YappingMawA8Aa4e },
        { new(Pose: 17), EnemyFrameId.YappingMawA8Aa69 },
        { new(Pose: 18), EnemyFrameId.YappingMawA8Aa7a },
        { new(Pose: 19), EnemyFrameId.YappingMawA8Aa95 },
        { new(Pose: 2), EnemyFrameId.YappingMawA8A91a },
        { new(Pose: 20), EnemyFrameId.YappingMawA8Aaa6 },
        { new(Pose: 21), EnemyFrameId.YappingMawA8Aac1 },
        { new(Pose: 22), EnemyFrameId.YappingMawA8Aad2 },
        { new(Pose: 23), EnemyFrameId.YappingMawA8Aaed },
        { new(Pose: 3), EnemyFrameId.YappingMawA8A935 },
        { new(Pose: 4), EnemyFrameId.YappingMawA8A946 },
        { new(Pose: 5), EnemyFrameId.YappingMawA8A961 },
        { new(Pose: 6), EnemyFrameId.YappingMawA8A972 },
        { new(Pose: 7), EnemyFrameId.YappingMawA8A98d },
        { new(Pose: 8), EnemyFrameId.YappingMawA8A99e },
        { new(Pose: 9), EnemyFrameId.YappingMawA8A9b9 },
    };
}
