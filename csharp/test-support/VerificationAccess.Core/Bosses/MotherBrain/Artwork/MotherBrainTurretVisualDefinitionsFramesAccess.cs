using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Frame lists and counts of <see cref="MotherBrainTurretVisualDefinitions"/> that only verification read.</summary>
internal static class MotherBrainTurretVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainTurretVisualDefinitions"/>.</summary>
    extension(MotherBrainTurretVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order: turret poses, bullet poses, then the smoke.</summary>
        internal static IReadOnlyList<MotherBrainTurretFrame> ProjectileFrames =>
        [
            .. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretFrame(MotherBrainTurretProgram.TurretPose, (int)direction)),
            .. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretFrame(MotherBrainTurretProgram.BulletPose, (int)direction)),
            .. Enumerable.Range(0, MotherBrainTurretAnimations.ImpactPhaseCount).Select(phase => new MotherBrainTurretFrame(MotherBrainTurretProgram.BulletImpact, phase)),
        ];

        /// <summary>Distinct frames: eight turret poses, eight bullet poses and five smoke frames.</summary>
        internal static int ProjectileFrameCount => 2 * MotherBrainTurretDefinitions.DirectionCount + MotherBrainTurretAnimations.ImpactPhaseCount;
    }
}
