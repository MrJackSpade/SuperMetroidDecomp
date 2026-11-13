using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatProgram program)
    : EnemyProjectileFrameAnimation<EyeDoorSweatProgram, EnemyProjectileCommonCue>(EyeDoorSweatAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorSweatProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => EyeDoorSweatAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorSweatProgram, Animation<EyeDoorSweatProgram>> StartingAt => EyeDoorSweatAnimations.StartingAt;
}
