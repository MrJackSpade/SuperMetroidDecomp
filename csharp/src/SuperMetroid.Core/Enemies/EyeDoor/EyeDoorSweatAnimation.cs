using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed partial class EyeDoorSweatAnimation
    : EnemyProjectileFrameAnimation<EyeDoorSweatSegment, EyeDoorProjectileCue>
{
    /// <summary>Creates an <see cref="EyeDoorSweatAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public EyeDoorSweatAnimation(IEnemyProjectileCues<EyeDoorProjectileCue> owner, EyeDoorSweatSegment segment)
        : base(segment, EyeDoorSweatAnimations.StartingAt[segment], EnemyProjectileCueRunner<EyeDoorSweatSegment, EyeDoorProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<EyeDoorSweatSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorSweatAnimations.Segments;
}
