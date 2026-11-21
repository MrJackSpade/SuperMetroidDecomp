namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Visible main-menu navigation.</summary>
internal static partial class FileSelectMainNavigation
{
    /// <summary>Moves one row through the main page's shown rows, wrapping from either end to the other.</summary>
    /// <remarks>
    /// The save slots and Exit are always shown; Copy and Clear only while a save exists (<see cref="FileSelectItemRows.Main"/>).
    /// Moving from an item that is not shown is an error. Direction is the file select's single Up or Down choice, Up winning
    /// when both are held.
    /// </remarks>
    /// <exception cref="InvalidDataException"><paramref name="selected"/> is not shown on the main page.</exception>
    internal static FileSelectItem Move(FileSelectItem selected, bool hasAnySave, VerticalDirection direction)
    {
        Menus.MenuRowOrder<FileSelectItem> rows = FileSelectItemRows.Main(hasAnySave);
        if (!rows.Contains(selected))
            throw new InvalidDataException($"File-select item {selected} is not currently visible.");
        return direction switch
        {
            VerticalDirection.Down => rows.Below(selected, out _),
            VerticalDirection.Up => rows.Above(selected, out _),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Not a vertical direction."),
        };
    }
}
