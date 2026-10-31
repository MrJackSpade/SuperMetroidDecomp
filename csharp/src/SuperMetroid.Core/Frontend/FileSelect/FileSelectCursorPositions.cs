using static SuperMetroid.Core.Frontend.FileSelect.FileSelectLayout;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Places the file-select cursors (<see cref="FileSelectLayout"/>): the main and copy/clear selection rows and each save slot's
/// helmet.
/// </summary>
internal static class FileSelectCursorPositions
{
    /// <summary>Calculates $81:A312+4*selection Y for main choices 0..5.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: save rows start at48 with five-tile spacing;
    /// Copy/Clear/Exit start at163 with three-tile spacing. These two role groups
    /// follow the native text layout. Bounds are checked before arithmetic and preserve
    /// ArgumentOutOfRangeException. X is the separate constant14.
    /// </remarks>
    public static int MainSelectionY(int selection)
    {
        if ((uint)selection >= MainSelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return WordWrap.Unsigned(selection < SaveSlotCount ? 48 + 40 * selection
            : 163 + 24 * (selection - SaveSlotCount));
    }

    /// <summary>Calculates copy/clear cursor Y from $81:9772 and $81:9C03, choices0..3.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: the three save choices start at72 with four-tile
    /// spacing. Exit shares the main-page Exit Y. Both original tables and the extractor's
    /// former duplicate are one logical mapping; unsupported indices remain rejected.
    /// </remarks>
    public static int DataSelectionY(int selection)
    {
        if ((uint)selection >= DataSelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return selection < SaveSlotCount ? WordWrap.Unsigned(72 + 32 * selection)
            : MainSelectionY(MainSelectionCount - 1);
    }

    /// <summary>Save-slot helmet Y from immediate operands $81:A028/A02E/A034.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: slots0..2 use the main slot cursor anchor minus
    /// one pixel, giving the same five-tile spacing. Reject unsupported slot indices.
    /// </remarks>
    public static int HelmetY(int slot)
    {
        if ((uint)slot >= SaveSlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
        return WordWrap.Unsigned(MainSelectionY(slot) - 1);
    }
}
