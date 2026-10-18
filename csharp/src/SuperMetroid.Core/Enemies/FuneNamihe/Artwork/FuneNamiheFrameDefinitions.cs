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

    /// <summary>The installed frame each <see cref="FuneNamiheFrame"/> shows.</summary>
    internal static EnemyFrameTable<FuneNamiheFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.FuneLeftIdle },
        { new(Pose: 1), EnemyFrameId.FuneLeftActive0 },
        { new(Pose: 10), EnemyFrameId.NamiheLeftIdle },
        { new(Pose: 11), EnemyFrameId.NamiheLeftActive0 },
        { new(Pose: 12), EnemyFrameId.NamiheLeftActive1 },
        { new(Pose: 13), EnemyFrameId.NamiheLeftActive2 },
        { new(Pose: 14), EnemyFrameId.NamiheLeftActive3 },
        { new(Pose: 15), EnemyFrameId.NamiheLeftActive4 },
        { new(Pose: 16), EnemyFrameId.NamiheRightIdle },
        { new(Pose: 17), EnemyFrameId.NamiheRightActive0 },
        { new(Pose: 18), EnemyFrameId.NamiheRightActive1 },
        { new(Pose: 19), EnemyFrameId.NamiheRightActive2 },
        { new(Pose: 2), EnemyFrameId.FuneLeftActive1 },
        { new(Pose: 20), EnemyFrameId.NamiheRightActive3 },
        { new(Pose: 21), EnemyFrameId.NamiheRightActive4 },
        { new(Pose: 3), EnemyFrameId.FuneLeftActive2 },
        { new(Pose: 4), EnemyFrameId.FuneLeftActive3 },
        { new(Pose: 5), EnemyFrameId.FuneRightIdle },
        { new(Pose: 6), EnemyFrameId.FuneRightActive0 },
        { new(Pose: 7), EnemyFrameId.FuneRightActive1 },
        { new(Pose: 8), EnemyFrameId.FuneRightActive2 },
        { new(Pose: 9), EnemyFrameId.FuneRightActive3 },
    };
}
