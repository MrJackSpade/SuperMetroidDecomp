using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The installed frames of <see cref="MotherBrainGlassAnimations"/> in installed order, which only verification reads.</summary>
internal static class MotherBrainGlassAnimationsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainGlassAnimations"/>.</summary>
    extension(MotherBrainGlassAnimations)
    {
        /// <summary>Every frame in installed-artwork order: each shard spin's eight in turn order, then the sparkle's four.</summary>
        internal static IReadOnlyList<EnemyProjectileFrameId> ProjectileFrames => [.. MotherBrainGlassAnimations.Segments.Frames];
    }
}
