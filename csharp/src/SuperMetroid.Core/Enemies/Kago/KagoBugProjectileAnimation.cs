using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileProgram program)
    : EnemyProjectileTypedAnimation<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>(KagoBugProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>> StartingAt => KagoBugProjectileAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(KagoBugProjectileFrame frame) =>
        KagoBugProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
