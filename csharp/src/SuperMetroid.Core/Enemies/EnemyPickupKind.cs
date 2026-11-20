namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The five pickups a defeated enemy can drop; the drop roll's outcome is one of them or nothing (<see cref="EnemyDropRoll"/>).
/// </summary>
public enum EnemyPickupKind
{
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
