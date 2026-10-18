using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac.Artwork;

/// <summary>
/// The presentation boundary of Cacatac's programs (#641): the bank-$A2 spritemap each
/// <see cref="CacatacFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CacatacFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CacatacFrame frame) => frame.Pose switch
    {
        0 => 0xa0bb,
        1 => 0xa0db,
        2 => 0xa0fb,
        3 => 0xa11b,
        4 => 0xa13b,
        5 => 0xa15b,
        6 => 0xa17b,
        7 => 0xa19b,
        8 => 0xa1bb,
        9 => 0xa1ef,
        10 => 0xa223,
        11 => 0xa243,
        12 => 0xa263,
        13 => 0xa283,
        14 => 0xa2a3,
        15 => 0xa2c3,
        16 => 0xa2e3,
        17 => 0xa303,
        18 => 0xa323,
        19 => 0xa357,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Cacatac pose."),
    };

    /// <summary>The installed frame each <see cref="CacatacFrame"/> shows.</summary>
    internal static EnemyFrameTable<CacatacFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.CacatacUprightIdle0 },
        { new(Pose: 1), EnemyFrameId.CacatacUprightIdle1 },
        { new(Pose: 10), EnemyFrameId.CacatacInvertedIdle0 },
        { new(Pose: 11), EnemyFrameId.CacatacInvertedIdle1 },
        { new(Pose: 12), EnemyFrameId.CacatacInvertedIdle2 },
        { new(Pose: 13), EnemyFrameId.CacatacInvertedIdle3 },
        { new(Pose: 14), EnemyFrameId.CacatacInvertedIdle4 },
        { new(Pose: 15), EnemyFrameId.CacatacInvertedIdle5 },
        { new(Pose: 16), EnemyFrameId.CacatacInvertedIdle6 },
        { new(Pose: 17), EnemyFrameId.CacatacInvertedIdle7 },
        { new(Pose: 18), EnemyFrameId.CacatacInvertedAttack1 },
        { new(Pose: 19), EnemyFrameId.CacatacInvertedAttack2 },
        { new(Pose: 2), EnemyFrameId.CacatacUprightIdle2 },
        { new(Pose: 3), EnemyFrameId.CacatacUprightIdle3 },
        { new(Pose: 4), EnemyFrameId.CacatacUprightIdle4 },
        { new(Pose: 5), EnemyFrameId.CacatacUprightIdle5 },
        { new(Pose: 6), EnemyFrameId.CacatacUprightIdle6 },
        { new(Pose: 7), EnemyFrameId.CacatacUprightIdle7 },
        { new(Pose: 8), EnemyFrameId.CacatacUprightAttack1 },
        { new(Pose: 9), EnemyFrameId.CacatacUprightAttack2 },
    };
}
