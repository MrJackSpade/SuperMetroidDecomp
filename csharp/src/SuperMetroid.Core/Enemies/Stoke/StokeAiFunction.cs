namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's main-AI routine. The game has separate moving-left and moving-right
/// routines; which one runs always matches the direction word, so the direction is read from
/// <see cref="StokeEnemyState.Direction"/> alone.
/// </summary>
public enum StokeAiFunction
{
    /// <summary>No-op main entry while the attack animation owns projectile emission and eventual walk resumption.</summary>
    IdleDuringAttack,

    /// <summary>
    /// Walking left or right, with the 16.16 speed pair of the direction word's side, terrain-turn checks, and the
    /// per-update random attack choice toward that side.
    /// </summary>
    Moving,
}
