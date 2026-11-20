using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class DownwardGateProjectileAnimation(IEnemyProjectileCues<DownwardGateProjectileCue> owner, DownwardGateProjectileSegment segment)
    : EnemyProjectileFrameAnimation<DownwardGateProjectileSegment, DownwardGateProjectileCue>(DownwardGateProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<DownwardGateProjectileSegment, DownwardGateProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>>> StartingAt => DownwardGateProjectileAnimations.StartingAt;
}
