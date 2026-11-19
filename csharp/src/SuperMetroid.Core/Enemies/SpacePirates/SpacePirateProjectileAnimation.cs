using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class SpacePirateProjectileAnimation(IEnemyProjectileCues<SpacePirateProjectileCue> owner, SpacePirateProjectileSegment segment)
    : EnemyProjectileFrameAnimation<SpacePirateProjectileSegment, SpacePirateProjectileCue>(SpacePirateProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SpacePirateProjectileSegment, SpacePirateProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt => SpacePirateProjectileAnimations.StartingAt;
}
