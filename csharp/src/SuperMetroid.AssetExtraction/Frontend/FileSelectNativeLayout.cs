using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Where bank $81's file-select routines write each label, prompt and save-slot field into the menu's BG1 tilemap, as
/// byte offsets into its 32-word rows, and the native spritemaps of its borders. Extraction places the cartridge's text
/// patches by these offsets once; the installed presentation carries the resulting cells and anchors.
/// </summary>
[EncodingOwner("bank-$81 file-select tilemap offsets and border spritemaps")]
internal static class FileSelectNativeLayout
{
    /// <summary>Main file-select border spritemap.</summary>
    public const ushort NormalBorderSpritemap = 0x48;
    /// <summary>Copy-menu border spritemap.</summary>
    public const ushort CopyBorderSpritemap = 0x49;
    /// <summary>Clear-menu border spritemap.</summary>
    public const ushort ClearBorderSpritemap = 0x4a;
    /// <summary>Bytes in one 32-word menu tilemap row.</summary>
    public const int NextTilemapRowByteOffset = 0x40;

    /// <summary>BG1 byte offset of the main file-select heading.</summary>
    public const int SamusDataDestination = 0x056;
    /// <summary>BG1 byte offset of the main menu's Copy label.</summary>
    public const int DataCopyDestination = 0x508;
    /// <summary>BG1 byte offset of the main menu's Clear label.</summary>
    public const int DataClearDestination = 0x5c8;
    /// <summary>BG1 byte offset of the main menu's Exit label.</summary>
    public const int ExitDestination = 0x688;

    /// <summary>BG1 byte offset of the Copy mode heading.</summary>
    public const int DataModeCopyDestination = 0x52;
    /// <summary>BG1 byte offset of the Clear mode heading.</summary>
    public const int DataModeClearDestination = 0x50;
    /// <summary>BG1 byte offset of the source-slot prompt during Copy.</summary>
    public const int CopySourcePromptDestination = 0x150;
    /// <summary>BG1 byte offset of the destination-slot prompt during Copy.</summary>
    public const int CopyDestinationPromptDestination = 0x148;
    /// <summary>BG1 byte offset of the confirmation prompt during Copy.</summary>
    public const int CopyConfirmationPromptDestination = 0x144;
    /// <summary>BG1 byte offset of the slot-selection prompt during Clear.</summary>
    public const int ClearPromptDestination = 0x140;
    /// <summary>BG1 byte offset of the source slot letter in Copy's destination prompt.</summary>
    public const int CopyDestinationSourceLetterDestination = 0x160;
    /// <summary>BG1 byte offset of the source slot letter in Copy's confirmation prompt.</summary>
    public const int CopyConfirmationSourceLetterDestination = 0x15c;
    /// <summary>BG1 byte offset of the destination slot letter in Copy's confirmation prompt.</summary>
    public const int CopyConfirmationDestinationLetterDestination = 0x176;
    /// <summary>BG1 byte offset of the source slot letter in Clear's confirmation prompt.</summary>
    public const int ClearConfirmationSourceLetterDestination = 0x16a;
    /// <summary>BG1 byte offset of the message shown after copying a slot.</summary>
    public const int CopyCompletedDestination = 0x510;
    /// <summary>BG1 byte offset of the message shown after clearing a slot.</summary>
    public const int DataClearedDestination = 0x500;

    /// <summary>BG1 byte offset of the copy/clear confirmation question.</summary>
    public const int ConfirmationQuestionDestination = 0x514;
    /// <summary>BG1 byte offset of the confirmation Yes choice.</summary>
    public const int ConfirmationYesDestination = 0x59c;
    /// <summary>BG1 byte offset of the confirmation No choice.</summary>
    public const int ConfirmationNoDestination = 0x65c;

    /// <summary>Main slot field byte offsets from native $81:9F16..9FAC.</summary>
    /// <remarks>Three slots start on tile row 5, separated by five rows. Labels,
    /// energy and time labels occupy columns 3, 14 and 27; time values start on
    /// the following row at column 26.</remarks>
    public static int MainSlotDestination(int slot, FileSelectSlotField field) =>
        SlotDestination(slot, field, page: FileSelectSlotPage.Main);

    /// <summary>Copy/clear slot field byte offsets from native $81:960F..9699.</summary>
    /// <remarks>Three slots start on tile row 8, separated by four rows. Labels,
    /// energy and time labels occupy columns 4, 12 and 26; time values start on
    /// the following row at column 25. Both Copy and Clear share this geometry.</remarks>
    public static int DataSlotDestination(int slot, FileSelectSlotField field) =>
        SlotDestination(slot, field, page: FileSelectSlotPage.DataManagement);

    /// <summary>Converts a slot field's row and column on the selected page to a BG1 byte offset.</summary>
    private static int SlotDestination(int slot, FileSelectSlotField field, FileSelectSlotPage page)
    {
        if ((uint)slot >= SuperMetroid.Core.Frontend.FileSelect.FileSelectLayout.SaveSlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot));
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
}
