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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(CacatacFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.CacatacUprightIdle0,
        { Pose: 1 } => EnemyFrameId.CacatacUprightIdle1,
        { Pose: 10 } => EnemyFrameId.CacatacInvertedIdle0,
        { Pose: 11 } => EnemyFrameId.CacatacInvertedIdle1,
        { Pose: 12 } => EnemyFrameId.CacatacInvertedIdle2,
        { Pose: 13 } => EnemyFrameId.CacatacInvertedIdle3,
        { Pose: 14 } => EnemyFrameId.CacatacInvertedIdle4,
        { Pose: 15 } => EnemyFrameId.CacatacInvertedIdle5,
        { Pose: 16 } => EnemyFrameId.CacatacInvertedIdle6,
        { Pose: 17 } => EnemyFrameId.CacatacInvertedIdle7,
        { Pose: 18 } => EnemyFrameId.CacatacInvertedAttack1,
        { Pose: 19 } => EnemyFrameId.CacatacInvertedAttack2,
        { Pose: 2 } => EnemyFrameId.CacatacUprightIdle2,
        { Pose: 3 } => EnemyFrameId.CacatacUprightIdle3,
        { Pose: 4 } => EnemyFrameId.CacatacUprightIdle4,
        { Pose: 5 } => EnemyFrameId.CacatacUprightIdle5,
        { Pose: 6 } => EnemyFrameId.CacatacUprightIdle6,
        { Pose: 7 } => EnemyFrameId.CacatacUprightIdle7,
        { Pose: 8 } => EnemyFrameId.CacatacUprightAttack1,
        { Pose: 9 } => EnemyFrameId.CacatacUprightAttack2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the CacatacFrame programs show."),
    };
}
