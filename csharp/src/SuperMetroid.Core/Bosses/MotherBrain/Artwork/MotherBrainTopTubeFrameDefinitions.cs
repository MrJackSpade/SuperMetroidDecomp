using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the ceiling-tube programs (#641): the projectile frame each tube's pose shows.
/// </summary>
internal static class MotherBrainTopTubeFrameDefinitions
{
    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeSegment, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainTopTubeSegment, EnemyProjectileFrameId>
        {
            [MotherBrainTopTubeSegment.TopRight] = EnemyProjectileFrameId.MotherBrainTopTubeTopRight,
            [MotherBrainTopTubeSegment.TopLeft] = EnemyProjectileFrameId.MotherBrainTopTubeTopLeft,
            [MotherBrainTopTubeSegment.TopMiddleLeft] = EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleLeft,
            [MotherBrainTopTubeSegment.TopMiddleRight] = EnemyProjectileFrameId.MotherBrainTopTubeTopMiddleRight,
        };
}
