using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Native BG1 byte offsets, OBJ identifiers, and positions for file select.</summary>
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

    /// <summary>Main slot field destinations from native $81:9F16..9FAC.</summary>
    /// <remarks>Three slots start on tile row 5, separated by five rows. Labels,
    /// energy and time labels occupy columns 3, 14 and 27; time values start on
    /// the following row at column 26. Convert 32-column cells to byte offsets.</remarks>
    public static int MainSlotDestination(int slot, FileSelectSlotField field) =>
        SlotDestination(slot, field, page: FileSelectSlotPage.Main);

    /// <summary>Copy/clear slot field destinations from native $81:960F..9699.</summary>
    /// <remarks>Three slots start on tile row 8, separated by four rows. Labels,
    /// energy and time labels occupy columns 4, 12 and 26; time values start on
    /// the following row at column 25. Both Copy and Clear share this geometry.</remarks>
    public static int DataSlotDestination(int slot, FileSelectSlotField field) =>
        SlotDestination(slot, field, page: FileSelectSlotPage.DataManagement);

    /// <summary>Converts a slot field's row and column on the selected page to a BG1 byte offset.</summary>
    private static int SlotDestination(int slot, FileSelectSlotField field, FileSelectSlotPage page)
    {
        if ((uint)slot >= SaveSlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
        int column = field switch
        {
            FileSelectSlotField.Label => page == FileSelectSlotPage.DataManagement ? 4 : 3,
            FileSelectSlotField.Energy => page == FileSelectSlotPage.DataManagement ? 12 : 14,
            FileSelectSlotField.TimeValue => page == FileSelectSlotPage.DataManagement ? 25 : 26,
            FileSelectSlotField.TimeLabel => page == FileSelectSlotPage.DataManagement ? 26 : 27,
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        int row = page == FileSelectSlotPage.DataManagement ? 8 + 4 * slot : 5 + 5 * slot;
        if (field == FileSelectSlotField.TimeValue) row++;
        return (row * 32 + column) * sizeof(ushort);
    }

    /// <summary>Main file-select border spritemap.</summary>
    public const ushort NormalBorderSpritemap = 0x48;
    /// <summary>Copy-menu border spritemap.</summary>
    public const ushort CopyBorderSpritemap = 0x49;
    /// <summary>Clear-menu border spritemap.</summary>
    public const ushort ClearBorderSpritemap = 0x4a;
    /// <summary>Blank bank-$81 menu tile.</summary>
    public const ushort BlankTile = 0x000f;
    /// <summary>First numeric menu tile; a decimal digit is added to this word.</summary>
    public const ushort DigitTileBase = 0x2060;
    /// <summary>Bytes in one 32-word menu tilemap row.</summary>
    public const int NextTilemapRowByteOffset = 0x40;

    /// <summary>BG1 byte destinations for the main file-select labels and fields.</summary>
    public const int SamusDataDestination = 0x056;
    /// <summary>BG1 destination of the main menu's Copy label.</summary>
    public const int DataCopyDestination = 0x508;
    /// <summary>BG1 destination of the main menu's Clear label.</summary>
    public const int DataClearDestination = 0x5c8;
    /// <summary>BG1 destination of the main menu's Exit label.</summary>
    public const int ExitDestination = 0x688;

    /// <summary>BG1 byte destinations used by Copy and Clear prompts.</summary>
    public const int DataModeCopyDestination = 0x52;
    /// <summary>BG1 destination of the Clear mode heading.</summary>
    public const int DataModeClearDestination = 0x50;
    /// <summary>BG1 destination of the source-slot prompt during Copy.</summary>
    public const int CopySourcePromptDestination = 0x150;
    /// <summary>BG1 destination of the destination-slot prompt during Copy.</summary>
    public const int CopyDestinationPromptDestination = 0x148;
    /// <summary>BG1 destination of the confirmation prompt during Copy.</summary>
    public const int CopyConfirmationPromptDestination = 0x144;
    /// <summary>BG1 destination of the slot-selection prompt during Clear.</summary>
    public const int ClearPromptDestination = 0x140;
    /// <summary>BG1 destination of the source slot letter in Copy's destination prompt.</summary>
    public const int CopyDestinationSourceLetterDestination = 0x160;
    /// <summary>BG1 destination of the source slot letter in Copy's confirmation prompt.</summary>
    public const int CopyConfirmationSourceLetterDestination = 0x15c;
    /// <summary>BG1 destination of the destination slot letter in Copy's confirmation prompt.</summary>
    public const int CopyConfirmationDestinationLetterDestination = 0x176;
    /// <summary>BG1 destination of the source slot letter in Clear's confirmation prompt.</summary>
    public const int ClearConfirmationSourceLetterDestination = 0x16a;
    /// <summary>First letter tile used to draw a save-slot name in Copy and Clear prompts.</summary>
    public const ushort SamusLetterTileBase = 0x206a;
    /// <summary>BG1 destination of the message shown after copying a slot.</summary>
    public const int CopyCompletedDestination = 0x510;
    /// <summary>BG1 destination of the message shown after clearing a slot.</summary>
    public const int DataClearedDestination = 0x500;

    /// <summary>BG1 byte destinations for the copy/clear confirmation choices.</summary>
    public const int ConfirmationQuestionDestination = 0x514;
    /// <summary>BG1 destination of the confirmation Yes choice.</summary>
    public const int ConfirmationYesDestination = 0x59c;
    /// <summary>BG1 destination of the confirmation No choice.</summary>
    public const int ConfirmationNoDestination = 0x65c;
}
