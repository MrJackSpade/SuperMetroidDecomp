using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers.Artwork;

/// <summary>
/// The presentation boundary of the Sidehoppers' and Dessgeegas' programs (#641): the bank-$A3 spritemap each
/// <see cref="HopperFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class HopperFrameDefinitions
{

    /// <summary>The installed frame each <see cref="HopperFrame"/> shows.</summary>
    internal static EnemyFrameTable<HopperFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.SidehopperLandFloor0 },
        { new(Pose: 1), EnemyFrameId.SidehopperLandFloor1 },
        { new(Pose: 10), EnemyFrameId.DessgeegaLandCeiling1 },
        { new(Pose: 11), EnemyFrameId.DessgeegaJumpCeiling },
        { new(Pose: 12), EnemyFrameId.LargeSidehopperLandFloor0 },
        { new(Pose: 13), EnemyFrameId.LargeSidehopperLandFloor1 },
        { new(Pose: 14), EnemyFrameId.LargeSidehopperJumpFloor },
        { new(Pose: 15), EnemyFrameId.LargeSidehopperLandCeiling0 },
        { new(Pose: 16), EnemyFrameId.LargeSidehopperLandCeiling1 },
        { new(Pose: 17), EnemyFrameId.LargeSidehopperJumpCeiling },
        { new(Pose: 18), EnemyFrameId.LargeDessgeegaLandFloor0 },
        { new(Pose: 19), EnemyFrameId.LargeDessgeegaLandFloor1 },
        { new(Pose: 2), EnemyFrameId.SidehopperJumpFloor },
        { new(Pose: 20), EnemyFrameId.LargeDessgeegaJumpFloor },
        { new(Pose: 21), EnemyFrameId.LargeDessgeegaLandCeiling0 },
        { new(Pose: 22), EnemyFrameId.LargeDessgeegaLandCeiling1 },
        { new(Pose: 23), EnemyFrameId.LargeDessgeegaJumpCeiling },
        { new(Pose: 3), EnemyFrameId.SidehopperLandCeiling0 },
        { new(Pose: 4), EnemyFrameId.SidehopperLandCeiling1 },
        { new(Pose: 5), EnemyFrameId.SidehopperJumpCeiling },
        { new(Pose: 6), EnemyFrameId.DessgeegaLandFloor0 },
        { new(Pose: 7), EnemyFrameId.DessgeegaLandFloor1 },
        { new(Pose: 8), EnemyFrameId.DessgeegaJumpFloor },
        { new(Pose: 9), EnemyFrameId.DessgeegaLandCeiling0 },
    };
}
