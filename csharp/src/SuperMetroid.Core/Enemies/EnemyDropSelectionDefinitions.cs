namespace SuperMetroid.Core.Enemies;

/// <summary>Semantic return identities of the six item-drop probability columns.</summary>
internal static class EnemyDropSelectionDefinitions
{
    /// <summary>
    /// The pickup identity of each
    /// probability column. Minor columns are small/big health, missiles
    /// and no-drop; the two major columns are Super Missiles and Power Bombs.
    /// The accumulator order and probability arithmetic remain in the caller.
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
}
