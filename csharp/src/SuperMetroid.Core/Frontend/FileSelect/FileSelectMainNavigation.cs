namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Visible main-menu navigation.</summary>
internal static partial class FileSelectMainNavigation
{
    /// <summary>Moves one row through the main page's shown rows, wrapping from either end to the other.</summary>
    /// <remarks>
    /// The save slots and Exit are always shown; Copy and Clear only while a save exists (<see cref="FileSelectItemRows.Main"/>).
    /// From Copy or Clear while no save exists, rows the page does not show, the game moves up to the row above on the full
    /// page and down to Exit, the first shown row below them. Direction is the file select's single Up or Down choice, Up
    /// winning when both are held.
    /// </remarks>
    internal static FileSelectItem Move(FileSelectItem selected, bool hasAnySave, VerticalDirection direction)
    {
        Menus.MenuRowOrder<FileSelectItem> rows = FileSelectItemRows.Main(hasAnySave);
        if (rows.Contains(selected))
            return direction == VerticalDirection.Down ? rows.Below(selected, out _) : rows.Above(selected, out _);
        return direction == VerticalDirection.Down ? FileSelectItem.Exit : FileSelectItemRows.MainWithData.Above(selected, out _);
    }
}
