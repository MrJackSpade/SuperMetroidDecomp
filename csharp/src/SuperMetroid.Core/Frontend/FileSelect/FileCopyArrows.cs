using System.Collections.Frozen;
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
    /// <exception cref="ArgumentOutOfRangeException">The slots are the same slot.</exception>
    internal static FileCopyArrowPlacement Select(SaveSlot source, SaveSlot destination) => source != destination
        ? Arrows[new FileCopyRoute(source, destination)]
        : throw new ArgumentOutOfRangeException(nameof(destination), destination, "A copy's destination is its source.");

    /// <summary>The arrow between each pair of different slots, from the first to the second.</summary>
    private static FrozenDictionary<FileCopyRoute, FileCopyArrowPlacement> Arrows => field ??=
        new Dictionary<FileCopyRoute, FileCopyArrowPlacement>
        {
            [new(SaveSlot.A, SaveSlot.B)] = new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.UpperShortArrowY),
            [new(SaveSlot.A, SaveSlot.C)] = new(FileCopyArrowShape.LongDown, FileCopyArrowDefinitions.LongArrowY),
            [new(SaveSlot.B, SaveSlot.A)] = new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.UpperShortArrowY),
            [new(SaveSlot.B, SaveSlot.C)] = new(FileCopyArrowShape.ShortDown, FileCopyArrowDefinitions.LowerShortArrowY),
            [new(SaveSlot.C, SaveSlot.A)] = new(FileCopyArrowShape.LongUp, FileCopyArrowDefinitions.LongArrowY),
            [new(SaveSlot.C, SaveSlot.B)] = new(FileCopyArrowShape.ShortUp, FileCopyArrowDefinitions.LowerShortArrowY),
        }.ToFrozenDictionary();
}
