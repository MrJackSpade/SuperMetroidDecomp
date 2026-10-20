namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Orders file-select items into the main and data-management page rows and maps save slots to their items.</summary>
internal static class FileSelectItemRows
{
    /// <summary>Main-page rows from top to bottom ($81:A312); the save slots lead, in slot order.</summary>
    private static readonly FileSelectItem[] MainRows =
        [FileSelectItem.SlotA, FileSelectItem.SlotB, FileSelectItem.SlotC, FileSelectItem.Copy, FileSelectItem.Clear, FileSelectItem.Exit];

    /// <summary>Copy and clear page rows from top to bottom ($81:9772, $81:9C03).</summary>
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

    /// <summary>The item that selects a save slot.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The slot is not one of the three save slots.</exception>
    internal static FileSelectItem ForSaveSlot(int slot) =>
        (uint)slot < FileSelectLayout.SaveSlotCount ? MainRows[slot] : throw new ArgumentOutOfRangeException(nameof(slot));

    /// <summary>Whether an item selects a save slot, and which.</summary>
    internal static bool TryGetSaveSlot(FileSelectItem item, out int slot)
    {
        slot = Array.IndexOf(MainRows, item);
        return slot is >= 0 and < FileSelectLayout.SaveSlotCount;
    }

    /// <summary>The save slot an item selects.</summary>
    /// <exception cref="InvalidDataException">The item is not a save slot.</exception>
    internal static int SaveSlot(FileSelectItem item) =>
        TryGetSaveSlot(item, out int slot) ? slot : throw new InvalidDataException($"File-select item {item} is not a save slot.");

    /// <summary>Finds an item's row on one page.</summary>
    private static int Row(FileSelectItem[] rows, FileSelectItem item, string page)
    {
        int row = Array.IndexOf(rows, item);
        return row >= 0 ? row : throw new InvalidDataException($"File-select item {item} is not on the {page} page.");
    }
}
