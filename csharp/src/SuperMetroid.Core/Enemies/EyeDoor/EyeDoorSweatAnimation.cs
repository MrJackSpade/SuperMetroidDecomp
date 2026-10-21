using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatProgram program)
    : EnemyProjectileProgramAnimation<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Programs => EyeDoorSweatAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EyeDoorSweatFrame frame) =>
        EyeDoorSweatFrameDefinitions.ShownProjectileFrames[frame];
}
