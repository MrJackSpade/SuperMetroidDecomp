namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The row the options cursor is on (#2206): a row of one of the three pages, named by what it does. Each page's rows
/// stand in <see cref="GameOptionsPageRows"/>'s order from the top, which is also the order of the page's installed cursor
/// anchors.
/// </summary>
public abstract record GameOptionsRow
{
    /// <summary>Closes the hierarchy to the pages declared here.</summary>
    private GameOptionsRow()
    {
    }

    /// <summary>The page the row is on.</summary>
    internal abstract GameOptionsPage Page { get; }

    /// <summary>How many rows above the row its page shows: the place of the row's installed cursor anchor.</summary>
    /// <exception cref="InvalidOperationException">The row is not on its page.</exception>
    internal abstract int Place { get; }

    /// <summary>A row of the primary page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnPrimaryPage(GameOptionsPrimaryRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Primary;

        /// <inheritdoc/>
        internal override int Place => GameOptionsPageRows.Primary.PlaceOf(Row);
    }

    /// <summary>A row of the controller page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnControllerPage(GameOptionsControllerRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Controller;

        /// <inheritdoc/>
        internal override int Place => GameOptionsPageRows.Controller.PlaceOf(Row);
    }

    /// <summary>A row of the special-settings page.</summary>
    /// <param name="Row">The row.</param>
    public sealed record OnSpecialPage(GameOptionsSpecialRow Row) : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Special;

        /// <inheritdoc/>
        internal override int Place => GameOptionsPageRows.Special.PlaceOf(Row);
    }
}
