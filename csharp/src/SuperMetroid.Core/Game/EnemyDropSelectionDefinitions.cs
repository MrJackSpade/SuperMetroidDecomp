namespace SuperMetroid.Core.Game;

/// <summary>Semantic return identities of the six native item-drop probability columns.</summary>
internal static class EnemyDropSelectionDefinitions
{
    /// <summary>
    /// $86:F25E-F263, Random_Drop_Routine.drops: converts the selected probability
    /// column into its pickup identity. Minor columns are small/big health, missiles
    /// and no-drop; the two major columns are Super Missiles and Power Bombs.
    /// The accumulator order and probability arithmetic remain in the caller.
    /// </summary>
    internal static EnemyPickupKind ForProbabilityColumn(int column) => column switch
    {
        0 => EnemyPickupKind.SmallEnergy,
        1 => EnemyPickupKind.BigEnergy,
        2 => EnemyPickupKind.Missile,
        3 => EnemyPickupKind.NoDrop,
        4 => EnemyPickupKind.SuperMissile,
        5 => EnemyPickupKind.PowerBomb,
        _ => throw new IndexOutOfRangeException(),
    };
}