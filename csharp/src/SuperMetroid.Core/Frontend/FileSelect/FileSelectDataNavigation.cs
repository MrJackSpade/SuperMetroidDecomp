using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Bounded copy/clear row selection.</summary>
internal static class FileSelectDataNavigation
{
    /// <summary>Three-bit mask admitting every save slot before occupied or excluded-source filtering.</summary>
    private const int AllSlots = (1 << FileSelectLayout.SaveSlotCount) - 1;

    /// <summary>Moves among occupied save slots and Exit, without wrapping.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the game's source/clear selection and
    /// the prior managed filtered lists. The occupied slots hold one flag each
    /// for slots A..C, each slot's flag twice the one before
    /// it. Exit is always selectable, even when no slot is occupied.
    /// No ordinal lookup or generated list is required.
    /// </remarks>
    internal static FileSelectItem MoveSourceOrClear(FileSelectItem selected, int occupiedSlots, ControllerButton pressed)
    {
        if ((uint)occupiedSlots > AllSlots)
            throw new ArgumentOutOfRangeException(nameof(occupiedSlots));
        return Move(selected, occupiedSlots, pressed);
    }

    /// <summary>Moves among the other two save slots and Exit, without wrapping.</summary>
    /// <remarks>Copy destinations exclude the source regardless of occupancy.
    /// The source is a validated save-slot identity0..2. The game
    /// skips that identity while moving in either direction.</remarks>
    internal static FileSelectItem MoveCopyDestination(FileSelectItem selected, int sourceSlot, ControllerButton pressed)
    {
        if ((uint)sourceSlot >= FileSelectLayout.SaveSlotCount)
            throw new ArgumentOutOfRangeException(nameof(sourceSlot));
        return Move(selected, AllSlots & ~(1 << sourceSlot), pressed);
    }

    /// <summary>Validates the selected row, gives Up priority, and scans toward the next enabled slot or Exit without wrapping.</summary>
    private static FileSelectItem Move(FileSelectItem selected, int enabledSlots, ControllerButton pressed)
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
            (FileSelectItemRows.TryGetSaveSlot(item, out int slot) && (enabledSlots & (1 << slot)) != 0);
    }
}
