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
    /// <summary>
    /// The motion a projectile this behaviour is bound to starts with, or null when it starts holding still. The null is the
    /// absence of a motion, not a stand-in for one: a projectile holds a motion only while something moves it, as
    /// <see cref="EnemyProjectile.Motion"/> holds none for every family, so one that holds none is moved by nothing (#2558).
    /// </summary>
    private readonly TMotion? _initial;

    /// <summary>Each of this behaviour's motions a projectile has moved by, created once.</summary>
    private readonly Dictionary<TMotion, EnemyProjectileFamilyMotion<TMotion>> _motions = [];

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
    internal override EnemyProjectileMotion? StartingMotion => _initial is { } motion ? Moving(motion) : null;

    /// <summary><paramref name="motion"/>, one of this behaviour's motions, held through this behaviour (#2571).</summary>
    /// <param name="motion">The motion.</param>
    /// <remarks>Each motion is created once, so a projectile that moves by it holds the same object every time.</remarks>
    internal EnemyProjectileFamilyMotion<TMotion> Moving(TMotion motion)
    {
        if (!_motions.TryGetValue(motion, out EnemyProjectileFamilyMotion<TMotion>? moving))
        {
            moving = new EnemyProjectileFamilyMotion<TMotion>(this, motion);
            _motions.Add(motion, moving);
        }
        return moving;
    }

    /// <summary>Moves the projectile one pass by <paramref name="motion"/>, which this behaviour owns.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    internal abstract void Move(EnemyProjectileMotionContext context, TMotion motion);
}
