namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the random drop selection chooses for a defeated enemy: nothing yet, one of the five pickups, or no drop.
/// </summary>
public enum EnemyPickupKind
{
    /// <summary>No pickup chosen yet; direct initialization treats it as dormant.</summary>
    None,

    /// <summary>Small energy pickup restoring five energy before reserve overflow handling.</summary>
    SmallEnergy,

    /// <summary>Large energy pickup restoring twenty energy before reserve overflow handling.</summary>
    BigEnergy,

    /// <summary>Power Bomb pickup restoring one round, subject to the current capacity.</summary>
    PowerBomb,

    /// <summary>Missile pickup restoring two missiles before the reserve-missile overflow path.</summary>
    Missile,

    /// <summary>Super Missile pickup restoring one round, subject to the current capacity.</summary>
    SuperMissile,

    /// <summary>Random-table tail result that produces no collectible and enters the dormant death-animation tail.</summary>
    NoDrop,
}
