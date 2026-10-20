namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>Frame lists and counts of <see cref="PhantoonFlameVisualDefinitions"/> that only verification read.</summary>
internal static class PhantoonFlameVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="PhantoonFlameVisualDefinitions"/>.</summary>
    extension(PhantoonFlameVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order.</summary>
        internal static IReadOnlyList<PhantoonFlameFrame> ProjectileFrames =>
        [
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.DestroyableIdle, PhantoonFlameAnimations.LoopPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.CasualHitGround, 1),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.CasualBouncing, PhantoonFlameAnimations.LoopPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.CasualResting, PhantoonFlameAnimations.RestingPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.Dying, PhantoonFlameAnimations.DyingPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.RainImpact, 1),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.CasualFalling, PhantoonFlameAnimationsConstants.FallingPhaseCount * PhantoonFlameAnimations.FallingPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.StartingFlame, PhantoonFlameAnimations.LoopPoseCount),
            .. PhantoonFlameVisualDefinitions.Frames(PhantoonFlameFrameKind.DestroyableShot, PhantoonFlameAnimations.ShotPoseCount),
        ];

        /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
        private static IEnumerable<PhantoonFlameFrame> Frames(PhantoonFlameFrameKind kind, int count) =>
            Enumerable.Range(0, count).Select(phase => new PhantoonFlameFrame(kind, phase));

        /// <summary>Distinct frames, in installed-artwork order.</summary>
        internal static int ProjectileFrameCount => 31;
    }
}
