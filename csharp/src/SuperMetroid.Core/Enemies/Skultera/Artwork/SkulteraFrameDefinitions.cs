using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skultera.Artwork;

/// <summary>
/// The presentation boundary of Skultera's programs (#641): the bank-$A3 spritemap each
/// <see cref="SkulteraFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class SkulteraFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(SkulteraFrame frame) => frame.Pose switch
    {
        0 => 0x928a,
        1 => 0x92a5,
        2 => 0x92c0,
        3 => 0x92db,
        4 => 0x92f6,
        5 => 0x9311,
        6 => 0x9327,
        7 => 0x933d,
        8 => 0x934e,
        9 => 0x9364,
        10 => 0x937f,
        11 => 0x939a,
        12 => 0x93b5,
        13 => 0x93d0,
        14 => 0x93eb,
        15 => 0x9406,
        16 => 0x9421,
        17 => 0x9437,
        18 => 0x944d,
        19 => 0x945e,
        20 => 0x9474,
        21 => 0x948f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Skultera pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(SkulteraFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.SkulteraSwimLeft0,
        { Pose: 1 } => EnemyFrameId.SkulteraSwimLeft1,
        { Pose: 10 } => EnemyFrameId.SkulteraTurnRight7,
        { Pose: 11 } => EnemyFrameId.SkulteraSwimRight0,
        { Pose: 12 } => EnemyFrameId.SkulteraSwimRight1,
        { Pose: 13 } => EnemyFrameId.SkulteraSwimRight2,
        { Pose: 14 } => EnemyFrameId.SkulteraTurnLeft0,
        { Pose: 15 } => EnemyFrameId.SkulteraTurnLeft1,
        { Pose: 16 } => EnemyFrameId.SkulteraTurnLeft2,
        { Pose: 17 } => EnemyFrameId.SkulteraTurnLeft3,
        { Pose: 18 } => EnemyFrameId.SkulteraTurnLeft4,
        { Pose: 19 } => EnemyFrameId.SkulteraTurnLeft5,
        { Pose: 2 } => EnemyFrameId.SkulteraSwimLeft2,
        { Pose: 20 } => EnemyFrameId.SkulteraTurnLeft6,
        { Pose: 21 } => EnemyFrameId.SkulteraTurnLeft7,
        { Pose: 3 } => EnemyFrameId.SkulteraTurnRight0,
        { Pose: 4 } => EnemyFrameId.SkulteraTurnRight1,
        { Pose: 5 } => EnemyFrameId.SkulteraTurnRight2,
        { Pose: 6 } => EnemyFrameId.SkulteraTurnRight3,
        { Pose: 7 } => EnemyFrameId.SkulteraTurnRight4,
        { Pose: 8 } => EnemyFrameId.SkulteraTurnRight5,
        { Pose: 9 } => EnemyFrameId.SkulteraTurnRight6,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the SkulteraFrame programs show."),
    };
}
