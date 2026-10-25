using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The presentation boundary of Phantoon's part frames: the BG2 frame or collision selector each one shows.</summary>
internal static class PhantoonPartFrameSpritemaps
{

    /// <summary>The installed frame each <see cref="PhantoonPartFrame"/> shows.</summary>
    internal static EnemyFrameTable<PhantoonPartFrame> ShownFrames { get; } = new()
    {
        { new(Kind: PhantoonPartFrameKind.CenteredEye, Pose: 0), EnemyFrameId.PhantoonCenteredEye0 },
        { new(Kind: PhantoonPartFrameKind.ClosedEye, Pose: 0), EnemyFrameId.PhantoonClosedEye0 },
        { new(Kind: PhantoonPartFrameKind.EyeHitboxBody, Pose: 0), EnemyFrameId.PhantoonEyeHitboxBody0 },
        { new(Kind: PhantoonPartFrameKind.FullHitboxBody, Pose: 0), EnemyFrameId.PhantoonFullHitboxBody0 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 0), EnemyFrameId.PhantoonGaze0 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 1), EnemyFrameId.PhantoonGaze1 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 2), EnemyFrameId.PhantoonGaze2 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 3), EnemyFrameId.PhantoonGaze3 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 4), EnemyFrameId.PhantoonGaze4 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 5), EnemyFrameId.PhantoonGaze5 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 6), EnemyFrameId.PhantoonGaze6 },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 7), EnemyFrameId.PhantoonGaze7 },
        { new(Kind: PhantoonPartFrameKind.InvulnerableBody, Pose: 0), EnemyFrameId.PhantoonInvulnerableBody0 },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 0), EnemyFrameId.PhantoonMouth0 },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 1), EnemyFrameId.PhantoonMouth1 },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 2), EnemyFrameId.PhantoonMouth2 },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 0), EnemyFrameId.PhantoonOpeningEye0 },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 1), EnemyFrameId.PhantoonOpeningEye1 },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 2), EnemyFrameId.PhantoonOpeningEye2 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 0), EnemyFrameId.PhantoonTentacle0 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 1), EnemyFrameId.PhantoonTentacle1 },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 2), EnemyFrameId.PhantoonTentacle2 },
    };
}
