using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Chooses the file-copy arrow (<see cref="FileCopyArrowDefinitions"/>) between a source and a destination slot.
/// </summary>
internal static class FileCopyArrows
{
    /// <summary>
    /// The arrow from <paramref name="source"/> to <paramref name="destination"/>: short between neighbouring slots and
    /// long between A and C, pointing down or up toward the destination, at the height between the two slots.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The slots are the same slot or not save slots.</exception>
    internal static FileCopyArrowPlacement Select(SaveSlot source, SaveSlot destination) => source switch
    {
        SaveSlot.A => destination switch
        {
            SaveSlot.B => new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.UpperShortArrowY),
            SaveSlot.C => new(FileCopyArrowShape.LongDown, FileCopyArrowDefinitions.LongArrowY),
            SaveSlot.A => throw SameSlot(destination),
            _ => throw Undefined(destination),
        },
        SaveSlot.B => destination switch
        {
            SaveSlot.A => new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.UpperShortArrowY),
            SaveSlot.C => new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.LowerShortArrowY),
            SaveSlot.B => throw SameSlot(destination),
            _ => throw Undefined(destination),
        },
        SaveSlot.C => destination switch
        {
            SaveSlot.A => new(FileCopyArrowShape.LongUp, FileCopyArrowDefinitions.LongArrowY),
            SaveSlot.B => new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.LowerShortArrowY),
            SaveSlot.C => throw SameSlot(destination),
            _ => throw Undefined(destination),
        },
        _ => throw Undefined(source),
    };

    /// <summary>The failure of a copy onto its own slot.</summary>
    private static ArgumentOutOfRangeException SameSlot(SaveSlot slot) =>
        new("destination", slot, "A copy's destination is its source.");

    /// <summary>The failure of a slot that is not a save slot.</summary>
    private static ArgumentOutOfRangeException Undefined(SaveSlot slot) =>
        new("slot", slot, "Undefined save slot.");
}
