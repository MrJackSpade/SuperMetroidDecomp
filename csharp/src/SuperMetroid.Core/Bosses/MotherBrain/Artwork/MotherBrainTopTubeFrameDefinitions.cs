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
            [MotherBrainTopTubeProgram.TopRight] = EnemyProjectileFrameId.MotherBrainTopTubeFrame00,
            [MotherBrainTopTubeProgram.TopLeft] = EnemyProjectileFrameId.MotherBrainTopTubeFrame01,
            [MotherBrainTopTubeProgram.TopMiddleLeft] = EnemyProjectileFrameId.MotherBrainTopTubeFrame02,
            [MotherBrainTopTubeProgram.TopMiddleRight] = EnemyProjectileFrameId.MotherBrainTopTubeFrame03,
        };
}
