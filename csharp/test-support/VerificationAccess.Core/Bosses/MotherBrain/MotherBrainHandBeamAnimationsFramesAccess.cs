using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The installed frames of <see cref="MotherBrainHandBeamAnimations"/> in installed order, which only verification reads.</summary>
internal static class MotherBrainHandBeamAnimationsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainHandBeamAnimations"/>.</summary>
    extension(MotherBrainHandBeamAnimations)
    {
        /// <summary>Every frame in installed-artwork order: the stages' seven frames in turn.</summary>
        internal static IReadOnlyList<EnemyProjectileFrameId> ProjectileFrames => [.. MotherBrainHandBeamAnimations.Segments.Frames];
    }
}
