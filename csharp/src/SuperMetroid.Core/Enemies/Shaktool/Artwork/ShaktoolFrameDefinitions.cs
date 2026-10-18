using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool.Artwork;

/// <summary>
/// The presentation boundary of the Shaktool's programs (#641): the bank-$AA spritemap each
/// <see cref="ShaktoolFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShaktoolFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ShaktoolFrame frame) => frame.Pose switch
    {
        0 => 0xdf5c,
        1 => 0xdf63,
        2 => 0xdf6a,
        3 => 0xdf71,
        4 => 0xdf78,
        5 => 0xdf8e,
        6 => 0xdfa4,
        7 => 0xdfba,
        8 => 0xdfd0,
        9 => 0xdfe6,
        10 => 0xdffc,
        11 => 0xe012,
        12 => 0xe028,
        13 => 0xe02f,
        14 => 0xe036,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Shaktool pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(ShaktoolFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.ShaktoolFinalSaw0,
        { Pose: 1 } => EnemyFrameId.ShaktoolFinalSaw1,
        { Pose: 10 } => EnemyFrameId.ShaktoolHeadDown,
        { Pose: 11 } => EnemyFrameId.ShaktoolHeadDownLeft,
        { Pose: 12 } => EnemyFrameId.ShaktoolPrimarySaw0,
        { Pose: 13 } => EnemyFrameId.ShaktoolPrimarySaw1,
        { Pose: 14 } => EnemyFrameId.ShaktoolPrimarySaw2,
        { Pose: 2 } => EnemyFrameId.ShaktoolFinalSaw2,
        { Pose: 3 } => EnemyFrameId.ShaktoolArm,
        { Pose: 4 } => EnemyFrameId.ShaktoolHeadLeft,
        { Pose: 5 } => EnemyFrameId.ShaktoolHeadUpLeft,
        { Pose: 6 } => EnemyFrameId.ShaktoolHeadUp,
        { Pose: 7 } => EnemyFrameId.ShaktoolHeadUpRight,
        { Pose: 8 } => EnemyFrameId.ShaktoolHeadRight,
        { Pose: 9 } => EnemyFrameId.ShaktoolHeadDownRight,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the ShaktoolFrame programs show."),
    };
}
