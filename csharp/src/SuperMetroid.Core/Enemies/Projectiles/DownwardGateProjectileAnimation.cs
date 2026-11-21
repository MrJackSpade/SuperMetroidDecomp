using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
internal sealed partial class DownwardGateProjectileAnimation
    : EnemyProjectileFrameAnimation<DownwardGateProjectileSegment, DownwardGateProjectileCue>
{
    /// <summary>Creates a <see cref="DownwardGateProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public DownwardGateProjectileAnimation(IEnemyProjectileCues<DownwardGateProjectileCue> owner, DownwardGateProjectileSegment segment)
        : base(segment, DownwardGateProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<DownwardGateProjectileSegment, DownwardGateProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;
}
