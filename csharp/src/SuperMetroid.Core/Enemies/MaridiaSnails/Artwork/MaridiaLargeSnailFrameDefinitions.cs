using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;

/// <summary>
/// The presentation boundary of Oum's programs (#641): the bank-$A2 spritemap each
/// <see cref="MaridiaLargeSnailFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MaridiaLargeSnailFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MaridiaLargeSnailFrame frame) => frame.Pose switch
    {
        0 => 0xcb87,
        1 => 0xcb91,
        2 => 0xcb9b,
        3 => 0xcba5,
        4 => 0xcbaf,
        5 => 0xcbb9,
        6 => 0xcbc3,
        7 => 0xcbcd,
        8 => 0xcbd7,
        9 => 0xcbe1,
        10 => 0xcbeb,
        11 => 0xcbf5,
        12 => 0xcbff,
        13 => 0xcc09,
        14 => 0xcc13,
        15 => 0xcc1d,
        16 => 0xcc27,
        17 => 0xcc31,
        18 => 0xcc3b,
        19 => 0xcc45,
        20 => 0xcc4f,
        21 => 0xcc59,
        22 => 0xcc63,
        23 => 0xcc6d,
        24 => 0xcc77,
        25 => 0xcc81,
        26 => 0xcc8b,
        27 => 0xcc95,
        28 => 0xcc9f,
        29 => 0xcca9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Oum pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MaridiaLargeSnailFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.OumOamCB87,
        { Pose: 1 } => EnemyFrameId.OumOamCB91,
        { Pose: 10 } => EnemyFrameId.OumOamCBEB,
        { Pose: 11 } => EnemyFrameId.OumOamCBF5,
        { Pose: 12 } => EnemyFrameId.OumOamCBFF,
        { Pose: 13 } => EnemyFrameId.OumOamCC09,
        { Pose: 14 } => EnemyFrameId.OumOamCC13,
        { Pose: 15 } => EnemyFrameId.OumOamCC1D,
        { Pose: 16 } => EnemyFrameId.OumOamCC27,
        { Pose: 17 } => EnemyFrameId.OumOamCC31,
        { Pose: 18 } => EnemyFrameId.OumOamCC3B,
        { Pose: 19 } => EnemyFrameId.OumOamCC45,
        { Pose: 2 } => EnemyFrameId.OumOamCB9B,
        { Pose: 20 } => EnemyFrameId.OumOamCC4F,
        { Pose: 21 } => EnemyFrameId.OumOamCC59,
        { Pose: 22 } => EnemyFrameId.OumOamCC63,
        { Pose: 23 } => EnemyFrameId.OumOamCC6D,
        { Pose: 24 } => EnemyFrameId.OumOamCC77,
        { Pose: 25 } => EnemyFrameId.OumOamCC81,
        { Pose: 26 } => EnemyFrameId.OumOamCC8B,
        { Pose: 27 } => EnemyFrameId.OumOamCC95,
        { Pose: 28 } => EnemyFrameId.OumOamCC9F,
        { Pose: 29 } => EnemyFrameId.OumOamCCA9,
        { Pose: 3 } => EnemyFrameId.OumOamCBA5,
        { Pose: 4 } => EnemyFrameId.OumOamCBAF,
        { Pose: 5 } => EnemyFrameId.OumOamCBB9,
        { Pose: 6 } => EnemyFrameId.OumOamCBC3,
        { Pose: 7 } => EnemyFrameId.OumOamCBCD,
        { Pose: 8 } => EnemyFrameId.OumOamCBD7,
        { Pose: 9 } => EnemyFrameId.OumOamCBE1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MaridiaLargeSnailFrame programs show."),
    };
}
