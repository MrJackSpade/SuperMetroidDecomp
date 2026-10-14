using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

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
