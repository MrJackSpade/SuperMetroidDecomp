using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Bounded copy/clear row selection.</summary>
internal static class FileSelectDataNavigation
{
    /// <summary>Moves among occupied save slots and Exit, without wrapping.</summary>
    /// <remarks>
    /// Exit is always selectable, even when no slot is occupied.
    /// No ordinal lookup or generated list is required.
    /// </remarks>
    internal static FileSelectItem MoveSourceOrClear(FileSelectItem selected, IReadOnlySet<SaveSlot> occupiedSlots, ControllerButton pressed) =>
        Move(selected, Ensure.NotNull(occupiedSlots), pressed);

    /// <summary>Moves among the other two save slots and Exit, without wrapping.</summary>
    /// <remarks>Copy destinations exclude the source regardless of occupancy; the game
    /// skips the source while moving in either direction.</remarks>
    internal static FileSelectItem MoveCopyDestination(FileSelectItem selected, SaveSlot sourceSlot, ControllerButton pressed) =>
        Move(selected, SaveSlots.All.Where(slot => slot != sourceSlot).ToHashSet(), pressed);

    /// <summary>
    /// Validates the selected row, gives Up priority, and moves to the nearest selectable row that way on the copy and clear
    /// pages (<see cref="FileSelectItemRows.Data"/>), staying put when there is none.
    /// </summary>
    private static FileSelectItem Move(FileSelectItem selected, IReadOnlySet<SaveSlot> enabledSlots, ControllerButton pressed)
    {
        if (!Selectable(selected))
            throw new InvalidDataException($"Submenu item {selected} is not selectable.");

        // The selection is checked even on an update with no press, and Up wins when both directions are pressed. The
        // file select plays the sound.
        Menus.MenuRowOrder<FileSelectItem> rows = FileSelectItemRows.Data;
        IEnumerable<FileSelectItem> onward;
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
            onward = rows.TakeWhile(row => row != selected).Reverse();
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
            onward = rows.SkipWhile(row => row != selected).Skip(1);
        else
            return selected;
        return onward.Where(Selectable).Cast<FileSelectItem?>().FirstOrDefault() ?? selected;

        bool Selectable(FileSelectItem item) => item == FileSelectItem.Exit ||
            (FileSelectItemRows.TryGetSaveSlot(item, out SaveSlot slot) && enabledSlots.Contains(slot));
    }
}
