using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Frame lists and counts of <see cref="MotherBrainGlassVisualDefinitions"/> that only verification read.</summary>
internal static class MotherBrainGlassVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="MotherBrainGlassVisualDefinitions"/>.</summary>
    extension(MotherBrainGlassVisualDefinitions)
    {
        /// <summary>Every frame in installed-artwork order: the shard groups in turn, then the sparkle.</summary>
        internal static IReadOnlyList<MotherBrainGlassFrame> ProjectileFrames =>
        [
            .. Enum.GetValues<MotherBrainGlassProgram>().Where(program => program != MotherBrainGlassProgram.Sparkle)
                .SelectMany(program => Enumerable.Range(0, MotherBrainGlassAnimations.ShardPhaseCount)
                    .Select(phase => new MotherBrainGlassFrame(program, phase))),
            .. Enumerable.Range(0, MotherBrainGlassAnimations.SparklePhaseCount)
                .Select(phase => new MotherBrainGlassFrame(MotherBrainGlassProgram.Sparkle, phase)),
        ];

        /// <summary>Distinct frames: eight per shard group, then the sparkle's four.</summary>
        internal static int ProjectileFrameCount => 8 * MotherBrainGlassAnimations.ShardPhaseCount + MotherBrainGlassAnimations.SparklePhaseCount;
    }
}
