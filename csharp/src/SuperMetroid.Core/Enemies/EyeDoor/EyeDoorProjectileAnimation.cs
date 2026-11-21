using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class EyeDoorProjectileAnimation(IEnemyProjectileCues<EyeDoorProjectileCue> owner, EyeDoorProjectileSegment segment)
    : EnemyProjectileFrameAnimation<EyeDoorProjectileSegment, EyeDoorProjectileCue>(segment, EyeDoorProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<EyeDoorProjectileSegment, EyeDoorProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorProjectileSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;
}
