namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The five pickups a defeated enemy can drop; the drop roll's outcome is one of them or nothing (<see cref="EnemyDropRoll"/>).
/// </summary>
public enum EnemyPickupKind
{
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
