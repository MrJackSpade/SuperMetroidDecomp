namespace SuperMetroid.Core.Frontend;

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
    /// the former array's IndexOutOfRangeException. X is the separate constant14.
    /// </remarks>
    public static ushort MainSelectionY(int selection)
    {
        if ((uint)selection >= MainSelectionCount) throw new IndexOutOfRangeException();
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
        if ((uint)selection >= DataSelectionCount) throw new IndexOutOfRangeException();
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
        if ((uint)slot >= SaveSlotCount) throw new IndexOutOfRangeException();
        return (ushort)(MainSelectionY(slot) - 1);
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
    public const int SlotALabelDestination = 0x146;
    public const int SlotAEnergyDestination = 0x15c;
    public const int SlotATimeValueDestination = 0x1b4;
    public const int SlotATimeLabelDestination = 0x176;
    public const int SlotBLabelDestination = 0x286;
    public const int SlotBEnergyDestination = 0x29c;
    public const int SlotBTimeValueDestination = 0x2f4;
    public const int SlotBTimeLabelDestination = 0x2b6;
    public const int SlotCLabelDestination = 0x3c6;
    public const int SlotCEnergyDestination = 0x3dc;
    public const int SlotCTimeValueDestination = 0x434;
    public const int SlotCTimeLabelDestination = 0x3f6;
    public const int DataCopyDestination = 0x508;
    public const int DataClearDestination = 0x5c8;
    public const int ExitDestination = 0x688;

    /// <summary>BG1 byte destinations used by Copy and Clear prompts.</summary>
    public const int DataModeCopyDestination = 0x52;
    public const int DataModeClearDestination = 0x50;
    public const int CopySourcePromptDestination = 0x150;
    public const int CopyDestinationPromptDestination = 0x148;
    public const int CopyConfirmationPromptDestination = 0x144;
    public const int ClearPromptDestination = 0x140;
    public const int CopyDestinationSourceLetterDestination = 0x160;
    public const int CopyConfirmationSourceLetterDestination = 0x15c;
    public const int CopyConfirmationDestinationLetterDestination = 0x176;
    public const int ClearConfirmationSourceLetterDestination = 0x16a;
    public const ushort SamusLetterTileBase = 0x206a;
    public const int CopyCompletedDestination = 0x510;
    public const int DataClearedDestination = 0x500;

    /// <summary>BG1 byte destinations for slots shown on data-management pages.</summary>
    public const int DataSlotALabelDestination = 0x208;
    public const int DataSlotAEnergyDestination = 0x218;
    public const int DataSlotATimeValueDestination = 0x272;
    public const int DataSlotATimeLabelDestination = 0x234;
    public const int DataSlotBLabelDestination = 0x308;
    public const int DataSlotBEnergyDestination = 0x318;
    public const int DataSlotBTimeValueDestination = 0x372;
    public const int DataSlotBTimeLabelDestination = 0x334;
    public const int DataSlotCLabelDestination = 0x408;
    public const int DataSlotCEnergyDestination = 0x418;
    public const int DataSlotCTimeValueDestination = 0x472;
    public const int DataSlotCTimeLabelDestination = 0x434;
    public const int ConfirmationQuestionDestination = 0x514;
    public const int ConfirmationYesDestination = 0x59c;
    public const int ConfirmationNoDestination = 0x65c;
}

/// <summary>Control words in the bank-$81 file-select tilemap stream format.</summary>
internal static class FileSelectTilemapFormat
{
    /// <summary>Bank containing the file-select text streams.</summary>
    public const int Bank = 0x810000;
    /// <summary>Terminates one tilemap text stream.</summary>
    public const ushort End = 0xffff;
    /// <summary>Moves the output cursor to the same column on the following row.</summary>
    public const ushort NextRow = 0xfffe;
    /// <summary>Bytes per 32-word BG tilemap row.</summary>
    public const int RowByteCount = 64;
}
