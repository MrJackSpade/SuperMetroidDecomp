using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// File select's rows on each page, from the top (#2359): the order the cursor moves through and the order of each page's
/// installed cursor anchors; and the save slot each slot row selects.
/// </summary>
internal static partial class FileSelectItemRows
{
    /// <summary>The main page's rows while a save exists: the save slots in slot order, Copy, Clear and Exit.</summary>
    internal static MenuRowOrder<FileSelectItem> MainWithData { get; } = new(
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Copy, FileSelectItem.Clear, FileSelectItem.Exit]);

    /// <summary>The main page's rows while every slot is empty: the save slots and Exit, Copy and Clear hidden.</summary>
    internal static MenuRowOrder<FileSelectItem> MainEmpty { get; } = new(
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Exit]);

    /// <summary>The copy and clear pages' rows: the save slots and Exit.</summary>
    internal static MenuRowOrder<FileSelectItem> Data { get; } = new(
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Exit]);

    /// <summary>The copy and clear confirmations' rows: Yes above No.</summary>
    internal static MenuRowOrder<YesNoChoice> Confirmation { get; } = new([YesNoChoice.Yes, YesNoChoice.No]);

    /// <summary>The main page's rows: with Copy and Clear while a save exists, without them while every slot is empty.</summary>
    internal static MenuRowOrder<FileSelectItem> Main(bool hasAnySave) => hasAnySave ? MainWithData : MainEmpty;

    /// <summary>The item that selects <paramref name="slot"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one of the three save slots.</exception>
    internal static FileSelectItem ForSaveSlot(SaveSlot slot) => slot switch
    {
        SaveSlot.A => FileSelectItem.SlotA,
        SaveSlot.B => FileSelectItem.SlotB,
        SaveSlot.C => FileSelectItem.SlotC,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Undefined save slot."),
    };

    /// <summary>Whether an item selects a save slot, and which.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The item is not a file-select row.</exception>
    internal static bool TryGetSaveSlot(FileSelectItem item, out SaveSlot slot)
    {
        switch (item)
        {
            case FileSelectItem.SlotA:
                slot = SaveSlot.A;
                return true;
            case FileSelectItem.SlotB:
                slot = SaveSlot.B;
                return true;
            case FileSelectItem.SlotC:
                slot = SaveSlot.C;
                return true;
            case FileSelectItem.Copy:
            case FileSelectItem.Clear:
            case FileSelectItem.Exit:
                slot = default;
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(item), item, "Undefined file-select item.");
        }
    }

    /// <summary>The save slot an item selects.</summary>
    /// <exception cref="InvalidDataException">The item is not a save slot.</exception>
    internal static SaveSlot SaveSlotOf(FileSelectItem item) =>
        TryGetSaveSlot(item, out SaveSlot slot) ? slot : throw new InvalidDataException($"File-select item {item} is not a save slot.");
}
