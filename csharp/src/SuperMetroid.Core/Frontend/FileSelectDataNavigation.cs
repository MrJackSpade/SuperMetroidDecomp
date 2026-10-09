using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Bounded copy/clear row selection from $81:96C2, $81:9813 and $81:9B64.</summary>
internal static class FileSelectDataNavigation
{
    private const int AllSlots = (1 << FileSelectLayout.SaveSlotCount) - 1;

    /// <summary>Moves among occupied save slots and Exit, without wrapping.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against native source/clear selection and
    /// the prior managed filtered lists. Bits0..2 represent the live occupancy of
    /// slots A..C; the native bit selectors at $81:975B and $81:9BEC are 1 shifted
    /// by the slot. Exit3 is always selectable, including an empty occupancy mask.
    /// No ordinal lookup or generated list is required.
    /// </remarks>
    internal static int MoveSourceOrClear(int selected, int occupiedSlots, SnesButton pressed)
    {
        if ((uint)occupiedSlots > AllSlots)
            throw new ArgumentOutOfRangeException(nameof(occupiedSlots));
        return Move(selected, occupiedSlots, pressed);
    }

    /// <summary>Moves among the other two save slots and Exit, without wrapping.</summary>
    /// <remarks>Copy destinations exclude the source regardless of occupancy.
    /// The source is a validated save-slot identity0..2. Native $81:983D..9848
    /// and $81:98A5..98AD skip that identity while moving in either direction.</remarks>
    internal static int MoveCopyDestination(int selected, int sourceSlot, SnesButton pressed)
    {
        if ((uint)sourceSlot >= FileSelectLayout.SaveSlotCount)
            throw new ArgumentOutOfRangeException(nameof(sourceSlot));
        return Move(selected, AllSlots & ~(1 << sourceSlot), pressed);
    }

    private static int Move(int selected, int enabledSlots, SnesButton pressed)
    {
        const int exit = FileSelectLayout.DataSelectionCount - 1;
        if ((uint)selected > exit || !Selectable(selected))
            throw new InvalidDataException($"Submenu item {selected} is not selectable.");

        // Preserve the managed input order, including validation on idle frames
        // and Up priority when both directions are pressed. The caller owns sound.
        int step = (pressed & SnesButton.Up) != 0 ? -1
            : (pressed & SnesButton.Down) != 0 ? 1 : 0;
        if (step == 0) return selected;
        for (int next = selected + step; next is >= 0 and <= exit; next += step)
            if (Selectable(next)) return next;
        return selected;

        bool Selectable(int row) => row == exit || (enabledSlots & (1 << row)) != 0;
    }
}
