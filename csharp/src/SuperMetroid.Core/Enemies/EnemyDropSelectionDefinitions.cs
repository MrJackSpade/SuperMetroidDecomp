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
    /// The random choice a drop's chance roll reads, 1 through 255 (a zero is drawn again), compared with the accumulated
    /// drop chances.
    /// </summary>
    internal static readonly RandomSampleField DropRoll = new(0, 8);

    /// <summary>The whole weight budget the drop chances share, before the enabled major drops take their part.</summary>
    internal const int FullWeightBudget = 255;

    /// <summary>The combined energy and reserve below which drops favour energy.</summary>
    internal const int CriticalEnergy = 30;

    /// <summary>The combined energy and reserve from which drops stop favouring energy.</summary>
    internal const int RecoveredEnergy = 50;
}
