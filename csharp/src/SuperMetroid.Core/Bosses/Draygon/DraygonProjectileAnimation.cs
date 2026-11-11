using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed class DraygonProjectileAnimation(DraygonProjectileProgram program)
    : EnemyProjectileTypedAnimation<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue>(DraygonProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DraygonProjectileProgram, DraygonProjectileFrame, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DraygonProjectileProgram, Animation<DraygonProjectileProgram>> StartingAt =>
        DraygonProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DraygonProjectileFrame frame) =>
        DraygonProjectileVisualDefinitions.ShownProjectileFrames[frame];
}
