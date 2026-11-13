using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Downward gate's typed animation.</summary>
internal sealed class DownwardGateProjectileAnimation(DownwardGateProjectileSegment segment)
    : EnemyProjectileFrameAnimation<DownwardGateProjectileSegment, DownwardGateProjectileCue>(DownwardGateProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DownwardGateProjectileSegment, EnemyProjectileFrameId, DownwardGateProjectileCue> Segments => DownwardGateProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DownwardGateProjectileSegment, Animation<DownwardGateProjectileSegment>> StartingAt => DownwardGateProjectileAnimations.StartingAt;
}
