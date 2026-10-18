using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi.Artwork;

/// <summary>
/// The presentation boundary of the Hibashi's programs (#641): the bank-$A6 spritemap each
/// <see cref="HibashiFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class HibashiFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(HibashiFrame frame) => frame.Pose switch
    {
        0 => 0x9082,
        1 => 0x9089,
        2 => 0x909f,
        3 => 0x90ba,
        4 => 0x90da,
        5 => 0x90ff,
        6 => 0x9124,
        7 => 0x9149,
        8 => 0x916e,
        9 => 0x9198,
        10 => 0x91c2,
        11 => 0x91f1,
        12 => 0x9234,
        13 => 0x927c,
        14 => 0x92c4,
        15 => 0x9302,
        16 => 0x934a,
        17 => 0x938d,
        18 => 0x93cb,
        19 => 0x9404,
        20 => 0x942e,
        21 => 0x944e,
        22 => 0x9469,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Hibashi pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(HibashiFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.Hibashi00,
        { Pose: 1 } => EnemyFrameId.Hibashi01,
        { Pose: 10 } => EnemyFrameId.Hibashi10,
        { Pose: 11 } => EnemyFrameId.Hibashi11,
        { Pose: 12 } => EnemyFrameId.Hibashi12,
        { Pose: 13 } => EnemyFrameId.Hibashi13,
        { Pose: 14 } => EnemyFrameId.Hibashi14,
        { Pose: 15 } => EnemyFrameId.Hibashi15,
        { Pose: 16 } => EnemyFrameId.Hibashi16,
        { Pose: 17 } => EnemyFrameId.Hibashi17,
        { Pose: 18 } => EnemyFrameId.Hibashi18,
        { Pose: 19 } => EnemyFrameId.Hibashi19,
        { Pose: 2 } => EnemyFrameId.Hibashi02,
        { Pose: 20 } => EnemyFrameId.Hibashi20,
        { Pose: 21 } => EnemyFrameId.Hibashi21,
        { Pose: 22 } => EnemyFrameId.Hibashi22,
        { Pose: 3 } => EnemyFrameId.Hibashi03,
        { Pose: 4 } => EnemyFrameId.Hibashi04,
        { Pose: 5 } => EnemyFrameId.Hibashi05,
        { Pose: 6 } => EnemyFrameId.Hibashi06,
        { Pose: 7 } => EnemyFrameId.Hibashi07,
        { Pose: 8 } => EnemyFrameId.Hibashi08,
        { Pose: 9 } => EnemyFrameId.Hibashi09,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the HibashiFrame programs show."),
    };
}
