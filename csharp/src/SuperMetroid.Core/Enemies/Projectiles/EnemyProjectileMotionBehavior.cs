namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself (#1843): it owns the motions <typeparamref name="TMotion"/> names, starts
/// each projectile it is bound to in its kind's motion, and moves it each pass by the motion the projectile holds. Its own
/// cues and code change the motion by calling its methods; a shot that destroys the projectile, or its deletion, stops it.
/// </summary>
/// <typeparam name="TMotion">The family's motions.</typeparam>
internal abstract partial class EnemyProjectileMotionBehavior<TMotion> : EnemyProjectileBehavior
    where TMotion : struct, Enum
{
    /// <summary>
    /// The motion a kind this behaviour is bound to starts with, or null when it starts holding still (#2560): one source,
    /// the one motion every kind starts with or each kind's own. The null is the absence of a motion, not a stand-in for one:
    /// a projectile holds a motion only while something moves it, as <see cref="EnemyProjectile.Motion"/> holds none for
    /// every family, so one that holds none is moved by nothing (#2558).
    /// </summary>
    private readonly TMotion? _initial;

    /// <summary>Each kind's own starting motion, or none when every kind starts with <see cref="_initial"/>.</summary>
    private readonly IReadOnlyDictionary<RoomEnemyProjectileKind, TMotion?>? _startingMotions;

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

    /// <summary>
    /// Kinds that start in the motions <paramref name="startingMotions"/> names for each, and react to shots and touch by
    /// the factories' animations.
    /// </summary>
    /// <param name="startingMotions">
    /// Every kind this behaviour is bound to, with the motion it starts with, or null when it starts holding still.
    /// </param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    private protected EnemyProjectileMotionBehavior(
        IReadOnlyDictionary<RoomEnemyProjectileKind, TMotion?> startingMotions,
        Func<EnemyProjectileAnimation>? shot = null,
        Func<EnemyProjectileAnimation>? touch = null)
        : base(shot, touch)
    {
        _startingMotions = Ensure.NotNull(startingMotions);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">This behaviour does not name the motion <paramref name="kind"/> starts with.</exception>
    internal sealed override EnemyProjectileMotion? InitialMotion(RoomEnemyProjectileKind kind)
    {
        TMotion? motion = _startingMotions is null ? _initial
            : _startingMotions.TryGetValue(kind, out TMotion? starting) ? starting
            : throw new InvalidOperationException($"{GetType().Name} is not bound to enemy projectile {kind}.");
        return motion is { } moving ? Moving(moving) : null;
    }

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
