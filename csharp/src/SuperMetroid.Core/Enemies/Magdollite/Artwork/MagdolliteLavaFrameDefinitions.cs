using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite.Artwork;

/// <summary>
/// The presentation boundary of the Magdollite's lava programs (#641): the projectile frame each
/// <see cref="MagdolliteLavaFrame"/> pose shows.
/// </summary>
internal static class MagdolliteLavaFrameDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MagdolliteLavaFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MagdolliteLavaFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.MagdolliteLavaFrame00,
            [new(1)] = EnemyProjectileFrameId.MagdolliteLavaFrame01,
        };
}
