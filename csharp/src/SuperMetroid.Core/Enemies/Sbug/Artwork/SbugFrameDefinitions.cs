using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sbug.Artwork;

/// <summary>
/// The presentation boundary of Sbug's programs (#641): the bank-$A3 spritemap each
/// <see cref="SbugFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SbugFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SbugFrame frame) => frame.Pose switch
    {
        0 => 0xa67d,
        1 => 0xa684,
        2 => 0xa68b,
        3 => 0xa692,
        4 => 0xa699,
        5 => 0xa6a0,
        6 => 0xa6a7,
        7 => 0xa6ae,
        8 => 0xa6b5,
        9 => 0xa6bc,
        10 => 0xa6c3,
        11 => 0xa6ca,
        12 => 0xa6d1,
        13 => 0xa6d8,
        14 => 0xa6df,
        15 => 0xa6e6,
        16 => 0xa6ed,
        17 => 0xa6f4,
        18 => 0xa6fb,
        19 => 0xa702,
        20 => 0xa709,
        21 => 0xa710,
        22 => 0xa717,
        23 => 0xa71e,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Sbug pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(SbugFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.SbugRight0,
        { Pose: 1 } => EnemyFrameId.SbugRight1,
        { Pose: 10 } => EnemyFrameId.SbugUpLeft1,
        { Pose: 11 } => EnemyFrameId.SbugUpLeft2,
        { Pose: 12 } => EnemyFrameId.SbugLeft0,
        { Pose: 13 } => EnemyFrameId.SbugLeft1,
        { Pose: 14 } => EnemyFrameId.SbugLeft2,
        { Pose: 15 } => EnemyFrameId.SbugDownLeft0,
        { Pose: 16 } => EnemyFrameId.SbugDownLeft1,
        { Pose: 17 } => EnemyFrameId.SbugDownLeft2,
        { Pose: 18 } => EnemyFrameId.SbugDown0,
        { Pose: 19 } => EnemyFrameId.SbugDown1,
        { Pose: 2 } => EnemyFrameId.SbugRight2,
        { Pose: 20 } => EnemyFrameId.SbugDown2,
        { Pose: 21 } => EnemyFrameId.SbugDownRight0,
        { Pose: 22 } => EnemyFrameId.SbugDownRight1,
        { Pose: 23 } => EnemyFrameId.SbugDownRight2,
        { Pose: 3 } => EnemyFrameId.SbugUpRight0,
        { Pose: 4 } => EnemyFrameId.SbugUpRight1,
        { Pose: 5 } => EnemyFrameId.SbugUpRight2,
        { Pose: 6 } => EnemyFrameId.SbugUp0,
        { Pose: 7 } => EnemyFrameId.SbugUp1,
        { Pose: 8 } => EnemyFrameId.SbugUp2,
        { Pose: 9 } => EnemyFrameId.SbugUpLeft0,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the SbugFrame programs show."),
    };
}
