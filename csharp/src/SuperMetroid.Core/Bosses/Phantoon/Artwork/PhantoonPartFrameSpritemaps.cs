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

    /// <summary>The installed frame each <see cref="PhantoonPartFrame"/> shows.</summary>
    internal static EnemyFrameTable<PhantoonPartFrame> ShownFrames { get; } = new()
    {
        { new(Kind: PhantoonPartFrameKind.CenteredEye, Pose: 0), EnemyFrameId.PhantoonBg2DF23 },
        { new(Kind: PhantoonPartFrameKind.ClosedEye, Pose: 0), EnemyFrameId.PhantoonBg2DEFB },
        { new(Kind: PhantoonPartFrameKind.EyeHitboxBody, Pose: 0), EnemyFrameId.PhantoonBg2DEF1 },
        { new(Kind: PhantoonPartFrameKind.FullHitboxBody, Pose: 0), EnemyFrameId.PhantoonBg2DEE7 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 0), EnemyFrameId.PhantoonBg2DF2D },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 1), EnemyFrameId.PhantoonBg2DF73 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 2), EnemyFrameId.PhantoonBg2DF4B },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 3), EnemyFrameId.PhantoonBg2DF5F },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 4), EnemyFrameId.PhantoonBg2DF37 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 5), EnemyFrameId.PhantoonBg2DF55 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 6), EnemyFrameId.PhantoonBg2DF41 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 7), EnemyFrameId.PhantoonBg2DF69 },
        { new(Kind: PhantoonPartFrameKind.InvulnerableBody, Pose: 0), EnemyFrameId.PhantoonBg2DEDD },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 0), EnemyFrameId.PhantoonBg2DFE9 },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 1), EnemyFrameId.PhantoonBg2DFF3 },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 2), EnemyFrameId.PhantoonBg2DFFD },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 0), EnemyFrameId.PhantoonBg2DF05 },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 1), EnemyFrameId.PhantoonBg2DF0F },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 2), EnemyFrameId.PhantoonBg2DF19 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 0), EnemyFrameId.PhantoonBg2DFB3 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 1), EnemyFrameId.PhantoonBg2DFC5 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 2), EnemyFrameId.PhantoonBg2DFD7 },
    };
}
