using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;

namespace SuperMetroid.Core.Enemies.EyeDoor;

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
