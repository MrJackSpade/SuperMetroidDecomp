using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Doors.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye-door sweat drop's typed animation.</summary>
internal sealed class EyeDoorSweatAnimation(EyeDoorSweatProgram program)
    : EnemyProjectileProgramAnimation<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Programs => EyeDoorSweatAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(EyeDoorSweatFrame frame) =>
        EnemyProjectileFramePresentation.Installed(EyeDoorSweatFrameDefinitions.PresentationKey(frame));
}
