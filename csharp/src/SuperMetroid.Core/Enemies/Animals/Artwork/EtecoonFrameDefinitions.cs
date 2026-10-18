using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>
/// The presentation boundary of the Etecoon's programs (#641): the bank-$A7 spritemap each
/// <see cref="EtecoonFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class EtecoonFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EtecoonFrame frame) => frame.Pose switch
    {
        0 => 0xeeed,
        1 => 0xeefe,
        2 => 0xef0a,
        3 => 0xef16,
        4 => 0xef27,
        5 => 0xef38,
        6 => 0xef49,
        7 => 0xef5a,
        8 => 0xef6b,
        9 => 0xef90,
        10 => 0xefb5,
        11 => 0xefda,
        12 => 0xefff,
        13 => 0xf024,
        14 => 0xf06e,
        15 => 0xf089,
        16 => 0xf09a,
        17 => 0xf0a6,
        18 => 0xf0b2,
        19 => 0xf0c3,
        20 => 0xf0d4,
        21 => 0xf0e5,
        22 => 0xf0f6,
        23 => 0xf107,
        24 => 0xf12c,
        25 => 0xf151,
        26 => 0xf176,
        27 => 0xf19b,
        28 => 0xf1c0,
        29 => 0xf1e5,
        30 => 0xf20a,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Etecoon pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(EtecoonFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.Etecoon02,
        { Pose: 1 } => EnemyFrameId.Etecoon03,
        { Pose: 10 } => EnemyFrameId.Etecoon12,
        { Pose: 11 } => EnemyFrameId.Etecoon13,
        { Pose: 12 } => EnemyFrameId.Etecoon00,
        { Pose: 13 } => EnemyFrameId.Etecoon01,
        { Pose: 14 } => EnemyFrameId.Etecoon14,
        { Pose: 15 } => EnemyFrameId.Etecoon17,
        { Pose: 16 } => EnemyFrameId.Etecoon18,
        { Pose: 17 } => EnemyFrameId.Etecoon19,
        { Pose: 18 } => EnemyFrameId.Etecoon21,
        { Pose: 19 } => EnemyFrameId.Etecoon22,
        { Pose: 2 } => EnemyFrameId.Etecoon04,
        { Pose: 20 } => EnemyFrameId.Etecoon23,
        { Pose: 21 } => EnemyFrameId.Etecoon24,
        { Pose: 22 } => EnemyFrameId.Etecoon20,
        { Pose: 23 } => EnemyFrameId.Etecoon25,
        { Pose: 24 } => EnemyFrameId.Etecoon26,
        { Pose: 25 } => EnemyFrameId.Etecoon27,
        { Pose: 26 } => EnemyFrameId.Etecoon28,
        { Pose: 27 } => EnemyFrameId.Etecoon30,
        { Pose: 28 } => EnemyFrameId.Etecoon16,
        { Pose: 29 } => EnemyFrameId.Etecoon15,
        { Pose: 3 } => EnemyFrameId.Etecoon06,
        { Pose: 30 } => EnemyFrameId.Etecoon29,
        { Pose: 4 } => EnemyFrameId.Etecoon07,
        { Pose: 5 } => EnemyFrameId.Etecoon08,
        { Pose: 6 } => EnemyFrameId.Etecoon09,
        { Pose: 7 } => EnemyFrameId.Etecoon05,
        { Pose: 8 } => EnemyFrameId.Etecoon10,
        { Pose: 9 } => EnemyFrameId.Etecoon11,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the EtecoonFrame programs show."),
    };
}
