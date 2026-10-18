using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The presentation boundary of Phantoon's part frames: the BG2 frame or collision selector each one shows.</summary>
internal static class PhantoonPartFrameSpritemaps
{
    /// <summary>The BG2 frame or collision selector <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PhantoonPartFrame frame) => frame.Kind switch
    {
        PhantoonPartFrameKind.InvulnerableBody => PhantoonBg2FrameDefinitions.InvulnerableBody,
        PhantoonPartFrameKind.FullHitboxBody => (ushort)PhantoonCollisionFrame.BodyFullHitbox,
        PhantoonPartFrameKind.EyeHitboxBody => (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
        PhantoonPartFrameKind.OpeningEye => PhantoonBg2FrameDefinitions.OpeningEye(frame.Pose),
        PhantoonPartFrameKind.ClosedEye => PhantoonBg2FrameDefinitions.ClosedEye,
        PhantoonPartFrameKind.CenteredEye => PhantoonBg2FrameDefinitions.CenteredEye,
        PhantoonPartFrameKind.Gaze => PhantoonBg2FrameDefinitions.Gaze(PhantoonGazeDirections.At(frame.Pose)),
        PhantoonPartFrameKind.Tentacle => PhantoonBg2FrameDefinitions.TentaclePose(frame.Pose),
        PhantoonPartFrameKind.Mouth => PhantoonBg2FrameDefinitions.MouthPose(frame.Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame.Kind, "Undefined Phantoon part frame."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(PhantoonPartFrame frame) => frame switch
    {
        { Kind: PhantoonPartFrameKind.CenteredEye, Pose: 0 } => EnemyFrameId.PhantoonBg2DF23,
        { Kind: PhantoonPartFrameKind.ClosedEye, Pose: 0 } => EnemyFrameId.PhantoonBg2DEFB,
        { Kind: PhantoonPartFrameKind.EyeHitboxBody, Pose: 0 } => EnemyFrameId.PhantoonBg2DEF1,
        { Kind: PhantoonPartFrameKind.FullHitboxBody, Pose: 0 } => EnemyFrameId.PhantoonBg2DEE7,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 0 } => EnemyFrameId.PhantoonBg2DF2D,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 1 } => EnemyFrameId.PhantoonBg2DF73,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 2 } => EnemyFrameId.PhantoonBg2DF4B,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 3 } => EnemyFrameId.PhantoonBg2DF5F,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 4 } => EnemyFrameId.PhantoonBg2DF37,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 5 } => EnemyFrameId.PhantoonBg2DF55,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 6 } => EnemyFrameId.PhantoonBg2DF41,
        { Kind: PhantoonPartFrameKind.Gaze, Pose: 7 } => EnemyFrameId.PhantoonBg2DF69,
        { Kind: PhantoonPartFrameKind.InvulnerableBody, Pose: 0 } => EnemyFrameId.PhantoonBg2DEDD,
        { Kind: PhantoonPartFrameKind.Mouth, Pose: 0 } => EnemyFrameId.PhantoonBg2DFE9,
        { Kind: PhantoonPartFrameKind.Mouth, Pose: 1 } => EnemyFrameId.PhantoonBg2DFF3,
        { Kind: PhantoonPartFrameKind.Mouth, Pose: 2 } => EnemyFrameId.PhantoonBg2DFFD,
        { Kind: PhantoonPartFrameKind.OpeningEye, Pose: 0 } => EnemyFrameId.PhantoonBg2DF05,
        { Kind: PhantoonPartFrameKind.OpeningEye, Pose: 1 } => EnemyFrameId.PhantoonBg2DF0F,
        { Kind: PhantoonPartFrameKind.OpeningEye, Pose: 2 } => EnemyFrameId.PhantoonBg2DF19,
        { Kind: PhantoonPartFrameKind.Tentacle, Pose: 0 } => EnemyFrameId.PhantoonBg2DFB3,
        { Kind: PhantoonPartFrameKind.Tentacle, Pose: 1 } => EnemyFrameId.PhantoonBg2DFC5,
        { Kind: PhantoonPartFrameKind.Tentacle, Pose: 2 } => EnemyFrameId.PhantoonBg2DFD7,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the PhantoonPartFrame programs show."),
    };
}
