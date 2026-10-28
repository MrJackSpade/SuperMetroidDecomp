using static SuperMetroid.Core.Enemies.EnemyDropChanceDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>Reads enemy item-drop probability tables and turns a chosen probability column into its pickup.</summary>
internal static class EnemyDropTables
{
    /// <summary>
    /// Copies one probability table into <paramref name="destination"/>. Tables outside the complete 118-table retail
    /// catalog fail explicitly rather than reading adjacent bytes as weights.
    /// </summary>
    internal static void Copy(EnemyDropTable table, Span<byte> destination)
    {
        if (destination.Length < RecordSize)
        {
            throw new ArgumentException(
                $"Enemy drop chance destination requires {RecordSize} bytes.",
                nameof(destination));
        }

        if ((uint)table.Record >= RecordCount)
        {
            throw new InvalidDataException(
                $"Enemy drop table {table.Record} is outside the compiled {RecordCount}-table catalog.");
        }
        PackedChances.AsSpan(table.Record * RecordSize, RecordSize).CopyTo(destination);
    }

    /// <summary>The chances of <paramref name="table"/>, validated as <see cref="Copy"/> does.</summary>
    internal static EnemyDropChances At(EnemyDropTable table)
    {
        Span<byte> columns = stackalloc byte[RecordSize];
        Copy(table, columns);
        return new EnemyDropChances(columns);
    }

    /// <summary>
    /// $86:F25E-F263, Random_Drop_Routine.drops: converts the selected probability column into its pickup identity.
    /// The accumulator order and probability arithmetic remain in the caller.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="column"/> is outside the six columns.</exception>
    internal static EnemyPickupKind ForProbabilityColumn(int column) =>
        (uint)column < (uint)EnemyDropSelectionDefinitions.ProbabilityColumns.Length
            ? EnemyDropSelectionDefinitions.ProbabilityColumns[column]
            : throw new ArgumentOutOfRangeException(nameof(column));
}
