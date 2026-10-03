namespace SuperMetroid.Core.Frontend;

/// <summary>Visible main-menu navigation from $81:A25E..A2AF.</summary>
internal static class FileSelectMainNavigation
{
    /// <summary>Moves one row, wrapping at Exit and skipping Copy/Clear when every save slot is empty.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the native up/down branches and the
    /// prior managed choice lists. Save slots0..2 always remain selectable; Copy3 and
    /// Clear4 require at least one save; Exit5 is always visible. These are semantic
    /// visibility and wrap rules, not stored ordinal lists. Unsupported or hidden
    /// selections preserve the previous InvalidDataException and diagnostic text.
    /// Direction is the existing caller's single Up or Down decision, after Up priority.
    /// </remarks>
    internal static int Move(int selected, bool hasAnySave, bool down)
    {
        const int exit = FileSelectLayout.MainSelectionCount - 1;
        if ((uint)selected >= FileSelectLayout.MainSelectionCount ||
            (!hasAnySave && selected is 3 or 4))
            throw new InvalidDataException($"File-select item {selected} is not currently visible.");

        if (down)
        {
            if (selected == exit) return 0;
            if (!hasAnySave && selected == FileSelectLayout.SaveSlotCount - 1) return exit;
            return selected + 1;
        }
        if (selected == 0) return exit;
        if (!hasAnySave && selected == exit) return FileSelectLayout.SaveSlotCount - 1;
        return selected - 1;
    }
}
