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

    /// <summary>Small energy pickup restoring <see cref="EnemyPickupDefinitions.SmallEnergyAmount"/>, any past full health filling the reserve tanks.</summary>
    SmallEnergy,

    /// <summary>Large energy pickup restoring <see cref="EnemyPickupDefinitions.BigEnergyAmount"/>, any past full health filling the reserve tanks.</summary>
    BigEnergy,

    /// <summary>Power Bomb pickup restoring <see cref="EnemyPickupDefinitions.PowerBombAmount"/>, subject to the current capacity.</summary>
    PowerBomb,

    /// <summary>Missile pickup restoring <see cref="EnemyPickupDefinitions.MissileAmount"/>, subject to the current capacity.</summary>
    Missile,

    /// <summary>Super Missile pickup restoring <see cref="EnemyPickupDefinitions.SuperMissileAmount"/>, subject to the current capacity.</summary>
    SuperMissile,
}
