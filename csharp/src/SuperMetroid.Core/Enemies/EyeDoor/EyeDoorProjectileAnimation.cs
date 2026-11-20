using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class EyeDoorProjectileAnimation(EyeDoorProjectileSegment segment, IEnemyProjectileCues<EyeDoorProjectileCue> cues)
    : EnemyProjectileFrameAnimation<EyeDoorProjectileSegment, EyeDoorProjectileCue>(EyeDoorProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorProjectileSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorProjectileSegment, Animation<EyeDoorProjectileSegment>> StartingAt => EyeDoorProjectileAnimations.StartingAt;
}
