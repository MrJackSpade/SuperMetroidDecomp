using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
internal sealed partial class EyeDoorProjectileAnimation
    : EnemyProjectileFrameAnimation<EyeDoorProjectileSegment, EyeDoorProjectileCue>
{
    /// <summary>Creates an <see cref="EyeDoorProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public EyeDoorProjectileAnimation(IEnemyProjectileCues<EyeDoorProjectileCue> owner, EyeDoorProjectileSegment segment)
        : base(segment, EyeDoorProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<EyeDoorProjectileSegment, EyeDoorProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<EyeDoorProjectileSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;
}
