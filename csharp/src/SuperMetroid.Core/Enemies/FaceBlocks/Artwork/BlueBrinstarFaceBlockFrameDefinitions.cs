using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// The presentation boundary of the Blue Brinstar face block's programs (#641): the bank-$A8 spritemap each
/// <see cref="BlueBrinstarFaceBlockFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class BlueBrinstarFaceBlockFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(BlueBrinstarFaceBlockFrame frame) => frame.Pose switch
    {
        0 => 0xe92c,
        1 => 0xe942,
        2 => 0xe958,
        3 => 0xe96e,
        4 => 0xe984,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Blue Brinstar face block pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(BlueBrinstarFaceBlockFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.FaceBlockNeutral,
        { Pose: 1 } => EnemyFrameId.FaceBlockSamusLeft1,
        { Pose: 2 } => EnemyFrameId.FaceBlockSamusLeft2,
        { Pose: 3 } => EnemyFrameId.FaceBlockSamusRight1,
        { Pose: 4 } => EnemyFrameId.FaceBlockSamusRight2,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the BlueBrinstarFaceBlockFrame programs show."),
    };
}
