namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy that tells its species when it leaves its place (#2426): it is deleted, killed or replaced by a new occupant,
/// so a species holding its occupants drops it then.
/// </summary>
internal interface IEnemyDeparture
{
    /// <summary>Runs once, as the enemy's place stops holding it, before any new occupant is bound.</summary>
    void Leave();
}
