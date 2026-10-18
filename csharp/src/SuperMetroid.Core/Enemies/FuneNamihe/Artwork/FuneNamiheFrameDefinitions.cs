using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>
/// The presentation boundary of the Fune's and Namihe's programs (#641): the bank-$A8 spritemap each
/// <see cref="FuneNamiheFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class FuneNamiheFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(FuneNamiheFrame frame) => frame.Pose switch
    {
        0 => 0x93f9,
        1 => 0x9423,
        2 => 0x944d,
        3 => 0x9477,
        4 => 0x94a1,
        5 => 0x94cb,
        6 => 0x94f5,
        7 => 0x951f,
        8 => 0x9549,
        9 => 0x9573,
        10 => 0x97b4,
        11 => 0x97de,
        12 => 0x9808,
        13 => 0x9832,
        14 => 0x985c,
        15 => 0x9886,
        16 => 0x98b0,
        17 => 0x98da,
        18 => 0x9904,
        19 => 0x992e,
        20 => 0x9958,
        21 => 0x9982,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fune or Namihe pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(FuneNamiheFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.FuneLeftIdle,
        { Pose: 1 } => EnemyFrameId.FuneLeftActive0,
        { Pose: 10 } => EnemyFrameId.NamiheLeftIdle,
        { Pose: 11 } => EnemyFrameId.NamiheLeftActive0,
        { Pose: 12 } => EnemyFrameId.NamiheLeftActive1,
        { Pose: 13 } => EnemyFrameId.NamiheLeftActive2,
        { Pose: 14 } => EnemyFrameId.NamiheLeftActive3,
        { Pose: 15 } => EnemyFrameId.NamiheLeftActive4,
        { Pose: 16 } => EnemyFrameId.NamiheRightIdle,
        { Pose: 17 } => EnemyFrameId.NamiheRightActive0,
        { Pose: 18 } => EnemyFrameId.NamiheRightActive1,
        { Pose: 19 } => EnemyFrameId.NamiheRightActive2,
        { Pose: 2 } => EnemyFrameId.FuneLeftActive1,
        { Pose: 20 } => EnemyFrameId.NamiheRightActive3,
        { Pose: 21 } => EnemyFrameId.NamiheRightActive4,
        { Pose: 3 } => EnemyFrameId.FuneLeftActive2,
        { Pose: 4 } => EnemyFrameId.FuneLeftActive3,
        { Pose: 5 } => EnemyFrameId.FuneRightIdle,
        { Pose: 6 } => EnemyFrameId.FuneRightActive0,
        { Pose: 7 } => EnemyFrameId.FuneRightActive1,
        { Pose: 8 } => EnemyFrameId.FuneRightActive2,
        { Pose: 9 } => EnemyFrameId.FuneRightActive3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the FuneNamiheFrame programs show."),
    };
}
