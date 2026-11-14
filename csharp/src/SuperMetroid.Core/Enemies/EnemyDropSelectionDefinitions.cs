namespace SuperMetroid.Core.Enemies;

/// <summary>The order in which the drop roll weighs an enemy's drop chances, and the random roll compared with them.</summary>
internal static class EnemyDropSelectionDefinitions
{
    /// <summary>
    /// The minor drops, in the order the roll weighs them: their chances share the budget the offered major drops leave.
    /// </summary>
    internal static IReadOnlyList<EnemyPickupKind> MinorDrops { get; } =
    [
        EnemyPickupKind.SmallEnergy,
        EnemyPickupKind.BigEnergy,
        EnemyPickupKind.Missile,
        EnemyPickupKind.NoDrop,
    ];

    /// <summary>The major drops, in the order the roll weighs them after the minor drops: each keeps its own chance.</summary>
    internal static IReadOnlyList<EnemyPickupKind> MajorDrops { get; } =
    [
        EnemyPickupKind.SuperMissile,
        EnemyPickupKind.PowerBomb,
    ];

    /// <summary>
    /// The random field a drop's chance roll reads: the sample modulo 256, 0 through 255, drawn again while it is zero, then
    /// compared with the accumulated drop chances.
    /// </summary>
    internal static readonly RandomSampleField DropRoll = new(0, 8);
}
