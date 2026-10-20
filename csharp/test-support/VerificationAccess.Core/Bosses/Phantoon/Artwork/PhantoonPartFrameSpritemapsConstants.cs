namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Cartridge values and helpers of <see cref="PhantoonPartFrameSpritemaps"/> that only verification reads.</summary>
internal static class PhantoonPartFrameSpritemapsConstants
{
    /// <summary>The BG2 frame or collision selector <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(PhantoonPartFrame frame) => frame.Kind switch
    {
        PhantoonPartFrameKind.InvulnerableBody => PhantoonBg2FrameDefinitionsConstants.InvulnerableBody,
        PhantoonPartFrameKind.FullHitboxBody => (ushort)PhantoonCollisionFrame.BodyFullHitbox,
        PhantoonPartFrameKind.EyeHitboxBody => (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
        PhantoonPartFrameKind.OpeningEye => PhantoonBg2FrameDefinitionsConstants.OpeningEye(frame.Pose),
        PhantoonPartFrameKind.ClosedEye => PhantoonBg2FrameDefinitionsConstants.ClosedEye,
        PhantoonPartFrameKind.CenteredEye => PhantoonBg2FrameDefinitionsConstants.CenteredEye,
        PhantoonPartFrameKind.Gaze => PhantoonBg2FrameDefinitionsConstants.Gaze(PhantoonGazeDirections.At(frame.Pose)),
        PhantoonPartFrameKind.Tentacle => PhantoonBg2FrameDefinitionsConstants.TentaclePose(frame.Pose),
        PhantoonPartFrameKind.Mouth => PhantoonBg2FrameDefinitionsConstants.MouthPose(frame.Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame.Kind, "Undefined Phantoon part frame."),
    };
}
