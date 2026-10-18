using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="NorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NorfairRioFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(NorfairRioFrame frame) => frame.Pose switch
    {
        0 => 0xc427,
        1 => 0xc442,
        2 => 0xc45d,
        3 => 0xc493,
        4 => 0xc49f,
        5 => 0xc4ab,
        6 => 0xc4b7,
        7 => 0xc4d2,
        8 => 0xc4ed,
        9 => 0xc508,
        10 => 0xc523,
        11 => 0xc534,
        12 => 0xc54f,
        13 => 0xc56a,
        14 => 0xc585,
        15 => 0xc5a0,
        16 => 0xc5bb,
        17 => 0xc5d6,
        18 => 0xc5e2,
        19 => 0xc5ee,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Norfair Rio pose."),
    };

    /// <summary>The installed frame each <see cref="NorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<NorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NorfairRioC427 },
        { new(Pose: 1), EnemyFrameId.NorfairRioC442 },
        { new(Pose: 10), EnemyFrameId.NorfairRioC523 },
        { new(Pose: 11), EnemyFrameId.NorfairRioC534 },
        { new(Pose: 12), EnemyFrameId.NorfairRioC54f },
        { new(Pose: 13), EnemyFrameId.NorfairRioC56a },
        { new(Pose: 14), EnemyFrameId.NorfairRioC585 },
        { new(Pose: 15), EnemyFrameId.NorfairRioC5a0 },
        { new(Pose: 16), EnemyFrameId.NorfairRioC5bb },
        { new(Pose: 17), EnemyFrameId.NorfairRioC5d6 },
        { new(Pose: 18), EnemyFrameId.NorfairRioC5e2 },
        { new(Pose: 19), EnemyFrameId.NorfairRioC5ee },
        { new(Pose: 2), EnemyFrameId.NorfairRioC45d },
        { new(Pose: 3), EnemyFrameId.NorfairRioC493 },
        { new(Pose: 4), EnemyFrameId.NorfairRioC49f },
        { new(Pose: 5), EnemyFrameId.NorfairRioC4ab },
        { new(Pose: 6), EnemyFrameId.NorfairRioC4b7 },
        { new(Pose: 7), EnemyFrameId.NorfairRioC4d2 },
        { new(Pose: 8), EnemyFrameId.NorfairRioC4ed },
        { new(Pose: 9), EnemyFrameId.NorfairRioC508 },
    };
}
