using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
internal sealed class EyeDoorProjectileAnimation(EyeDoorProjectileSegment segment)
    : EnemyProjectileFrameAnimation<EyeDoorProjectileSegment, EyeDoorProjectileCue>(EyeDoorProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorProjectileSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorProjectileSegment, Animation<EyeDoorProjectileSegment>> StartingAt => EyeDoorProjectileAnimations.StartingAt;
}
