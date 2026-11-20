namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself one way (#1760, #1843): it starts moving, or holds still until its
/// spawner or a cue starts it, and moves by its one motion until a shot, a cue or its deletion stops it. A family whose
/// animations publish cues of its own derives from it to handle them.
/// </summary>
internal class SingleMotionProjectileBehavior : EnemyProjectileMotionBehavior<SingleMotion>
{
    /// <summary>Moves the projectile one pass by the kind's one motion.</summary>
    private readonly Action<EnemyProjectileMotionContext> _move;

    /// <summary>A kind that moves by <paramref name="move"/>.</summary>
    /// <param name="move">Moves the projectile one pass by the kind's one motion.</param>
    /// <param name="initial">The kind's one motion when the projectile moves from its spawn, or null when it holds still until started.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    internal SingleMotionProjectileBehavior(
        Action<EnemyProjectileMotionContext> move,
        SingleMotion? initial = SingleMotion.Moving,
        Func<EnemyProjectileAnimation>? shot = null)
        : base(initial, shot) => _move = Ensure.NotNull(move);

    /// <summary>Starts <paramref name="projectile"/> moving by its kind's one motion.</summary>
    /// <param name="projectile">A projectile this behaviour is bound to.</param>
    internal void StartMoving(EnemyProjectile projectile) => StartMotion(projectile, SingleMotion.Moving);

    /// <inheritdoc/>
    internal sealed override void Move(EnemyProjectileMotionContext context, SingleMotion motion) => _move(context);
}
