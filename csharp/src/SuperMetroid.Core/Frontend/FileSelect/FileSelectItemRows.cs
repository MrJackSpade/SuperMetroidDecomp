using System.Collections.Frozen;
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

    /// <summary>The for save slot of each slot.</summary>
    private static IdentityTable<SaveSlot, FileSelectItem> ForSaveSlotBySlot => field ??= new(new Dictionary<SaveSlot, FileSelectItem>
    {
        [SaveSlot.A] = FileSelectItem.SlotA,
        [SaveSlot.B] = FileSelectItem.SlotB,
        [SaveSlot.C] = FileSelectItem.SlotC,
    });

    /// <summary>The item that selects <paramref name="slot"/>.</summary>
    internal static FileSelectItem ForSaveSlot(SaveSlot slot) => ForSaveSlotBySlot[slot];

    /// <summary>Whether an item selects a save slot, and which.</summary>
    internal static bool TryGetSaveSlot(FileSelectItem item, out SaveSlot slot) => SlotsByItem.TryGetValue(item, out slot);

    /// <summary>The save slot each slot item selects; Copy, Clear and Exit select none.</summary>
    private static FrozenDictionary<FileSelectItem, SaveSlot> SlotsByItem => field ??= new Dictionary<FileSelectItem, SaveSlot>
    {
        [FileSelectItem.SlotA] = SaveSlot.A,
        [FileSelectItem.SlotB] = SaveSlot.B,
        [FileSelectItem.SlotC] = SaveSlot.C,
    }.ToFrozenDictionary();
}
