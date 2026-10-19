namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One enemy's six item-drop probabilities, out of 255, in the table's column order; the drop roll reads them by
/// column.
/// </summary>
internal sealed class EnemyDropChances
{
    /// <summary>The probabilities by column.</summary>
    private readonly byte[] _columns;

    /// <summary>Copies the six probabilities of one table.</summary>
    internal EnemyDropChances(ReadOnlySpan<byte> columns) =>
        _columns = Ensure.LengthEqual(columns, EnemyDropChanceDefinitions.RecordSize).ToArray();

    /// <summary>Copies the probabilities into <paramref name="destination"/>, column by column.</summary>
    internal void CopyTo(Span<byte> destination) => _columns.CopyTo(destination);
}
