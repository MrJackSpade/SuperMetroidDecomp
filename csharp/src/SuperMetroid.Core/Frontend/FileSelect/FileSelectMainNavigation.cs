namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Visible main-menu navigation from $81:A25E..A2AF.</summary>
internal static class FileSelectMainNavigation
{
    /// <summary>Moves one row, wrapping at Exit and skipping Copy/Clear when every save slot is empty.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the native up/down branches and the
    /// prior managed choice lists. Save slots always remain selectable; Copy and
    /// Clear require at least one save; Exit is always visible. These are semantic
    /// visibility and wrap rules, not stored ordinal lists. Hidden selections preserve
    /// the previous InvalidDataException and diagnostic text.
    /// Direction is the existing caller's single Up or Down decision, after Up priority.
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
