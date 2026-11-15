using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The installed frames of <see cref="MotherBrainTurretAnimations"/> in installed order, which only verification reads.</summary>
internal static class MotherBrainTurretAnimationsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainTurretAnimations"/>.</summary>
    extension(MotherBrainTurretAnimations)
    {
        /// <summary>Every frame in installed-artwork order: the turret poses in ring order, the bullet poses, then the smoke.</summary>
        internal static IReadOnlyList<EnemyProjectileFrameId> ProjectileFrames => [.. MotherBrainTurretAnimations.Segments.Frames];
    }
}
