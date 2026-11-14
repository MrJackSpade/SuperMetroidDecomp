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
            .. Enumerable.Range(0, MotherBrainTurretDefinitions.Ring.Count).Select(position => new MotherBrainTurretFrame(MotherBrainTurretSegment.TurretPose, position)),
            .. Enumerable.Range(0, MotherBrainTurretDefinitions.Ring.Count).Select(position => new MotherBrainTurretFrame(MotherBrainTurretSegment.BulletPose, position)),
            .. Enumerable.Range(0, MotherBrainTurretAnimations.ImpactPhaseCount).Select(phase => new MotherBrainTurretFrame(MotherBrainTurretSegment.BulletImpact, phase)),
        ];

        /// <summary>Distinct frames: eight turret poses, eight bullet poses and five smoke frames.</summary>
        internal static int ProjectileFrameCount => 2 * MotherBrainTurretDefinitions.Ring.Count + MotherBrainTurretAnimations.ImpactPhaseCount;
    }
}
