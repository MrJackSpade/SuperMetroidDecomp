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

    /// <summary>The installed frame each <see cref="MaridiaLargeSnailFrame"/> shows.</summary>
    internal static EnemyFrameTable<MaridiaLargeSnailFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.OumOamCB87 },
        { new(Pose: 1), EnemyFrameId.OumOamCB91 },
        { new(Pose: 10), EnemyFrameId.OumOamCBEB },
        { new(Pose: 11), EnemyFrameId.OumOamCBF5 },
        { new(Pose: 12), EnemyFrameId.OumOamCBFF },
        { new(Pose: 13), EnemyFrameId.OumOamCC09 },
        { new(Pose: 14), EnemyFrameId.OumOamCC13 },
        { new(Pose: 15), EnemyFrameId.OumOamCC1D },
        { new(Pose: 16), EnemyFrameId.OumOamCC27 },
        { new(Pose: 17), EnemyFrameId.OumOamCC31 },
        { new(Pose: 18), EnemyFrameId.OumOamCC3B },
        { new(Pose: 19), EnemyFrameId.OumOamCC45 },
        { new(Pose: 2), EnemyFrameId.OumOamCB9B },
        { new(Pose: 20), EnemyFrameId.OumOamCC4F },
        { new(Pose: 21), EnemyFrameId.OumOamCC59 },
        { new(Pose: 22), EnemyFrameId.OumOamCC63 },
        { new(Pose: 23), EnemyFrameId.OumOamCC6D },
        { new(Pose: 24), EnemyFrameId.OumOamCC77 },
        { new(Pose: 25), EnemyFrameId.OumOamCC81 },
        { new(Pose: 26), EnemyFrameId.OumOamCC8B },
        { new(Pose: 27), EnemyFrameId.OumOamCC95 },
        { new(Pose: 28), EnemyFrameId.OumOamCC9F },
        { new(Pose: 29), EnemyFrameId.OumOamCCA9 },
        { new(Pose: 3), EnemyFrameId.OumOamCBA5 },
        { new(Pose: 4), EnemyFrameId.OumOamCBAF },
        { new(Pose: 5), EnemyFrameId.OumOamCBB9 },
        { new(Pose: 6), EnemyFrameId.OumOamCBC3 },
        { new(Pose: 7), EnemyFrameId.OumOamCBCD },
        { new(Pose: 8), EnemyFrameId.OumOamCBD7 },
        { new(Pose: 9), EnemyFrameId.OumOamCBE1 },
    };
}
