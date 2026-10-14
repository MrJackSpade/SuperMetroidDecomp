using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed class DraygonProjectileAnimation(DraygonProjectileProgram program)
    : EnemyProjectileProgramAnimation<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> Programs =>
        DraygonProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(DraygonProjectileFrame frame) => DraygonProjectileVisualDefinitions.PresentationKey(frame);
}
