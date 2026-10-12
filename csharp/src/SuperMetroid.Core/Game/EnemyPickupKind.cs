using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Values returned by <c>Random_Drop_Routine</c> at $86:F106. These are deliberately the
/// cartridge values stored as <c>eproj_E / 2</c>; the table order is not the visual order of
/// the five pickup instruction lists.
/// </summary>
public enum EnemyPickupKind : ushort
{
    /// <summary>Zero sentinel used before a valid drop-table result; direct initialization treats it as dormant.</summary>
    None = 0,

    /// <summary>Small energy pickup restoring five energy before reserve overflow handling.</summary>
    SmallEnergy = 1,

    /// <summary>Large energy pickup restoring twenty energy before reserve overflow handling.</summary>
    BigEnergy = 2,

    /// <summary>Power Bomb pickup restoring one round, subject to the current capacity.</summary>
    PowerBomb = 3,

    /// <summary>Missile pickup restoring two missiles before the native reserve-missile overflow path.</summary>
    Missile = 4,

    /// <summary>Super Missile pickup restoring one round, subject to the current capacity.</summary>
    SuperMissile = 5,

    /// <summary>Random-table tail result that produces no collectible and enters the dormant death-animation tail.</summary>
    NoDrop = 6,
}
