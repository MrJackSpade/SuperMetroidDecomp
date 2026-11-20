using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A Pirate/Mother Brain laser's or Ninja Pirate claw's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SpacePirateProjectileAnimation(SpacePirateProjectileSegment segment, IEnemyProjectileCues<SpacePirateProjectileCue> cues)
    : EnemyProjectileFrameAnimation<SpacePirateProjectileSegment, SpacePirateProjectileCue>(SpacePirateProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments => SpacePirateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt => SpacePirateProjectileAnimations.StartingAt;
}
