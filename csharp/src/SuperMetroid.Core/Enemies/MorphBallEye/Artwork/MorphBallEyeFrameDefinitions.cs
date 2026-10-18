using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MorphBallEye.Artwork;

/// <summary>
/// The presentation boundary of the Morph Ball eye's programs (#641): the bank-$A8 spritemap each
/// <see cref="MorphBallEyeFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class MorphBallEyeFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(MorphBallEyeFrame frame) => frame.Pose switch
    {
        0 => 0x91df,
        1 => 0x91e6,
        2 => 0x91ed,
        3 => 0x91f4,
        4 => 0x91fb,
        5 => 0x9202,
        6 => 0x9209,
        7 => 0x9210,
        8 => 0x9217,
        9 => 0x921e,
        10 => 0x9225,
        11 => 0x922c,
        12 => 0x9233,
        13 => 0x923a,
        14 => 0x9241,
        15 => 0x9257,
        16 => 0x926d,
        17 => 0x9283,
        18 => 0x9299,
        19 => 0x92a5,
        20 => 0x92b1,
        21 => 0x92bd,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Morph Ball eye pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(MorphBallEyeFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.MorphEyeActive13,
        { Pose: 1 } => EnemyFrameId.MorphEyeActive12,
        { Pose: 10 } => EnemyFrameId.MorphEyeActive3,
        { Pose: 11 } => EnemyFrameId.MorphEyeActive4,
        { Pose: 12 } => EnemyFrameId.MorphEyeActive5,
        { Pose: 13 } => EnemyFrameId.MorphEyeActive6,
        { Pose: 14 } => EnemyFrameId.MorphEyeRightClosed,
        { Pose: 15 } => EnemyFrameId.MorphEyeRightTransition,
        { Pose: 16 } => EnemyFrameId.MorphEyeLeftClosed,
        { Pose: 17 } => EnemyFrameId.MorphEyeLeftTransition,
        { Pose: 18 } => EnemyFrameId.MorphEyeMountRight,
        { Pose: 19 } => EnemyFrameId.MorphEyeMountDown,
        { Pose: 2 } => EnemyFrameId.MorphEyeActive11,
        { Pose: 20 } => EnemyFrameId.MorphEyeMountLeft,
        { Pose: 21 } => EnemyFrameId.MorphEyeMountUp,
        { Pose: 3 } => EnemyFrameId.MorphEyeActive10,
        { Pose: 4 } => EnemyFrameId.MorphEyeActive9,
        { Pose: 5 } => EnemyFrameId.MorphEyeActive8,
        { Pose: 6 } => EnemyFrameId.MorphEyeActive7,
        { Pose: 7 } => EnemyFrameId.MorphEyeActive0,
        { Pose: 8 } => EnemyFrameId.MorphEyeActive1,
        { Pose: 9 } => EnemyFrameId.MorphEyeActive2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the MorphBallEyeFrame programs show."),
    };
}
