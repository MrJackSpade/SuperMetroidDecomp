namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself one way (<see cref="SingleMotionProjectileBehavior"/>) and keeps its own
/// state on each of its projectiles (#2571), in the table it creates: its spawner gives each projectile its entry, and its
/// motion reads the entry back typed.
/// </summary>
/// <typeparam name="TState">What the kind keeps on each of its projectiles.</typeparam>
/// <param name="move">Moves the projectile one pass by the kind's one motion.</param>
/// <param name="initial">The kind's one motion when the projectile moves from its spawn, or null when it holds still until started.</param>
/// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
internal sealed class StatefulSingleMotionProjectileBehavior<TState>(
    Action<EnemyProjectileMotionContext> move,
    SingleMotion? initial = SingleMotion.Moving,
    Func<EnemyProjectileAnimation>? shot = null)
    : SingleMotionProjectileBehavior(move, initial, shot)
    where TState : EnemyProjectileState
{
    /// <summary>What the kind keeps on each of its projectiles, given by its spawner.</summary>
    internal EnemyProjectileStates<TState> States { get; } = new();
}
