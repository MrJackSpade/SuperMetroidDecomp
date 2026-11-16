namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Visible main-menu navigation.</summary>
internal static class FileSelectMainNavigation
{
    /// <summary>Moves one row, wrapping at Exit and skipping Copy/Clear when every save slot is empty.</summary>
    /// <remarks>
    /// The save slots and Exit are always shown; Copy and Clear only while a save exists. Moving from an item that is
    /// not shown is an error. Direction is the file select's single Up or Down choice, Up winning when both are held.
    /// </remarks>
    internal static FileSelectItem Move(FileSelectItem selected, bool hasAnySave, VerticalDirection direction)
    {
        if (!Visible(selected, hasAnySave))
            throw new InvalidDataException($"File-select item {selected} is not currently visible.");

        if (direction == VerticalDirection.Down)
        {
            if (selected == FileSelectItem.Exit) return FileSelectItem.SlotA;
            if (!hasAnySave && selected == FileSelectItem.SlotC) return FileSelectItem.Exit;
            return FileSelectItemRows.MainItem(FileSelectItemRows.MainRow(selected) + 1);
        }
        if (selected == FileSelectItem.SlotA) return FileSelectItem.Exit;
        if (!hasAnySave && selected == FileSelectItem.Exit) return FileSelectItem.SlotC;
        return FileSelectItemRows.MainItem(FileSelectItemRows.MainRow(selected) - 1);
    }

    /// <summary>Whether the main page shows an item: Copy and Clear appear only while a save exists.</summary>
    private static bool Visible(FileSelectItem item, bool hasAnySave) => item switch
    {
        FileSelectItem.SlotA or FileSelectItem.SlotB or FileSelectItem.SlotC or FileSelectItem.Exit => true,
        FileSelectItem.Copy or FileSelectItem.Clear => hasAnySave,
        _ => throw new InvalidDataException($"File-select item {item} is not currently visible."),
    };
}
