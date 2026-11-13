namespace SuperMetroid.Core.Enemies;

/// <summary>Semantic return identities of the six item-drop probability columns, and the random roll compared with them.</summary>
internal static class EnemyDropSelectionDefinitions
{
    /// <summary>
    /// The pickup identity of each
    /// probability column. Minor columns are small/big health, missiles
    /// and no-drop; the two major columns are Super Missiles and Power Bombs.
    /// The running-sum order and probability arithmetic remain in the caller.
    /// </summary>
    internal static readonly EnemyPickupKind[] ProbabilityColumns =
    [
        EnemyPickupKind.SmallEnergy,
        EnemyPickupKind.BigEnergy,
        EnemyPickupKind.Missile,
        EnemyPickupKind.NoDrop,
        EnemyPickupKind.SuperMissile,
        EnemyPickupKind.PowerBomb,
    ];

    /// <summary>
    /// The random field a drop's chance roll reads: bits zero to seven, 0 through 255, drawn again while it is zero, then
    /// compared with the accumulated column chances.
    /// </summary>
    internal static readonly RandomSampleField DropRoll = new(0, 8);
}
