using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatSegment segment, IEnemyProjectileCues<EyeDoorSweatCue> cues)
    : EnemyProjectileFrameAnimation<EyeDoorSweatSegment, EyeDoorSweatCue>(EyeDoorSweatAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorSweatSegment, EnemyProjectileFrameId, EyeDoorSweatCue> Segments => EyeDoorSweatAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>> StartingAt => EyeDoorSweatAnimations.StartingAt;
}
