namespace SuperMetroid.Core.Menus;

/// <summary>A row the host compares through the interface it implements.</summary>
/// <param name="Row">The row's place, which no code names: the comparison calls the record's equality, which compares it.</param>
public sealed record InterfaceComparedRow(int Row) : IComparedRow
{
    /// <inheritdoc/>
    public int Page => 1;
}
