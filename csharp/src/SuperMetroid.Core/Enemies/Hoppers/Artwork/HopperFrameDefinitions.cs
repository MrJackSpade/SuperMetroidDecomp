using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers.Artwork;

/// <summary>
/// The presentation boundary of the Sidehoppers' and Dessgeegas' programs (#641): the bank-$A3 spritemap each
/// <see cref="HopperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class HopperFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(HopperFrame frame) => frame.Pose switch
    {
        0 => 0xaee3,
        1 => 0xaefe,
        2 => 0xaf19,
        3 => 0xaf34,
        4 => 0xaf4f,
        5 => 0xaf6a,
        6 => 0xafe3,
        7 => 0xaffe,
        8 => 0xb019,
        9 => 0xb034,
        10 => 0xb04f,
        11 => 0xb06a,
        12 => 0xb111,
        13 => 0xb136,
        14 => 0xb15b,
        15 => 0xb194,
        16 => 0xb1b9,
        17 => 0xb1de,
        18 => 0xb273,
        19 => 0xb2a2,
        20 => 0xb2d1,
        21 => 0xb30a,
        22 => 0xb339,
        23 => 0xb368,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Hopper pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(HopperFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.SidehopperLandFloor0,
        { Pose: 1 } => EnemyFrameId.SidehopperLandFloor1,
        { Pose: 10 } => EnemyFrameId.DessgeegaLandCeiling1,
        { Pose: 11 } => EnemyFrameId.DessgeegaJumpCeiling,
        { Pose: 12 } => EnemyFrameId.LargeSidehopperLandFloor0,
        { Pose: 13 } => EnemyFrameId.LargeSidehopperLandFloor1,
        { Pose: 14 } => EnemyFrameId.LargeSidehopperJumpFloor,
        { Pose: 15 } => EnemyFrameId.LargeSidehopperLandCeiling0,
        { Pose: 16 } => EnemyFrameId.LargeSidehopperLandCeiling1,
        { Pose: 17 } => EnemyFrameId.LargeSidehopperJumpCeiling,
        { Pose: 18 } => EnemyFrameId.LargeDessgeegaLandFloor0,
        { Pose: 19 } => EnemyFrameId.LargeDessgeegaLandFloor1,
        { Pose: 2 } => EnemyFrameId.SidehopperJumpFloor,
        { Pose: 20 } => EnemyFrameId.LargeDessgeegaJumpFloor,
        { Pose: 21 } => EnemyFrameId.LargeDessgeegaLandCeiling0,
        { Pose: 22 } => EnemyFrameId.LargeDessgeegaLandCeiling1,
        { Pose: 23 } => EnemyFrameId.LargeDessgeegaJumpCeiling,
        { Pose: 3 } => EnemyFrameId.SidehopperLandCeiling0,
        { Pose: 4 } => EnemyFrameId.SidehopperLandCeiling1,
        { Pose: 5 } => EnemyFrameId.SidehopperJumpCeiling,
        { Pose: 6 } => EnemyFrameId.DessgeegaLandFloor0,
        { Pose: 7 } => EnemyFrameId.DessgeegaLandFloor1,
        { Pose: 8 } => EnemyFrameId.DessgeegaJumpFloor,
        { Pose: 9 } => EnemyFrameId.DessgeegaLandCeiling0,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the HopperFrame programs show."),
    };
}
