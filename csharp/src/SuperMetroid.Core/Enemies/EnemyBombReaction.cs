namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species reaction to a normal bomb that enters common bomb damage. The bomb pass runs its stages around the
/// common damage at the game's positions; a reaction sets only the stages it has. The species' entity decides which
/// contacts use it (<see cref="IEnemyBombReaction.CommonBombDamageReaction"/>).
/// </summary>
internal sealed class EnemyBombReaction
{
    /// <summary>Whether the private reaction returns before common damage (the bomb stays marked).</summary>
    internal Func<EnemyBombContext, bool>? Rejects { get; init; }

    /// <summary>Whether common damage runs the generic death animation on a lethal hit.</summary>
    internal bool RunsGenericDeath { get; init; } = true;

    /// <summary>
    /// Whether a bomb that hurts an enemy that is not frozen shows the enemy-hit burst at it, after the damage and before
    /// the death (<see cref="EnemyCollision"/>), as the shot reaction the bomb runs does for a beam.
    /// </summary>
    internal Func<EnemyBombContext, bool> ShowsHitBurst { get; init; } = AlwaysShowsHitBurst;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction whose every hurting bomb shows the burst.</summary>
    internal static Func<EnemyBombContext, bool> AlwaysShowsHitBurst { get; } = static _ => true;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction that runs the common damage without the burst.</summary>
    internal static Func<EnemyBombContext, bool> NeverShowsHitBurst { get; } = static _ => false;

    /// <summary>Runs immediately before common damage.</summary>
    internal Action<EnemyBombContext>? BeforeCommon { get; init; }

    /// <summary>The species tail after common damage.</summary>
    internal Action<EnemyBombContext>? AfterCommon { get; init; }
}
