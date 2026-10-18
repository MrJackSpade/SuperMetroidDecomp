using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>
/// The presentation boundary of the Alcoon's programs (#641): the bank-$A8 spritemap each
/// <see cref="AlcoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class AlcoonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(AlcoonFrame frame) => frame.Pose switch
    {
        0 => 0xdfa2,
        1 => 0xdfc2,
        2 => 0xdfe2,
        3 => 0xe007,
        4 => 0xe027,
        5 => 0xe047,
        6 => 0xe06c,
        7 => 0xe09b,
        8 => 0xe0bb,
        9 => 0xe0db,
        10 => 0xe0fb,
        11 => 0xe11b,
        12 => 0xe140,
        13 => 0xe160,
        14 => 0xe180,
        15 => 0xe1a5,
        16 => 0xe1d4,
        17 => 0xe1f4,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(AlcoonFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.AlcoonLeftWalk0,
        { Pose: 1 } => EnemyFrameId.AlcoonLeftWalk1,
        { Pose: 10 } => EnemyFrameId.AlcoonRightWalk1,
        { Pose: 11 } => EnemyFrameId.AlcoonRightWalk2,
        { Pose: 12 } => EnemyFrameId.AlcoonRightWalk3,
        { Pose: 13 } => EnemyFrameId.AlcoonRightFire0,
        { Pose: 14 } => EnemyFrameId.AlcoonRightFire1,
        { Pose: 15 } => EnemyFrameId.AlcoonRightFire2,
        { Pose: 16 } => EnemyFrameId.AlcoonRightFire3,
        { Pose: 17 } => EnemyFrameId.AlcoonRightAirUp,
        { Pose: 2 } => EnemyFrameId.AlcoonLeftWalk2,
        { Pose: 3 } => EnemyFrameId.AlcoonLeftWalk3,
        { Pose: 4 } => EnemyFrameId.AlcoonLeftFire0,
        { Pose: 5 } => EnemyFrameId.AlcoonLeftFire1,
        { Pose: 6 } => EnemyFrameId.AlcoonLeftFire2,
        { Pose: 7 } => EnemyFrameId.AlcoonLeftFire3,
        { Pose: 8 } => EnemyFrameId.AlcoonLeftAirUp,
        { Pose: 9 } => EnemyFrameId.AlcoonRightWalk0,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the AlcoonFrame programs show."),
    };
}
