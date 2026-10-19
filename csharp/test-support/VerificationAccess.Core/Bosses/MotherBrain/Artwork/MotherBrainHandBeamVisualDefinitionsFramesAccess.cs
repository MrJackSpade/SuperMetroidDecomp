using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Frame lists and counts of <see cref="MotherBrainHandBeamVisualDefinitions"/> that only verification read.</summary>
internal static class MotherBrainHandBeamVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainHandBeamVisualDefinitions"/>.</summary>
    extension(MotherBrainHandBeamVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order: the stages in turn.</summary>
        internal static IReadOnlyList<MotherBrainHandBeamFrame> ProjectileFrames =>
        [
            .. Enumerable.Range(0, MotherBrainHandBeamAnimations.StageCount).SelectMany(stage =>
                Enumerable.Range(0, MotherBrainHandBeamAnimationsConstants.FramesPerStage).Select(phase => new MotherBrainHandBeamFrame(stage, phase))),
        ];

        /// <summary>Distinct frames: seven in each of three stages.</summary>
        internal static int ProjectileFrameCount => MotherBrainHandBeamAnimations.StageCount * MotherBrainHandBeamAnimationsConstants.FramesPerStage;
    }
}
