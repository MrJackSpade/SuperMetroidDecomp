namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The behaviour Cacatac's main AI runs each frame. Initialization selects patrolling, or stopped for a Cacatac placed
/// to stop at first; no other start is authored.
/// Its values are arbitrary.
/// </summary>
public enum CacatacAction
{
    /// <summary>
    /// Moves at its patrol speed the way <see cref="CacatacEnemyState.PatrolDirection"/> names (#2277), turning back past
    /// the left bound or at or beyond the right bound, then rolls for an attack.
    /// </summary>
    Patrolling,
    /// <summary>Performs no patrol update while attack animation cues own the actor; also the initial action of a Cacatac placed with an initial stop.</summary>
    Stopped,
}
