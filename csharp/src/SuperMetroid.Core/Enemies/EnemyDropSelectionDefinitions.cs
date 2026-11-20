namespace SuperMetroid.Core.Enemies;

/// <summary>The order in which the drop roll weighs an enemy's drop chances, and the random roll compared with them.</summary>
internal static class EnemyDropSelectionDefinitions
{
    /// <summary>
    /// The minor drops, in the order the roll weighs them: their chances share the budget the offered major drops leave.
    /// </summary>
    internal static IReadOnlyList<EnemyDropRoll> MinorDrops { get; } =
    [
        EnemyDropRoll.Pickup(EnemyPickupKind.SmallEnergy),
        EnemyDropRoll.Pickup(EnemyPickupKind.BigEnergy),
        EnemyDropRoll.Pickup(EnemyPickupKind.Missile),
        EnemyDropRoll.Nothing,
    ];

    /// <summary>The major drops, in the order the roll weighs them after the minor drops: each keeps its own chance.</summary>
    internal static IReadOnlyList<EnemyDropRoll> MajorDrops { get; } =
    [
        EnemyDropRoll.Pickup(EnemyPickupKind.SuperMissile),
        EnemyDropRoll.Pickup(EnemyPickupKind.PowerBomb),
    ];

    /// <summary>
    /// The random choice a drop's chance roll reads, 1 through 255 (a zero is drawn again), compared with the accumulated
    /// drop chances.
    /// </summary>
    internal static readonly RandomSampleField DropRoll = new(0, 8);

    /// <summary>
    /// Whether a drop's chance roll is drawn again (#2510): taken when <see cref="DropRoll"/> reads zero, one sample in 256.
    /// </summary>
    internal static readonly RandomChance DropRollDrawnAgain = RandomChance.Lowest(1, DropRoll);

    /// <summary>The whole weight budget the drop chances share, before the enabled major drops take their part.</summary>
    internal const int FullWeightBudget = 255;

    /// <summary>The combined energy and reserve below which drops favour energy.</summary>
    internal const int CriticalEnergy = 30;

    /// <summary>The combined energy and reserve from which drops stop favouring energy.</summary>
    internal const int RecoveredEnergy = 50;
}
