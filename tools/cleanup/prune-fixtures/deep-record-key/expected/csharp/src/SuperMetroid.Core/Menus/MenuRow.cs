namespace SuperMetroid.Core.Menus;

/// <summary>The row a menu's cursor is on, which names a row of its page.</summary>
public abstract record MenuRow
{
    /// <summary>Closes the hierarchy to the page declared here.</summary>
    private MenuRow()
    {
    }

    /// <summary>A row of the controller page.</summary>
    /// <param name="Row">The row, which no code names.</param>
    public sealed record OnControllerPage(ControllerRow Row) : MenuRow;
}
