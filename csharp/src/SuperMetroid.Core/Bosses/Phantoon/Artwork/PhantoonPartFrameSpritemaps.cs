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
}
