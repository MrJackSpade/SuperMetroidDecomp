using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the ceiling-tube programs (#641): the projectile frame each tube's pose shows.
/// </summary>
internal static class MotherBrainTopTubeFrameDefinitions
{
    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeProgram, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainTopTubeProgram, EnemyProjectileFrameId>
        {
            [MotherBrainTopTubeProgram.TopRight] = EnemyProjectileFrameId.MotherBrainTopTubeTopRight,
            [MotherBrainTopTubeProgram.TopLeft] = EnemyProjectileFrameId.MotherBrainTopTubeTopLeft,
            [MotherBrainTopTubeProgram.TopMiddleLeft] = EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft,
            [MotherBrainTopTubeProgram.TopMiddleRight] = EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight,
        };
}
