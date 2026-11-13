using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Eye door projectile's typed animation.</summary>
internal sealed class EyeDoorProjectileAnimation(EyeDoorProjectileProgram program)
    : EnemyProjectileFrameAnimation<EyeDoorProjectileProgram, EyeDoorProjectileCue>(EyeDoorProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EyeDoorProjectileProgram, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments => EyeDoorProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EyeDoorProjectileProgram, Animation<EyeDoorProjectileProgram>> StartingAt => EyeDoorProjectileAnimations.StartingAt;
}
