namespace SuperMetroid.Core.Menus;

/// <summary>A row of the first page.</summary>
/// <param name="Row">The row's place on the page, which no code names: comparing two rows by the interface compares it.</param>
public sealed record FirstPageMenuRow(int Row) : IMenuRow
{
    /// <inheritdoc/>
    public int Page => 1;
}
