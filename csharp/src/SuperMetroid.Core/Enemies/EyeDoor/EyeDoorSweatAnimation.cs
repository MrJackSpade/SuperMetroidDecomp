using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatSegment segment)
    : EnemyProjectileFrameAnimation<EyeDoorSweatSegment, EyeDoorProjectileCue>(EyeDoorSweatAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorSweatSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorSweatAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>> StartingAt => EyeDoorSweatAnimations.StartingAt;
}
