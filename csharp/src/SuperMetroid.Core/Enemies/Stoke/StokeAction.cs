namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// The action Stoke performs each frame. The game has separate moving-left and moving-right
/// actions; which one runs always matches the stored direction, so the direction is read from
/// <see cref="StokeEnemyState.Direction"/> alone.
/// </summary>
public enum StokeAction
{
    /// <summary>No-op main entry while the attack animation owns projectile emission and eventual walk resumption.</summary>
    IdleDuringAttack,

    /// <summary>
    /// Walking left or right, with the 16.16 speed pair of the stored direction's side, terrain-turn checks, and the
    /// per-update random attack choice toward that side.
    /// </summary>
    Moving,
}
