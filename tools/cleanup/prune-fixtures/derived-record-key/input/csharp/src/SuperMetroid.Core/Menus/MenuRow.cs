namespace SuperMetroid.Core.Menus;

/// <summary>The row a menu's cursor is on: a row of one of the two pages, which tells one row of a page from another only by the row it names.</summary>
public abstract record MenuRow
{
    /// <summary>Closes the hierarchy to the pages declared here.</summary>
    private MenuRow()
    {
    }

    /// <summary>A row of the first page.</summary>
    /// <param name="Row">The row, which no code names: comparing two rows of the page compares it.</param>
    public sealed record OnFirstPage(FirstPageRow Row) : MenuRow;

    /// <summary>A row of the second page.</summary>
    /// <param name="Row">The row, which no code names either.</param>
    public sealed record OnSecondPage(SecondPageRow Row) : MenuRow;
}
