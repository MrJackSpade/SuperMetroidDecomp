using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File select's selection counts, cursor positions and the menu tile words its fields are drawn with.</summary>
/// <remarks>Shared missile animation is defined by <see cref="MenuMissileAnimationDefinitions"/>.</remarks>
internal static class FileSelectLayout
{
    /// <summary>Three save slots on each file-select page.</summary>
    public const int SaveSlotCount = 3;
    /// <summary>Slots A-C followed by Copy, Clear and Exit at $81:A312.</summary>
    public const int MainSelectionCount = 6;
    /// <summary>Slots A-C followed by Exit at $81:9772 and $81:9C03.</summary>
    public const int DataSelectionCount = 4;

    /// <summary>Calculates $81:A312+4*selection Y for main choices 0..5.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: save rows start at48 with five-tile spacing;
    /// Copy/Clear/Exit start at163 with three-tile spacing. These two role groups
    /// follow the native text layout. Bounds are checked before arithmetic and preserve
    /// ArgumentOutOfRangeException. X is the separate constant14.
    /// </remarks>
    public static ushort MainSelectionY(int selection)
    {
        if ((uint)selection >= MainSelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return (ushort)(selection < SaveSlotCount ? 48 + 40 * selection
            : 163 + 24 * (selection - SaveSlotCount));
    }

    /// <summary>Calculates copy/clear cursor Y from $81:9772 and $81:9C03, choices0..3.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: the three save choices start at72 with four-tile
    /// spacing. Exit shares the main-page Exit Y. Both original tables and the extractor's
    /// former duplicate are one logical mapping; unsupported indices remain rejected.
    /// </remarks>
    public static ushort DataSelectionY(int selection)
    {
        if ((uint)selection >= DataSelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return selection < SaveSlotCount ? (ushort)(72 + 32 * selection)
            : MainSelectionY(MainSelectionCount - 1);
    }

    /// <summary>Save-slot helmet Y from immediate operands $81:A028/A02E/A034.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: slots0..2 use the main slot cursor anchor minus
    /// one pixel, giving the same five-tile spacing. Reject unsupported slot indices.
    /// </remarks>
    public static ushort HelmetY(int slot)
    {
        if ((uint)slot >= SaveSlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
        return (ushort)(MainSelectionY(slot) - 1);
    }

    /// <summary>Blank bank-$81 menu tile.</summary>
    public const ushort BlankTile = 0x000f;
    /// <summary>First numeric menu tile; a decimal digit is added to this word.</summary>
    public const ushort DigitTileBase = 0x2060;
    /// <summary>First letter tile used to draw a save-slot name in Copy and Clear prompts.</summary>
    public const ushort SamusLetterTileBase = 0x206a;
}
