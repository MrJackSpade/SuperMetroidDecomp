using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea.Artwork;

/// <summary>
/// The presentation boundary of Fireflea's programs (#641): the bank-$A3 spritemap each
/// <see cref="FirefleaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FirefleaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FirefleaFrame frame) => frame.Pose switch
    {
        0 => 0x8ea5,
        1 => 0x8eb6,
        2 => 0x8ec7,
        3 => 0x8ed8,
        4 => 0x8ee9,
        5 => 0x8efa,
        6 => 0x8f0b,
        7 => 0x8f1c,
        8 => 0x8f2d,
        9 => 0x8f3e,
        10 => 0x8f4f,
        11 => 0x8f60,
        12 => 0x8f71,
        13 => 0x8f82,
        14 => 0x8f93,
        15 => 0x8fa4,
        16 => 0x8fb5,
        17 => 0x8fc6,
        18 => 0x8fd7,
        19 => 0x8fe8,
        20 => 0x8ff9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fireflea pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(FirefleaFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.FirefleaCycle0,
        { Pose: 1 } => EnemyFrameId.FirefleaCycle1,
        { Pose: 10 } => EnemyFrameId.FirefleaCycle10,
        { Pose: 11 } => EnemyFrameId.FirefleaCycle11,
        { Pose: 12 } => EnemyFrameId.FirefleaCycle12,
        { Pose: 13 } => EnemyFrameId.FirefleaCycle13,
        { Pose: 14 } => EnemyFrameId.FirefleaCycle14,
        { Pose: 15 } => EnemyFrameId.FirefleaCycle15,
        { Pose: 16 } => EnemyFrameId.FirefleaCycle16,
        { Pose: 17 } => EnemyFrameId.FirefleaCycle17,
        { Pose: 18 } => EnemyFrameId.FirefleaCycle18,
        { Pose: 19 } => EnemyFrameId.FirefleaCycle19,
        { Pose: 2 } => EnemyFrameId.FirefleaCycle2,
        { Pose: 20 } => EnemyFrameId.FirefleaCycle20,
        { Pose: 3 } => EnemyFrameId.FirefleaCycle3,
        { Pose: 4 } => EnemyFrameId.FirefleaCycle4,
        { Pose: 5 } => EnemyFrameId.FirefleaCycle5,
        { Pose: 6 } => EnemyFrameId.FirefleaCycle6,
        { Pose: 7 } => EnemyFrameId.FirefleaCycle7,
        { Pose: 8 } => EnemyFrameId.FirefleaCycle8,
        { Pose: 9 } => EnemyFrameId.FirefleaCycle9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the FirefleaFrame programs show."),
    };
}
