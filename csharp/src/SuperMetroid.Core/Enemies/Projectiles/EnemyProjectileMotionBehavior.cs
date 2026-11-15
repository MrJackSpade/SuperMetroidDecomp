namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A kind of enemy projectile that moves itself (#1843): it owns the motions <typeparamref name="TMotion"/> names, starts
/// each projectile it is bound to in its kind's motion, and moves it each pass by the motion the projectile holds. Its own
/// cues and code change the motion by calling its methods; a shot that destroys the projectile, or its deletion, stops it.
/// </summary>
/// <typeparam name="TMotion">The family's motions.</typeparam>
internal abstract class EnemyProjectileBehavior<TMotion> : EnemyProjectileBehavior
    where TMotion : struct, Enum
{
    /// <summary>The motion every kind this behaviour is bound to starts with, or null when it starts holding still.</summary>
    private readonly TMotion? _initial;

    /// <summary>
    /// The motion each kind this behaviour is bound to starts with, null for one that starts holding still; null when every
    /// kind starts with <see cref="_initial"/>.
    /// </summary>
    private readonly IReadOnlyDictionary<RoomEnemyProjectileKind, TMotion?>? _startingMotions;

    /// <summary>A kind that starts in <paramref name="initial"/> and reacts to shots and touch by the factories' animations.</summary>
    /// <param name="initial">The motion the projectile starts with, or null when it starts holding still.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    private protected EnemyProjectileBehavior(
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
    private protected EnemyProjectileBehavior(
        IReadOnlyDictionary<RoomEnemyProjectileKind, TMotion?> startingMotions,
        Func<EnemyProjectileAnimation>? shot = null,
        Func<EnemyProjectileAnimation>? touch = null)
        : base(shot, touch) => _startingMotions = Ensure.NotNull(startingMotions);

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">This behaviour does not name the motion <paramref name="kind"/> starts with.</exception>
    internal sealed override Enum? InitialMotion(RoomEnemyProjectileKind kind)
    {
        TMotion? motion = _startingMotions is null
            ? _initial
            : _startingMotions.TryGetValue(kind, out TMotion? starting)
                ? starting
                : throw new InvalidOperationException($"{GetType().Name} is not bound to enemy projectile {kind}.");
        return motion is { } moving ? moving : null;
    }

    /// <inheritdoc/>
    internal sealed override void RunMotion(EnemyProjectileMotionContext context)
    {
        if (context.Projectile.Motion is TMotion motion)
            Move(context, motion);
        else
            base.RunMotion(context);
    }

    /// <summary>Moves the projectile one pass by <paramref name="motion"/>, which this behaviour owns.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    private protected abstract void Move(EnemyProjectileMotionContext context, TMotion motion);
}
