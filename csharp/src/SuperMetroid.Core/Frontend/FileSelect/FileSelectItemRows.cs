using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Orders file-select items into the main and data-management page rows and maps save slots to their items.</summary>
internal static class FileSelectItemRows
{
    /// <summary>Main-page rows from top to bottom; the save slots lead, in slot order.</summary>
    private static readonly FileSelectItem[] MainRows =
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Copy, FileSelectItem.Clear, FileSelectItem.Exit];

    /// <summary>Copy and clear page rows from top to bottom.</summary>
    private static readonly FileSelectItem[] DataRows =
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Exit];

    /// <summary>The main-page row an item occupies.</summary>
    /// <exception cref="InvalidDataException">The item is not a main-page row.</exception>
    internal static int MainRow(FileSelectItem item) => Row(MainRows, item, "main");

    /// <summary>The item on a main-page row.</summary>
    internal static FileSelectItem MainItem(int row) => MainRows[row];

    /// <summary>The copy/clear page row an item occupies.</summary>
    /// <exception cref="InvalidDataException">The item is not a data-management row.</exception>
    internal static int DataRow(FileSelectItem item) => Row(DataRows, item, "data-management");

    /// <summary>The item on a copy/clear page row.</summary>
    internal static FileSelectItem DataItem(int row) => DataRows[row];

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

    /// <summary>Finds an item's row on one page.</summary>
    private static int Row(FileSelectItem[] rows, FileSelectItem item, string page)
    {
        int row = Array.IndexOf(rows, item);
        return row >= 0 ? row : throw new InvalidDataException($"File-select item {item} is not on the {page} page.");
    }
}
