namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// One motion a projectile behaviour owns as its own object (#2571): the behaviour creates each of its motions once, a
/// projectile it is bound to moves by whichever it holds, and the motion runs itself, so no switch over a family's motions
/// chooses how the projectile moves.
/// </summary>
internal sealed partial class EnemyProjectileOwnedMotion : EnemyProjectileMotion
{
    /// <summary>The motion's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Moves the projectile one pass by this motion.</summary>
    private readonly Action<EnemyProjectileMotionContext> _move;

    /// <summary>A motion named <paramref name="name"/> that moves the projectile by <paramref name="move"/>.</summary>
    /// <param name="name">The motion's name, for diagnostics.</param>
    /// <param name="move">Moves the projectile one pass by this motion.</param>
    internal EnemyProjectileOwnedMotion(string name, Action<EnemyProjectileMotionContext> move)
    {
        _name = Ensure.NotNull(name);
        _move = Ensure.NotNull(move);
    }

    /// <inheritdoc/>
    internal override void Move(EnemyProjectileMotionContext context) => _move(context);

    /// <summary>Names the motion.</summary>
    public override string ToString() => _name;
}
