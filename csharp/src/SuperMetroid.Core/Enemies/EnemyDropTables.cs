namespace SuperMetroid.Core.Enemies;

/// <summary>The order in which the drop roll weighs an enemy's drop chances.</summary>
internal static class EnemyDropTables
{
    /// <summary>
    /// $86:F25E-F263, Random_Drop_Routine.drops: the drop the roll weighs at position <paramref name="column"/>.
    /// The accumulator order and probability arithmetic remain in the caller.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="column"/> is outside the six positions.</exception>
    internal static EnemyPickupKind ForProbabilityColumn(int column) =>
        (uint)column < (uint)EnemyDropSelectionDefinitions.ProbabilityColumns.Length
            ? EnemyDropSelectionDefinitions.ProbabilityColumns[column]
            : throw new ArgumentOutOfRangeException(nameof(column));
}
