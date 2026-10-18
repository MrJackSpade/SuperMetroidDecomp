using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver.Artwork;

/// <summary>
/// The presentation boundary of Waver's programs (#641): the bank-$A3 spritemap each
/// <see cref="WaverFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class WaverFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(WaverFrame frame) => frame.Pose switch
    {
        0 => 0x881e,
        1 => 0x8834,
        2 => 0x884a,
        3 => 0x885b,
        4 => 0x8871,
        5 => 0x8887,
        6 => 0x889d,
        7 => 0x88b3,
        8 => 0x88c4,
        9 => 0x88da,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Waver pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(WaverFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.WaverSpinLeft2,
        { Pose: 1 } => EnemyFrameId.WaverSpinLeft3,
        { Pose: 2 } => EnemyFrameId.WaverSteadyLeft,
        { Pose: 3 } => EnemyFrameId.WaverSpinLeft0,
        { Pose: 4 } => EnemyFrameId.WaverSpinLeft1,
        { Pose: 5 } => EnemyFrameId.WaverSpinRight2,
        { Pose: 6 } => EnemyFrameId.WaverSpinRight3,
        { Pose: 7 } => EnemyFrameId.WaverSteadyRight,
        { Pose: 8 } => EnemyFrameId.WaverSpinRight0,
        { Pose: 9 } => EnemyFrameId.WaverSpinRight1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the WaverFrame programs show."),
    };
}
