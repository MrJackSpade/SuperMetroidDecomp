namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Schema names and fixed dimensions for <c>file-select.json</c>.</summary>
public static class FileSelectPresentationDefinitions
{
    /// <summary>$81:B4AC-B4C0, the literal small-font empty-slot label including its padding.</summary>
    private const string NoDataLabel = " NO DATA   ";
    /// <summary>$81:B4B6: tile $6A is the small-font A; following alphabet glyphs are consecutive.</summary>
    private const ushort SmallLetterA = 0x206a;
    /// <summary>$81:B4B2 and B4BC-B4C0: the small-font paletted space.</summary>
    private const ushort SmallSpace = 0x200f;
    /// <summary>$81:B496-B49A: three consecutive tiles composing the ENERGY label.</summary>
    private const ushort EnergyLabelFirst = 0x209d;
    /// <summary>$81:B49C: the separately placed final glyph of the ENERGY patch.</summary>
    private const ushort EnergyLabelFinal = 0x20cc;
    /// <summary>$81:B4A8: the time-field colon glyph.</summary>
    private const ushort TimeColonGlyph = 0x208c;

    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static int PatchCellCount(string name) => name switch
    {
        EnergyPatch => 4,
        TimeColonPatch => 1,
        NoDataPatch => NoDataLabel.Length,
        _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
    };

    /// <summary>Calculates text glyph selection and left-to-right cell placement for one native patch.</summary>
    internal static FileSelectCompiledPatchCell PatchCell(string name, int index)
    {
        if ((uint)index >= PatchCellCount(name)) throw new IndexOutOfRangeException();
        ushort word = name switch
        {
            EnergyPatch => index < 3 ? (ushort)(EnergyLabelFirst + index) : EnergyLabelFinal,
            TimeColonPatch => TimeColonGlyph,
            NoDataPatch => index == 0 ? FileSelectLayout.BlankTile
                : NoDataLabel[index] == ' ' ? SmallSpace
                : (ushort)(SmallLetterA + NoDataLabel[index] - 'A'),
            _ => throw new InvalidDataException($"Unknown file-select patch {name}."),
        };
        return new(new Offset16<TilemapCells>(checked((short)index), 0), word);
    }

    /// <summary>Supported file-select presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for menu pages, fields, sprite compositions, and layout.</summary>
    public const string FileName = "file-select.json";
    /// <summary>Width of each menu tilemap page in tile cells.</summary>
    public const int Width = 32;
    /// <summary>Height of each menu tilemap page in tile cells.</summary>
    public const int Height = 32;
    /// <summary>Number of row-major tile cells required in each menu page.</summary>
    public const int CellCount = Width * Height;
    /// <summary>Size of each menu tilemap page in tile cells.</summary>
    public static Extent16<TilemapCells> PageSize => new(Width, Height);
    /// <summary>Width in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterColumns = 32;
    /// <summary>Height in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>Ten consecutive digit cells beginning at FileSelectLayout.DigitTileBase.</summary>
    public const int DigitCount = 10;
    /// <summary>Three consecutive save-slot letters beginning at FileSelectLayout.SamusLetterTileBase.</summary>
    public const int SlotLetterCount = 3;

    /// <summary>Schema identity of the common BG2 background tilemap, separate from foreground menu pages.</summary>
    public const string BackgroundPage = "Background";
    /// <summary>Schema identity of the main foreground page used when at least one save slot contains data.</summary>
    public const string MainWithDataPage = "Main.WithData";
    /// <summary>Schema identity of the main foreground page used when every save slot is empty.</summary>
    public const string MainEmptyPage = "Main.Empty";
    /// <summary>Schema identity of the copy page for choosing the source save slot.</summary>
    public const string CopySourcePage = "Copy.Source";
    /// <summary>Schema identity of the copy page for choosing a destination, with a dynamic source-slot letter.</summary>
    public const string CopyDestinationPage = "Copy.Destination";
    /// <summary>Schema identity of the copy confirmation page, displaying both selected slot letters.</summary>
    public const string CopyConfirmPage = "Copy.Confirm";
    /// <summary>Schema identity of the completed-copy page, retaining source and destination slot-letter fields.</summary>
    public const string CopyCompletedPage = "Copy.Completed";
    /// <summary>Schema identity of the clear page for choosing the save slot to erase.</summary>
    public const string ClearSelectionPage = "Clear.Selection";
    /// <summary>Schema identity of the clear confirmation page with its selected slot-letter field.</summary>
    public const string ClearConfirmPage = "Clear.Confirm";
    /// <summary>Schema identity of the completed-clear page with its selected slot-letter field.</summary>
    public const string ClearCompletedPage = "Clear.Completed";

    /// <summary>Schema identity of the ENERGY label patch; stock glyphs are calculated from the native label at $81:B496.</summary>
    public const string EnergyPatch = "Energy";
    /// <summary>Schema identity of the padded empty-slot NO DATA patch corresponding to $81:B4AC-B4C0.</summary>
    public const string NoDataPatch = "NoData";
    /// <summary>Schema identity of the time-field colon patch corresponding to the native glyph at $81:B4A8.</summary>
    public const string TimeColonPatch = "TimeColon";
    /// <summary>Layout identity of the main-menu border anchor and corresponding border sprite composition.</summary>
    public const string MainBorder = "Main";
    /// <summary>Layout identity of the copy-menu border anchor and corresponding border sprite composition.</summary>
    public const string CopyBorder = "Copy";
    /// <summary>Layout identity of the clear-menu border anchor and corresponding border sprite composition.</summary>
    public const string ClearBorder = "Clear";
    /// <summary>Tile-anchor identity for the source-slot letter on the copy destination-selection page.</summary>
    public const string CopyDestinationSourceAnchor = "Copy.Destination.Source";
    /// <summary>Tile-anchor identity for the source-slot letter on copy confirmation and completed-copy pages.</summary>
    public const string CopyConfirmSourceAnchor = "Copy.Confirm.Source";
    /// <summary>Tile-anchor identity for the destination-slot letter on copy confirmation and completed-copy pages.</summary>
    public const string CopyConfirmDestinationAnchor = "Copy.Confirm.Destination";
    /// <summary>Tile-anchor identity for the erased-slot letter on clear confirmation and completed-clear pages.</summary>
    public const string ClearConfirmSourceAnchor = "Clear.Confirm.Source";

