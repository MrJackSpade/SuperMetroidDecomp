using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatProgram program)
    : EnemyProjectileTypedAnimation<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>(EyeDoorSweatAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Segments => EyeDoorSweatAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorSweatProgram, Animation<EyeDoorSweatProgram>> StartingAt => EyeDoorSweatAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EyeDoorSweatFrame frame) =>
        EyeDoorSweatFrameDefinitions.ShownProjectileFrames[frame];
}
