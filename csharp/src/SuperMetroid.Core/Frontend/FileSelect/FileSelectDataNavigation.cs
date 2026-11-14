using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Bounded copy/clear row selection.</summary>
internal static class FileSelectDataNavigation
{
    /// <summary>Moves among occupied save slots and Exit, without wrapping.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the game's source/clear selection and
    /// the prior managed filtered lists. Exit is always selectable, even when no slot is occupied.
    /// No ordinal lookup or generated list is required.
    /// </remarks>
    internal static FileSelectItem MoveSourceOrClear(FileSelectItem selected, IReadOnlySet<SaveSlot> occupiedSlots, ControllerButton pressed) =>
        Move(selected, Ensure.NotNull(occupiedSlots), pressed);

    /// <summary>Moves among the other two save slots and Exit, without wrapping.</summary>
    /// <remarks>Copy destinations exclude the source regardless of occupancy; the game
    /// skips the source while moving in either direction.</remarks>
    internal static FileSelectItem MoveCopyDestination(FileSelectItem selected, SaveSlot sourceSlot, ControllerButton pressed) =>
        Move(selected, SaveSlots.All.Where(slot => slot != sourceSlot).ToHashSet(), pressed);

    /// <summary>Validates the selected row, gives Up priority, and scans toward the next enabled slot or Exit without wrapping.</summary>
    private static FileSelectItem Move(FileSelectItem selected, IReadOnlySet<SaveSlot> enabledSlots, ControllerButton pressed)
    {
        if (!Selectable(selected))
            throw new InvalidDataException($"Submenu item {selected} is not selectable.");

        // Preserve the managed input order, including validation on idle frames
        // and Up priority when both directions are pressed. The caller owns sound.
        int step = (pressed & ControllerButton.Up) != ControllerButton.None ? -1
            : (pressed & ControllerButton.Down) != ControllerButton.None ? 1 : 0;
        if (step == 0) return selected;
        for (int row = FileSelectItemRows.DataRow(selected) + step; row is >= 0 and < FileSelectLayout.DataSelectionCount; row += step)
            if (Selectable(FileSelectItemRows.DataItem(row))) return FileSelectItemRows.DataItem(row);
        return selected;

        bool Selectable(FileSelectItem item) => item == FileSelectItem.Exit ||
            (FileSelectItemRows.TryGetSaveSlot(item, out SaveSlot slot) && enabledSlots.Contains(slot));
    }
}
