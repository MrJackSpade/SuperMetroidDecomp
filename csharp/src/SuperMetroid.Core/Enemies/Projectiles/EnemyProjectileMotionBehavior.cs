namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself (#1843): it owns the motions <typeparamref name="TMotion"/> names, starts
/// each projectile it is bound to in the motion it was built with, and moves it each pass by the motion it started. Each
/// motion is started through the behaviour that owns it (<see cref="StartMotion"/>) and reaches only that behaviour
/// (#2554); a shot that destroys the projectile, or its deletion, stops it.
/// </summary>
/// <typeparam name="TMotion">The behaviour's own motions.</typeparam>
internal abstract class EnemyProjectileMotionBehavior<TMotion> : EnemyProjectileBehavior
    where TMotion : struct, Enum
{
    /// <summary>The motion a projectile this behaviour is bound to starts with, or null when it starts holding still.</summary>
    private readonly TMotion? _initial;

    /// <summary>A kind that starts in <paramref name="initial"/> and reacts to shots and touch by the factories' animations.</summary>
    /// <param name="initial">The motion the projectile starts with, or null when it starts holding still.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    private protected EnemyProjectileMotionBehavior(
        TMotion? initial = null,
        Func<EnemyProjectileAnimation>? shot = null,
        Func<EnemyProjectileAnimation>? touch = null)
        : base(shot, touch) => _initial = initial;

    /// <inheritdoc/>
    internal sealed override EnemyProjectileMotion? StartingMotion => _initial is { } motion ? new Moving(this, motion) : null;

    /// <summary>Moves <paramref name="projectile"/>, bound to this behaviour, by <paramref name="motion"/> from its next pass.</summary>
    /// <param name="projectile">A projectile this behaviour is bound to.</param>
    /// <param name="motion">One of this behaviour's motions.</param>
    internal void StartMotion(EnemyProjectile projectile, TMotion motion) => Ensure.NotNull(projectile).StartMotion(new Moving(this, motion));

    /// <summary>Whether <paramref name="projectile"/> moves by <paramref name="motion"/>, one of this behaviour's motions.</summary>
    /// <param name="projectile">A projectile this behaviour is bound to.</param>
    /// <param name="motion">One of this behaviour's motions.</param>
    internal static bool MovesBy(EnemyProjectile projectile, TMotion motion) =>
        Ensure.NotNull(projectile).Motion is Moving moving && EqualityComparer<TMotion>.Default.Equals(moving.Motion, motion);

    /// <summary>Moves the projectile one pass by <paramref name="motion"/>, which this behaviour owns.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    private protected abstract void Move(EnemyProjectileMotionContext context, TMotion motion);

    /// <summary>One of the behaviour's motions, bound with the behaviour that runs it.</summary>
    /// <param name="owner">The behaviour that owns and runs the motion.</param>
    /// <param name="motion">The motion.</param>
    private sealed class Moving(EnemyProjectileMotionBehavior<TMotion> owner, TMotion motion) : EnemyProjectileMotion
    {
        /// <summary>The motion.</summary>
        internal TMotion Motion => motion;

        /// <inheritdoc/>
        internal override void Run(EnemyProjectileMotionContext context) => owner.Move(context, motion);
    }
}
