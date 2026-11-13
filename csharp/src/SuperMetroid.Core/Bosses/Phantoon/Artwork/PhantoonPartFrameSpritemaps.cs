using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>The presentation boundary of Phantoon's part frames: the background frame or collision selector each one shows.</summary>
internal static class PhantoonPartFrameSpritemaps
{

    /// <summary>The installed frame each <see cref="PhantoonPartFrame"/> shows.</summary>
    internal static EnemyFrameTable<PhantoonPartFrame> ShownFrames { get; } = new()
    {
        { new(Kind: PhantoonPartFrameKind.CenteredEye, Pose: 0), EnemyFrameId.PhantoonEyeballCentered },
        { new(Kind: PhantoonPartFrameKind.ClosedEye, Pose: 0), EnemyFrameId.PhantoonEyeClosed },
        { new(Kind: PhantoonPartFrameKind.EyeHitboxBody, Pose: 0), EnemyFrameId.PhantoonEyeHitboxBody },
        { new(Kind: PhantoonPartFrameKind.FullHitboxBody, Pose: 0), EnemyFrameId.PhantoonFullHitboxBody },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 0), EnemyFrameId.PhantoonEyeLookingUp },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 1), EnemyFrameId.PhantoonEyeLookingUpRight },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 2), EnemyFrameId.PhantoonEyeLookingRight },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 3), EnemyFrameId.PhantoonEyeLookingDownRight },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 4), EnemyFrameId.PhantoonEyeLookingDown },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 5), EnemyFrameId.PhantoonEyeLookingDownLeft },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 6), EnemyFrameId.PhantoonEyeLookingLeft },
        { new(Kind: PhantoonPartFrameKind.Gaze, Pose: 7), EnemyFrameId.PhantoonEyeLookingUpLeft },
        { new(Kind: PhantoonPartFrameKind.InvulnerableBody, Pose: 0), EnemyFrameId.PhantoonInvulnerableBody },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 0), EnemyFrameId.PhantoonMouthInitial },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 1), EnemyFrameId.PhantoonMouthSpawnFlameSecond },
        { new(Kind: PhantoonPartFrameKind.Mouth, Pose: 2), EnemyFrameId.PhantoonMouthSpawnFlameFirst },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 0), EnemyFrameId.PhantoonEyeOpenFirst },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 1), EnemyFrameId.PhantoonEyeOpenSecond },
        { new(Kind: PhantoonPartFrameKind.OpeningEye, Pose: 2), EnemyFrameId.PhantoonEyeOpenThird },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 0), EnemyFrameId.PhantoonTentaclesFirst },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 1), EnemyFrameId.PhantoonTentaclesSecond },
        { new(Kind: PhantoonPartFrameKind.Tentacle, Pose: 2), EnemyFrameId.PhantoonTentaclesThird },
    };
}
