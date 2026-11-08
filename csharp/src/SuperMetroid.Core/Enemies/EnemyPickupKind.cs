namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the random drop selection chooses for a defeated enemy: nothing yet, one of the five pickups, or no drop.
/// Its values are arbitrary.
/// </summary>
public enum EnemyPickupKind
{
    /// <summary>No pickup chosen yet; direct initialization treats it as dormant.</summary>
    None = 0,

    /// <summary>Small energy pickup restoring five energy before reserve overflow handling.</summary>
    SmallEnergy = 1,

    /// <summary>Large energy pickup restoring twenty energy before reserve overflow handling.</summary>
    BigEnergy = 2,

    /// <summary>Power Bomb pickup restoring one round, subject to the current capacity.</summary>
    PowerBomb = 3,

    /// <summary>Missile pickup restoring two missiles before the reserve-missile overflow path.</summary>
    Missile = 4,

    /// <summary>Super Missile pickup restoring one round, subject to the current capacity.</summary>
    SuperMissile = 5,

    /// <summary>Random-table tail result that produces no collectible and enters the dormant death-animation tail.</summary>
    NoDrop = 6,
}