    /// <summary>Exact required schema identities, enumerated in the original definition order.
    /// Each case names a distinct menu page or asset role; these are not numerical samples.
    /// The enumerators evaluate cases directly and retain no generated name arrays.</summary>
    public static IEnumerable<string> PageNames => Names(10, PageName);
    /// <summary>Enumerates the three exact required patch identities in energy, empty-slot, and time-colon order.</summary>
    public static IEnumerable<string> PatchNames => Names(3, PatchName);
    /// <summary>Enumerates the three exact required border layout identities in main, copy, and clear order.</summary>
    public static IEnumerable<string> BorderNames => Names(3, BorderName);
    /// <summary>Enumerates the four exact required tile-anchor identities for dynamic copy and clear slot letters.</summary>
    public static IEnumerable<string> DynamicAnchorNames => Names(4, DynamicAnchorName);
    /// <summary>Enumerates fifteen required sprite identities: three borders, four cursor frames, then eight helmet frames.</summary>
    public static IEnumerable<string> SpriteNames => Names(15, SpriteName);

    /// <summary>Returns a required page identity in the fixed schema enumeration order.</summary>
    /// <param name="index">Zero-based page index 0-9.</param>
    /// <returns>The corresponding background, main, copy, or clear page key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-9.</exception>
    public static string PageName(int index) => index switch
    {
        0 => BackgroundPage,
        1 => MainWithDataPage,
        2 => MainEmptyPage,
        3 => CopySourcePage,
        4 => CopyDestinationPage,
        5 => CopyConfirmPage,
        6 => CopyCompletedPage,
        7 => ClearSelectionPage,
        8 => ClearConfirmPage,
        9 => ClearCompletedPage,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns the patch identity at a fixed schema index.</summary>
    /// <param name="index">Zero-based index 0-2, selecting energy, empty-slot, or time-colon artwork.</param>
    /// <returns>The required patch dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-2.</exception>
    public static string PatchName(int index) => index switch
    {
        0 => EnergyPatch,
        1 => NoDataPatch,
        2 => TimeColonPatch,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns the border layout identity at a fixed schema index.</summary>
    /// <param name="index">Zero-based index 0-2, selecting main, copy, or clear.</param>
    /// <returns>The required border-anchor key, without the sprite's <c>Border.</c> prefix.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-2.</exception>
    public static string BorderName(int index) => index switch
    {
        0 => MainBorder,
        1 => CopyBorder,
        2 => ClearBorder,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns a required copy or clear slot-letter tile-anchor identity in fixed schema order.</summary>
    /// <param name="index">Zero-based dynamic-anchor index 0-3.</param>
    /// <returns>The required dynamic-anchor dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-3.</exception>
    public static string DynamicAnchorName(int index) => index switch
    {
        0 => CopyDestinationSourceAnchor,
        1 => CopyConfirmSourceAnchor,
        2 => CopyConfirmDestinationAnchor,
        3 => ClearConfirmSourceAnchor,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Returns a required sprite composition identity in border, cursor, then helmet order.</summary>
    /// <param name="index">Zero-based index: 0-2 for borders, 3-6 for cursor frames, or 7-14 for helmet frames.</param>
    /// <returns>The prefixed sprite dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-14.</exception>
    public static string SpriteName(int index) => index switch
    {
        >= 0 and < 3 => BorderFrameName(BorderName(index)),
        >= 3 and < 7 => CursorFrameName(index - 3),
        >= 7 and < 15 => HelmetFrameName(index - 7),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>Enumerates the expected schema keys in numeric index order.</summary>
    private static IEnumerable<string> Names(int count, Func<int, string> name)
    {
        for (int index = 0; index < count; index++) yield return name(index);
    }
    /// <summary>Prefixes a border layout name to form its sprite composition key, without validating the name.</summary>
    /// <param name="name">Border layout name, normally main, copy, or clear as defined by the schema.</param>
    /// <returns>The supplied name prefixed with <c>Border.</c>.</returns>
    public static string BorderFrameName(string name) => $"Border.{name}";
    /// <summary>Forms the sprite composition key for one of the four looping missile-cursor frames.</summary>
    /// <param name="frame">Zero-based animation frame 0-3.</param>
    /// <returns><c>Cursor.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public static string CursorFrameName(int frame) => (uint)frame < 4
        ? $"Cursor.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));
    /// <summary>Forms the sprite composition key for one of the eight selected-slot helmet frames.</summary>
    /// <param name="frame">Zero-based animation frame 0-7.</param>
    /// <returns><c>Helmet.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-7.</exception>
    public static string HelmetFrameName(int frame) => (uint)frame < 8
        ? $"Helmet.{frame}" : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>$82:CAE9: the helmet's first 24x24 body block begins at menu atlas cell $D0.</summary>
    internal const int HelmetFirstBodyCell = 0xd0;

    /// <summary>$82:CB2B: the helmet's first two-cell visor strip begins at menu atlas cell $D9.</summary>
    internal const int HelmetFirstVisorCell = 0xd9;
}
