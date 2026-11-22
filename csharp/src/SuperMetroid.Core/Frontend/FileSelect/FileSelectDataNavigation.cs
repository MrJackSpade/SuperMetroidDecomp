using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Bounded copy/clear row selection.</summary>
internal static partial class FileSelectDataNavigation
{
    /// <summary>Moves among occupied save slots and Exit, without wrapping.</summary>
    /// <remarks>
    /// Exit is always selectable, even when no slot is occupied.
    /// </remarks>
    internal static FileSelectItem MoveSourceOrClear(FileSelectItem selected, IReadOnlySet<SaveSlot> occupiedSlots, ControllerButton pressed) =>
        Move(selected, Ensure.NotNull(occupiedSlots), pressed);

    /// <summary>Moves among the other two save slots and Exit, without wrapping, Up winning when both are pressed.</summary>
    /// <remarks>Copy destinations exclude the source regardless of occupancy; the game
    /// skips the source while moving in either direction. The menu of a source holds no row for it (#2553).</remarks>
    internal static FileCopyDestinationRow MoveCopyDestination(FileCopyDestinationRow selected, ControllerButton pressed)
    {
        Ensure.NotNull(selected);
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
            return selected.Above;
        return (pressed & ControllerButton.Down) != ControllerButton.None ? selected.Below : selected;
    }

    /// <summary>
    /// Gives Up priority and moves from the selected row to the nearest selectable row that way on the copy and clear pages
    /// (<see cref="FileSelectItemRows.Data"/>), staying put when there is none. The search starts from the selected row's
    /// place on the page, as the game's does.
    /// </summary>
    private static FileSelectItem Move(FileSelectItem selected, IReadOnlySet<SaveSlot> enabledSlots, ControllerButton pressed)
    {
        // Up wins when both directions are pressed. The file select plays the sound.
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
