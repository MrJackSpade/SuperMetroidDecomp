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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(LowerNorfairRioFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.LowerNorfairRioC8bd,
        { Pose: 1 } => EnemyFrameId.LowerNorfairRioC8d3,
        { Pose: 10 } => EnemyFrameId.LowerNorfairRioC999,
        { Pose: 11 } => EnemyFrameId.LowerNorfairRioC9af,
        { Pose: 12 } => EnemyFrameId.LowerNorfairRioC9c5,
        { Pose: 13 } => EnemyFrameId.LowerNorfairRioC9db,
        { Pose: 14 } => EnemyFrameId.LowerNorfairRioC9f1,
        { Pose: 15 } => EnemyFrameId.LowerNorfairRioCa07,
        { Pose: 16 } => EnemyFrameId.LowerNorfairRioCa13,
        { Pose: 17 } => EnemyFrameId.LowerNorfairRioCa1f,
        { Pose: 2 } => EnemyFrameId.LowerNorfairRioC8e9,
        { Pose: 3 } => EnemyFrameId.LowerNorfairRioC8ff,
        { Pose: 4 } => EnemyFrameId.LowerNorfairRioC915,
        { Pose: 5 } => EnemyFrameId.LowerNorfairRioC92b,
        { Pose: 6 } => EnemyFrameId.LowerNorfairRioC941,
        { Pose: 7 } => EnemyFrameId.LowerNorfairRioC957,
        { Pose: 8 } => EnemyFrameId.LowerNorfairRioC96d,
        { Pose: 9 } => EnemyFrameId.LowerNorfairRioC983,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the LowerNorfairRioFrame programs show."),
    };
}
