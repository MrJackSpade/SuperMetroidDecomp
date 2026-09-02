namespace SuperMetroid.Core.Frontend;

/// <summary>Native BG1 byte offsets, OBJ identifiers, and positions for file select.</summary>
internal static class FileSelectLayout
{
    /// <summary>Screen-space Y coordinates for slots A-C, Copy, Clear, and Exit.</summary>
    public static readonly ushort[] MainSelectionY = [48, 88, 128, 163, 187, 211];
    /// <summary>Screen-space Y coordinates of the three save-slot helmet actors.</summary>
    public static readonly ushort[] HelmetY = [47, 87, 127];
    /// <summary>Four-frame menu missile animation, in native playback order.</summary>
    public static readonly ushort[] MissileSpritemapIds = [0x37, 0x36, 0x35, 0x34];

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
