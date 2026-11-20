namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a kind of enemy projectile does (#1760): how it moves itself under each motion it reaches, the animation
/// a shot that destroys it starts and the one touching Samus starts. Its spawner binds it to the projectile at spawn and
/// starts the animation it chooses; the projectile engine then runs the projectile through it without naming any kind.
/// <para>
/// A behaviour handles the cues its kinds' animations publish by implementing <see cref="IEnemyProjectileCues{TCue}"/>
/// for each cue type they publish, or <see cref="IEnemyProjectileBranchingCues{TSegment, TCue}"/> where a cue continues
/// the animation elsewhere; the engine handles the shared <see cref="EnemyProjectileCommonCue"/> for every kind.
/// </para>
/// <para>
/// A behaviour of this class itself is a kind that never moves itself and whose animations publish only shared cues;
/// families that move their projectiles derive from <see cref="EnemyProjectileMotionBehavior{TMotion}"/>, which owns their
/// typed motions (#1843); each motion reaches only the behaviour that started it (#2554).
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
    internal virtual EnemyProjectileAnimation ShotAnimation() => _shot?.Invoke() ?? EnemyProjectileDeleteAnimation.Instance;

    /// <summary>A new animation touching Samus starts, or null when the kind has none of its own.</summary>
    internal virtual EnemyProjectileAnimation? TouchAnimation() => _touch?.Invoke();

    /// <summary>
    /// Whether the projectile's flicker hides it on this update (#1839, #2455): the family of a projectile that flickers
    /// hides it on the updates its flicker chooses, and it is drawn only while shown. The n00b tube's crack and shards
    /// flicker so.
    /// </summary>
    internal virtual bool IsHidden => false;

    /// <summary>
    /// The motion a projectile starts with when its spawner binds this behaviour to it (#1843), bound with the behaviour
    /// that runs it, or null when it starts holding still. A kind of this class itself never moves itself.
    /// </summary>
    internal virtual EnemyProjectileMotion? StartingMotion => null;
}
