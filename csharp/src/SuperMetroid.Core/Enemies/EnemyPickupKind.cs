namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the random drop selection chooses for a defeated enemy: one of the five pickups, or no drop.
/// </summary>
public enum EnemyPickupKind
{
    /// <summary>
    /// No collectible: the selection's result when nothing drops, which leaves the death to end without a pickup.
    /// </summary>
    NoDrop,

    /// <summary>Small energy pickup restoring five energy, any past full health filling the reserve tanks.</summary>
    SmallEnergy,

    /// <summary>Large energy pickup restoring twenty energy, any past full health filling the reserve tanks.</summary>
    BigEnergy,

    /// <summary>Power Bomb pickup restoring one round, subject to the current capacity.</summary>
    PowerBomb,

    /// <summary>Missile pickup restoring two missiles, subject to the current capacity.</summary>
    Missile,

    /// <summary>Super Missile pickup restoring one round, subject to the current capacity.</summary>
    SuperMissile,
}
