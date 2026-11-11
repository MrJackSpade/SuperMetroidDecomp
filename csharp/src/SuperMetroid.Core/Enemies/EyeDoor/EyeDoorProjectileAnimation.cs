using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
internal sealed class EyeDoorProjectileAnimation(EyeDoorProjectileProgram program)
    : EnemyProjectileTypedAnimation<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue>(EyeDoorProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorProjectileProgram, Animation<EyeDoorProjectileProgram>> StartingAt => EyeDoorProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EyeDoorProjectileFrame frame) =>
        EyeDoorProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
