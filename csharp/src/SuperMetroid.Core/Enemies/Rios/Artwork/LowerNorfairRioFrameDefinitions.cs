using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios.Artwork;

/// <summary>
/// The presentation boundary of the Lower Norfair Rio's programs (#641): the bank-$A2 spritemap each
/// <see cref="LowerNorfairRioFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class LowerNorfairRioFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(LowerNorfairRioFrame frame) => frame.Pose switch
    {
        0 => 0xc8bd,
        1 => 0xc8d3,
        2 => 0xc8e9,
        3 => 0xc8ff,
        4 => 0xc915,
        5 => 0xc92b,
        6 => 0xc941,
        7 => 0xc957,
        8 => 0xc96d,
        9 => 0xc983,
        10 => 0xc999,
        11 => 0xc9af,
        12 => 0xc9c5,
        13 => 0xc9db,
        14 => 0xc9f1,
        15 => 0xca07,
        16 => 0xca13,
        17 => 0xca1f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Lower Norfair Rio pose."),
    };

    /// <summary>The installed frame each <see cref="LowerNorfairRioFrame"/> shows.</summary>
    internal static EnemyFrameTable<LowerNorfairRioFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.LowerNorfairRioC8bd },
        { new(Pose: 1), EnemyFrameId.LowerNorfairRioC8d3 },
        { new(Pose: 10), EnemyFrameId.LowerNorfairRioC999 },
        { new(Pose: 11), EnemyFrameId.LowerNorfairRioC9af },
        { new(Pose: 12), EnemyFrameId.LowerNorfairRioC9c5 },
        { new(Pose: 13), EnemyFrameId.LowerNorfairRioC9db },
        { new(Pose: 14), EnemyFrameId.LowerNorfairRioC9f1 },
        { new(Pose: 15), EnemyFrameId.LowerNorfairRioCa07 },
        { new(Pose: 16), EnemyFrameId.LowerNorfairRioCa13 },
        { new(Pose: 17), EnemyFrameId.LowerNorfairRioCa1f },
        { new(Pose: 2), EnemyFrameId.LowerNorfairRioC8e9 },
        { new(Pose: 3), EnemyFrameId.LowerNorfairRioC8ff },
        { new(Pose: 4), EnemyFrameId.LowerNorfairRioC915 },
        { new(Pose: 5), EnemyFrameId.LowerNorfairRioC92b },
        { new(Pose: 6), EnemyFrameId.LowerNorfairRioC941 },
        { new(Pose: 7), EnemyFrameId.LowerNorfairRioC957 },
        { new(Pose: 8), EnemyFrameId.LowerNorfairRioC96d },
        { new(Pose: 9), EnemyFrameId.LowerNorfairRioC983 },
    };
}
