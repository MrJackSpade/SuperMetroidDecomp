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

    /// <summary>The installed frame each <see cref="SkulteraFrame"/> shows.</summary>
    internal static EnemyFrameTable<SkulteraFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.SkulteraSwimLeft0 },
        { new(Pose: 1), EnemyFrameId.SkulteraSwimLeft1 },
        { new(Pose: 10), EnemyFrameId.SkulteraTurnRight7 },
        { new(Pose: 11), EnemyFrameId.SkulteraSwimRight0 },
        { new(Pose: 12), EnemyFrameId.SkulteraSwimRight1 },
        { new(Pose: 13), EnemyFrameId.SkulteraSwimRight2 },
        { new(Pose: 14), EnemyFrameId.SkulteraTurnLeft0 },
        { new(Pose: 15), EnemyFrameId.SkulteraTurnLeft1 },
        { new(Pose: 16), EnemyFrameId.SkulteraTurnLeft2 },
        { new(Pose: 17), EnemyFrameId.SkulteraTurnLeft3 },
        { new(Pose: 18), EnemyFrameId.SkulteraTurnLeft4 },
        { new(Pose: 19), EnemyFrameId.SkulteraTurnLeft5 },
        { new(Pose: 2), EnemyFrameId.SkulteraSwimLeft2 },
        { new(Pose: 20), EnemyFrameId.SkulteraTurnLeft6 },
        { new(Pose: 21), EnemyFrameId.SkulteraTurnLeft7 },
        { new(Pose: 3), EnemyFrameId.SkulteraTurnRight0 },
        { new(Pose: 4), EnemyFrameId.SkulteraTurnRight1 },
        { new(Pose: 5), EnemyFrameId.SkulteraTurnRight2 },
        { new(Pose: 6), EnemyFrameId.SkulteraTurnRight3 },
        { new(Pose: 7), EnemyFrameId.SkulteraTurnRight4 },
        { new(Pose: 8), EnemyFrameId.SkulteraTurnRight5 },
        { new(Pose: 9), EnemyFrameId.SkulteraTurnRight6 },
    };
}
