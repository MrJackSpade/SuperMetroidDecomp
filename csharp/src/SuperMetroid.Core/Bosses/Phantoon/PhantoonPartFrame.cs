using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon part frame: what it shows and its pose.</summary>
/// <param name="Kind">What the frame shows.</param>
/// <param name="Pose">The eyelid, gaze, tentacle or mouth pose; zero for the body and the single eye poses.</param>
internal readonly record struct PhantoonPartFrame(PhantoonPartFrameKind Kind, int Pose)
{
    /// <summary>The BG2 frame or collision selector the part shows.</summary>
    internal ushort Spritemap => Kind switch
    {
        PhantoonPartFrameKind.InvulnerableBody => PhantoonBg2FrameDefinitions.InvulnerableBody,
        PhantoonPartFrameKind.FullHitboxBody => (ushort)PhantoonCollisionFrame.BodyFullHitbox,
        PhantoonPartFrameKind.EyeHitboxBody => (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
        PhantoonPartFrameKind.OpeningEye => PhantoonBg2FrameDefinitions.OpeningEye(Pose),
        PhantoonPartFrameKind.ClosedEye => PhantoonBg2FrameDefinitions.ClosedEye,
        PhantoonPartFrameKind.CenteredEye => PhantoonBg2FrameDefinitions.CenteredEye,
        PhantoonPartFrameKind.Gaze => PhantoonBg2FrameDefinitions.Gaze(PhantoonGazeDirections.At(Pose)),
        PhantoonPartFrameKind.Tentacle => PhantoonBg2FrameDefinitions.TentaclePose(Pose),
        PhantoonPartFrameKind.Mouth => PhantoonBg2FrameDefinitions.MouthPose(Pose),
        _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Undefined Phantoon part frame."),
    };
}