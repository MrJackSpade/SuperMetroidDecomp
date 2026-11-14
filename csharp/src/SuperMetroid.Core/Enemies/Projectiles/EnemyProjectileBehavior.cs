namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a kind of enemy projectile does (#1760): how it moves itself under each frame behaviour it reaches, the animation
/// a shot that destroys it starts and the one touching Samus starts. Its spawner binds it to the projectile at spawn and
/// starts the animation it chooses; the projectile engine then runs the projectile through it without naming any kind.
/// <para>
/// A behaviour handles the cues its kinds' animations publish by implementing <see cref="IEnemyProjectileCues{TCue}"/>
/// for each cue type they publish, or <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> where a cue continues
/// the animation elsewhere; the engine handles the shared <see cref="EnemyProjectileCommonCue"/> for every kind.
/// </para>
/// <para>
/// A behaviour of this class itself is a kind that never moves itself and whose animations publish only shared cues;
/// families that move their projectiles derive from it and override <see cref="Move"/>.
/// </para>
/// </summary>
internal class EnemyProjectileBehavior
{
    /// <summary>
    /// The kinds that do nothing of their own (#1760): they never move themselves, their animations publish only shared
    /// cues, and a shot that destroys one plays the shared delete animation. It keeps no state.
    /// </summary>
    internal static EnemyProjectileBehavior Still { get; } = new();

    /// <summary>Creates the animation a destroying shot starts, or null when the kind plays the shared delete animation.</summary>
    private readonly Func<EnemyProjectileAnimation>? _shot;

    /// <summary>Creates the animation touching Samus starts, or null when the kind has no touch animation of its own.</summary>
    private readonly Func<EnemyProjectileAnimation>? _touch;

    /// <summary>A kind that reacts to shots and Samus's touch with the animations the factories create.</summary>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    internal EnemyProjectileBehavior(Func<EnemyProjectileAnimation>? shot = null, Func<EnemyProjectileAnimation>? touch = null)
    {
        _shot = shot;
        _touch = touch;
    }

    /// <summary>
    /// A new animation a shot that destroys the projectile starts: the kind's own, or the shared delete animation every
    /// kind without a shot reaction of its own plays.
    /// </summary>
    internal EnemyProjectileAnimation ShotAnimation() => _shot?.Invoke() ?? EnemyProjectileDeleteAnimation.Instance;

    /// <summary>A new animation touching Samus starts, or null when the kind has none of its own.</summary>
    internal EnemyProjectileAnimation? TouchAnimation() => _touch?.Invoke();

    /// <summary>
    /// Runs the projectile's frame behaviour for one projectile pass, before its animation advances. A stationary
    /// projectile does not move itself: it holds still while its animation plays, or its owner positions it.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every frame behaviour.</param>
    internal void RunFrameBehavior(EnemyProjectileFrameBehaviorContext context)
    {
        if (context.Projectile.FrameBehavior != EnemyProjectileFrameBehavior.Stationary)
            Move(context);
    }

    /// <summary>
    /// Moves the projectile under its frame behaviour, which is not <see cref="EnemyProjectileFrameBehavior.Stationary"/>.
    /// A kind runs only the frame behaviours its definition, its animations and its own code install; any other is a
    /// defect.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every frame behaviour.</param>
    /// <exception cref="InvalidOperationException">The kind does not run the projectile's frame behaviour.</exception>
    private protected virtual void Move(EnemyProjectileFrameBehaviorContext context) =>
        throw new InvalidOperationException(
            $"Enemy projectile {context.Projectile.Kind} in place {context.Projectile.Place} does not run frame behaviour {context.Projectile.FrameBehavior}.");
}
