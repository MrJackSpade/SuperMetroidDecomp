namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself under one frame behaviour of its own (#1760), besides holding still. A
/// family whose animations publish cues of its own derives from it to handle them.
/// </summary>
internal class SingleMotionProjectileBehavior : EnemyProjectileBehavior
{
    /// <summary>The kind's one frame behaviour.</summary>
    private readonly EnemyProjectileFrameBehavior _motion;

    /// <summary>Moves the projectile one pass under <see cref="_motion"/>.</summary>
    private readonly Action<EnemyProjectileFrameBehaviorContext> _move;

    /// <summary>A kind that moves under <paramref name="motion"/> by <paramref name="move"/>.</summary>
    /// <param name="motion">The kind's one frame behaviour.</param>
    /// <param name="move">Moves the projectile one pass under that frame behaviour.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    internal SingleMotionProjectileBehavior(
        EnemyProjectileFrameBehavior motion,
        Action<EnemyProjectileFrameBehaviorContext> move,
        Func<EnemyProjectileAnimation>? shot = null)
        : base(shot)
    {
        _motion = motion;
        _move = Ensure.NotNull(move);
    }

    /// <inheritdoc/>
    private protected sealed override void Move(EnemyProjectileFrameBehaviorContext context)
    {
        if (context.Projectile.FrameBehavior == _motion)
            _move(context);
        else
            base.Move(context);
    }
}
