using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>
/// Places the file-select cursors: the main and copy/clear selection rows and each save slot's helmet.
/// </summary>
internal static class FileSelectCursorPositions
{
    /// <summary>The save slots, A-C, each file-select page lists.</summary>
    internal const int SaveSlotCount = 3;

    /// <summary>The main-page cursor's X, the same for every choice.</summary>
    internal const int MainCursorX = 14;

    /// <summary>The copy/clear cursor's X, the same for every choice.</summary>
    internal const int DataCursorX = 22;

    /// <summary>Each save slot's helmet's X, the same for every slot.</summary>
    internal const int HelmetX = 100;

    /// <summary>Where the copy/clear confirmation's cursor stands beside Yes.</summary>
    internal static Offset16<ScreenPixels> ConfirmationYesAnchor { get; } = new(94, 184);

    /// <summary>Where the copy/clear confirmation's cursor stands beside No.</summary>
    internal static Offset16<ScreenPixels> ConfirmationNoAnchor { get; } = new(94, 208);

    /// <summary>Calculates the main-page cursor Y beside <paramref name="row"/>.</summary>
    /// <remarks>
    /// The save rows lie five tiles apart from the first slot's row down; Copy, Clear and Exit lie three tiles apart
    /// below them, following the menu's text layout (<see cref="FileSelectItemRows.MainWithData"/>). A row not on the page
    /// throws <see cref="ArgumentOutOfRangeException"/>. X is the separate <see cref="MainCursorX"/>.
    /// </remarks>
    public static int MainSelectionY(FileSelectItem row)
    {
        if (!FileSelectItemRows.MainWithData.Contains(row)) throw new ArgumentOutOfRangeException(nameof(row), row, "Not a main-page row.");
        int rowsAbove = FileSelectItemRows.MainWithData.PlaceOf(row);
        return rowsAbove < SaveSlotCount ? 48 + 40 * rowsAbove
            : 163 + 24 * (rowsAbove - SaveSlotCount);
    }

    /// <summary>Calculates the copy/clear cursor Y for the save slots and Exit.</summary>
    /// <remarks>
    /// The save choices lie four tiles apart from the first slot's row down; Exit shares the main page's Exit row.
    /// A choice outside the page throws <see cref="ArgumentOutOfRangeException"/>.
    /// </remarks>
    public static int DataSelectionY(FileSelectItem row)
    {
        if (!FileSelectItemRows.Data.Contains(row)) throw new ArgumentOutOfRangeException(nameof(row), row, "Not a copy or clear page row.");
        return row == FileSelectItem.Exit ? MainSelectionY(FileSelectItem.Exit)
            : 72 + 32 * FileSelectItemRows.Data.PlaceOf(row);
    }

    /// <summary>Save-slot helmet Y.</summary>
    /// <remarks>
    /// Each slot's helmet lies one pixel above the main page's cursor row for that slot, so the helmets share the rows'
    /// five-tile spacing. A slot outside the page throws <see cref="ArgumentOutOfRangeException"/>.
    /// </remarks>
    public static int HelmetY(SaveSlot slot) => MainSelectionY(FileSelectItemRows.ForSaveSlot(slot)) - 1;

    /// <summary>The save slots, in the order the installed helmet anchors list them.</summary>
    internal static IReadOnlyList<SaveSlot> Slots { get; } = [SaveSlot.A, SaveSlot.B, SaveSlot.C];
}
