using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver.Artwork;

/// <summary>
/// The presentation boundary of Waver's programs (#641): the bank-$A3 spritemap each
/// <see cref="WaverFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WaverFrameDefinitions
{

    /// <summary>The installed frame each <see cref="WaverFrame"/> shows.</summary>
    internal static EnemyFrameTable<WaverFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.WaverSpinLeft2 },
        { new(Pose: 1), EnemyFrameId.WaverSpinLeft3 },
        { new(Pose: 2), EnemyFrameId.WaverSteadyLeft },
        { new(Pose: 3), EnemyFrameId.WaverSpinLeft0 },
        { new(Pose: 4), EnemyFrameId.WaverSpinLeft1 },
        { new(Pose: 5), EnemyFrameId.WaverSpinRight2 },
        { new(Pose: 6), EnemyFrameId.WaverSpinRight3 },
        { new(Pose: 7), EnemyFrameId.WaverSteadyRight },
        { new(Pose: 8), EnemyFrameId.WaverSpinRight0 },
        { new(Pose: 9), EnemyFrameId.WaverSpinRight1 },
    };
}
