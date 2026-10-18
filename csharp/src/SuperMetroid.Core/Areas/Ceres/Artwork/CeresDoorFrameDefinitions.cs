using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The presentation boundary of the Ceres door's programs (#641): the bank-$A6 spritemap each
/// <see cref="CeresDoorFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class CeresDoorFrameDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>The spritemap $A6:FAC7 every door starts on, before its program shows a pose.</summary>
    internal const ushort InitialSpritemap = 0xfac7;

    /// <summary>The Ridley-room door overlay $A6:A329 that Ridley's scene draws while it holds the door.</summary>
    internal const ushort RidleyPrivateOverlaySpritemap = 0xa329;

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CeresDoorFrame frame) => frame.Pose switch
    {
        0 => 0xf921,
        1 => 0xf95f,
        2 => 0xf989,
        3 => 0xf9b3,
        4 => 0xf9d3,
        5 => 0xf9f3,
        6 => 0xfa13,
        7 => 0xfa3d,
        8 => 0xfa67,
        9 => 0xfa87,
        10 => 0xfaa7,
        11 => 0xface,
        12 => 0xfb2f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ceres door pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(CeresDoorFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.CeresDoorRotatingOverlay,
        { Pose: 1 } => EnemyFrameId.CeresDoorLeftHold,
        { Pose: 10 } => EnemyFrameId.CeresDoorRightTransition3,
        { Pose: 11 } => EnemyFrameId.CeresDoorMode7LeftWall,
        { Pose: 12 } => EnemyFrameId.CeresDoorMode7RightWall,
        { Pose: 2 } => EnemyFrameId.CeresDoorLeftTransition0,
        { Pose: 3 } => EnemyFrameId.CeresDoorLeftTransition1,
        { Pose: 4 } => EnemyFrameId.CeresDoorLeftTransition2,
        { Pose: 5 } => EnemyFrameId.CeresDoorLeftTransition3,
        { Pose: 6 } => EnemyFrameId.CeresDoorRightHold,
        { Pose: 7 } => EnemyFrameId.CeresDoorRightTransition0,
        { Pose: 8 } => EnemyFrameId.CeresDoorRightTransition1,
        { Pose: 9 } => EnemyFrameId.CeresDoorRightTransition2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the CeresDoorFrame programs show."),
    };
}
