using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Beetom.Artwork;

/// <summary>
/// The presentation boundary of the Beetom's programs (#641): the bank-$A8 spritemap each
/// <see cref="BeetomFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BeetomFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BeetomFrame frame) => frame.Pose switch
    {
        0 => 0xbed3,
        1 => 0xbeee,
        2 => 0xbf09,
        3 => 0xbf24,
        4 => 0xbf3f,
        5 => 0xbf5a,
        6 => 0xbf75,
        7 => 0xbf90,
        8 => 0xbfab,
        9 => 0xbfcb,
        10 => 0xbfeb,
        11 => 0xc00b,
        12 => 0xc026,
        13 => 0xc041,
        14 => 0xc05c,
        15 => 0xc077,
        16 => 0xc092,
        17 => 0xc0ad,
        18 => 0xc0c8,
        19 => 0xc0e3,
        20 => 0xc103,
        21 => 0xc123,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Beetom pose."),
    };

    /// <summary>The installed frame each <see cref="BeetomFrame"/> shows.</summary>
    internal static EnemyFrameTable<BeetomFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.BeetomLeftCrawl0 },
        { new(Pose: 1), EnemyFrameId.BeetomLeftCrawl1 },
        { new(Pose: 10), EnemyFrameId.BeetomLeftDrain5 },
        { new(Pose: 11), EnemyFrameId.BeetomRightCrawl0 },
        { new(Pose: 12), EnemyFrameId.BeetomRightCrawl1 },
        { new(Pose: 13), EnemyFrameId.BeetomRightCrawl2 },
        { new(Pose: 14), EnemyFrameId.BeetomRightHop0 },
        { new(Pose: 15), EnemyFrameId.BeetomRightHop1 },
        { new(Pose: 16), EnemyFrameId.BeetomRightDrain0 },
        { new(Pose: 17), EnemyFrameId.BeetomRightDrain1 },
        { new(Pose: 18), EnemyFrameId.BeetomRightDrain2 },
        { new(Pose: 19), EnemyFrameId.BeetomRightDrain3 },
        { new(Pose: 2), EnemyFrameId.BeetomLeftCrawl2 },
        { new(Pose: 20), EnemyFrameId.BeetomRightDrain4 },
        { new(Pose: 21), EnemyFrameId.BeetomRightDrain5 },
        { new(Pose: 3), EnemyFrameId.BeetomLeftHop0 },
        { new(Pose: 4), EnemyFrameId.BeetomLeftHop1 },
        { new(Pose: 5), EnemyFrameId.BeetomLeftDrain0 },
        { new(Pose: 6), EnemyFrameId.BeetomLeftDrain1 },
        { new(Pose: 7), EnemyFrameId.BeetomLeftDrain2 },
        { new(Pose: 8), EnemyFrameId.BeetomLeftDrain3 },
        { new(Pose: 9), EnemyFrameId.BeetomLeftDrain4 },
    };
}
