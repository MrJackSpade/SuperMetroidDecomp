using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class EyeDoorSweatAnimation(IEnemyProjectileCues<EyeDoorProjectileCue> owner, EyeDoorSweatSegment segment)
    : EnemyProjectileFrameAnimation<EyeDoorSweatSegment, EyeDoorProjectileCue>(segment, EyeDoorSweatAnimations.StartingWith(segment), EnemyProjectileCueRunner<EyeDoorSweatSegment, EyeDoorProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorSweatSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorSweatAnimations.Segments;
}
