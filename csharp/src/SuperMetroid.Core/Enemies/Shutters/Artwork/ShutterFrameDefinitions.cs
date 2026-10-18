using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters.Artwork;

/// <summary>
/// The presentation boundary of the shutters' and Kamer's programs (#641): the bank-$A2 spritemap each
/// <see cref="ShutterFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class ShutterFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(ShutterFrame frame) => frame.Pose switch
    {
        0 => 0xed44,
        1 => 0xed57,
        2 => 0xed74,
        3 => 0xed9b,
        4 => 0xedb1,
        5 => 0xf468,
        6 => 0xf474,
        7 => 0xf480,
        8 => 0xf48c,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined shutter pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(ShutterFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.ShutterGrowing10px,
        { Pose: 1 } => EnemyFrameId.ShutterGrowing20px,
        { Pose: 2 } => EnemyFrameId.ShutterGrowing30px,
        { Pose: 3 } => EnemyFrameId.ShutterVertical40px,
        { Pose: 4 } => EnemyFrameId.ShutterHorizontal,
        { Pose: 5 } => EnemyFrameId.KamerPlatform0,
        { Pose: 6 } => EnemyFrameId.KamerPlatform1,
        { Pose: 7 } => EnemyFrameId.KamerPlatform2,
        { Pose: 8 } => EnemyFrameId.KamerPlatform3,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the ShutterFrame programs show."),
    };
}
