using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A glass shard's or sparkle's typed animation.</summary>
internal sealed partial class MotherBrainGlassAnimation
    : EnemyProjectileFrameAnimation<MotherBrainGlassSegment, MotherBrainGlassCue>
{
    /// <summary>Creates a <see cref="MotherBrainGlassAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">The animation to play.</param>
    public MotherBrainGlassAnimation(IEnemyProjectileCues<MotherBrainGlassCue> owner, MotherBrainGlassSegment segment)
        : base(segment, MotherBrainGlassAnimations.StartingAt[segment], EnemyProjectileCueRunner<MotherBrainGlassSegment, MotherBrainGlassCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainGlassSegment, EnemyProjectileFrameId, MotherBrainGlassCue> Segments =>
        MotherBrainGlassAnimations.Segments;
}
