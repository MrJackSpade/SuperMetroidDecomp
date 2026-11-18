namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The row the options cursor is on (#2206): a row of one of the three pages, named by what it does; the installation
/// places the cursor on each row (#2415).
/// </summary>
public abstract record GameOptionsRow
{
    /// <summary>Closes the hierarchy to the pages declared here.</summary>
    private GameOptionsRow()
    {
    }

    /// <summary>The page the row is on.</summary>
    internal abstract GameOptionsPage Page { get; }

    /// <summary>A row of the primary page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnPrimaryPage(GameOptionsPrimaryRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Primary;
    }

    /// <summary>A row of the controller page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnControllerPage(GameOptionsControllerRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Controller;
    }

    /// <summary>A row of the special-settings page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnSpecialPage(GameOptionsSpecialRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Special;
    }
}
