using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Doors.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye door projectile's typed animation.</summary>
internal sealed class EyeDoorProjectileAnimation(EyeDoorProjectileProgram program)
    : EnemyProjectileProgramAnimation<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Programs => EyeDoorProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EyeDoorProjectileFrame frame) =>
        EyeDoorProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
