using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
internal sealed partial class SpacePirateProjectileAnimation
    : EnemyProjectileFrameAnimation<SpacePirateProjectileSegment, SpacePirateProjectileCue>
{
    /// <summary>Creates a <see cref="SpacePirateProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public SpacePirateProjectileAnimation(IEnemyProjectileCues<SpacePirateProjectileCue> owner, SpacePirateProjectileSegment segment)
        : base(segment, SpacePirateProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<SpacePirateProjectileSegment, SpacePirateProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;
}
