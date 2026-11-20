using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileSegment segment, IEnemyProjectileCues<DownwardGateProjectileCue> cues)
    : EnemyProjectileFrameAnimation<DownwardGateProjectileSegment, DownwardGateProjectileCue>(DownwardGateProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>> StartingAt => DownwardGateProjectileAnimations.StartingAt;
}
