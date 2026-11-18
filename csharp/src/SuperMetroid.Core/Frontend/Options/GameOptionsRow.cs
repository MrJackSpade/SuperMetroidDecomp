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
    public sealed record OnPrimaryPage() : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Primary;
    }

    /// <summary>A row of the controller page.</summary>
    public sealed record OnControllerPage() : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Controller;
    }

    /// <summary>A row of the special-settings page.</summary>
    public sealed record OnSpecialPage() : GameOptionsRow
    {
        /// <inheritdoc/>
        internal override GameOptionsPage Page => GameOptionsPage.Special;
    }
}
