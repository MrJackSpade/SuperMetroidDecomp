using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Cartridge values of Phantoon's part frames that only verification reads.</summary>
internal static class PhantoonPartFramesConstants
{
    /// <summary>The cartridge frame or collision selector <paramref name="frame"/>, one of Phantoon's part frames, shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of Phantoon's part frames.</exception>
    internal static ushort Spritemap(EnemyFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a Phantoon part frame.");

    /// <summary>Each frame by the cartridge frame it shows.</summary>
    private static readonly Dictionary<EnemyFrameId, ushort> Spritemaps = new()
    {
        [EnemyFrameId.PhantoonInvulnerableBody] = PhantoonBg2FrameDefinitionsConstants.InvulnerableBody,
        [EnemyFrameId.PhantoonFullHitboxBody] = (ushort)PhantoonCollisionFrame.BodyFullHitbox,
        [EnemyFrameId.PhantoonEyeHitboxBody] = (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
        [EnemyFrameId.PhantoonEyeOpenFirst] = PhantoonBg2FrameDefinitionsConstants.OpeningEye(0),
        [EnemyFrameId.PhantoonEyeOpenSecond] = PhantoonBg2FrameDefinitionsConstants.OpeningEye(1),
        [EnemyFrameId.PhantoonEyeOpenThird] = PhantoonBg2FrameDefinitionsConstants.OpeningEye(2),
        [EnemyFrameId.PhantoonEyeClosed] = PhantoonBg2FrameDefinitionsConstants.ClosedEye,
        [EnemyFrameId.PhantoonEyeballCentered] = PhantoonBg2FrameDefinitionsConstants.CenteredEye,
        [EnemyFrameId.PhantoonEyeLookingUp] = Gaze(0),
        [EnemyFrameId.PhantoonEyeLookingUpRight] = Gaze(1),
        [EnemyFrameId.PhantoonEyeLookingRight] = Gaze(2),
        [EnemyFrameId.PhantoonEyeLookingDownRight] = Gaze(3),
        [EnemyFrameId.PhantoonEyeLookingDown] = Gaze(4),
        [EnemyFrameId.PhantoonEyeLookingDownLeft] = Gaze(5),
        [EnemyFrameId.PhantoonEyeLookingLeft] = Gaze(6),
        [EnemyFrameId.PhantoonEyeLookingUpLeft] = Gaze(7),
        [EnemyFrameId.PhantoonTentaclesFirst] = PhantoonBg2FrameDefinitionsConstants.TentaclePose(0),
        [EnemyFrameId.PhantoonTentaclesSecond] = PhantoonBg2FrameDefinitionsConstants.TentaclePose(1),
        [EnemyFrameId.PhantoonTentaclesThird] = PhantoonBg2FrameDefinitionsConstants.TentaclePose(2),
        [EnemyFrameId.PhantoonMouthInitial] = PhantoonBg2FrameDefinitionsConstants.MouthPose(0),
        [EnemyFrameId.PhantoonMouthSpawnFlameSecond] = PhantoonBg2FrameDefinitionsConstants.MouthPose(1),
        [EnemyFrameId.PhantoonMouthSpawnFlameFirst] = PhantoonBg2FrameDefinitionsConstants.MouthPose(2),
    };

    /// <summary>The gaze frame of the eye's gaze direction <paramref name="direction"/>, counted clockwise from up.</summary>
    private static ushort Gaze(int direction) => PhantoonBg2FrameDefinitionsConstants.Gaze(PhantoonGazeDirections.At(direction));
}
