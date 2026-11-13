namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The shared delete reaction: the shot or touch animation of every projectile
/// without a typed reaction of its own. Its single cue deletes the projectile on the pass after the reaction starts,
/// as the game's animation does.
/// </summary>
internal sealed class EnemyProjectileDeleteAnimation : EnemyProjectileAnimation
{
    /// <summary>The one shared reaction; it keeps no state.</summary>
    internal static EnemyProjectileDeleteAnimation Instance { get; } = new();

    /// <summary>Use <see cref="Instance"/>.</summary>
    private EnemyProjectileDeleteAnimation()
    {
    }

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameStep NextFrame(EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers)
    {
        context.Projectile.Delete();
        return EnemyProjectileFrameStep.Rests;
    }
}
