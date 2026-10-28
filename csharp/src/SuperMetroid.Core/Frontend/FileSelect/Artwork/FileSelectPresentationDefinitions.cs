using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Schema names and fixed dimensions for <c>file-select.json</c>.</summary>
public static class FileSelectPresentationDefinitions
{
    /// <summary>$81:B4AC-B4C0, the literal small-font empty-slot label including its padding.</summary>
    internal const string NoDataLabel = " NO DATA   ";
    /// <summary>$81:B4B6: tile $6A is the small-font A; following alphabet glyphs are consecutive.</summary>
    internal static readonly BackgroundTile SmallLetterA = new(0x6a, 0, highPriority: true);
    /// <summary>$81:B4B2 and B4BC-B4C0: the small-font space.</summary>
    internal static readonly BackgroundTile SmallSpace = new(0x0f, 0, highPriority: true);
    /// <summary>$81:B496-B49A: three consecutive tiles composing the ENERGY label.</summary>
    internal static readonly BackgroundTile EnergyLabelFirst = new(0x9d, 0, highPriority: true);
    /// <summary>$81:B49C: the separately placed final glyph of the ENERGY patch.</summary>
    internal static readonly BackgroundTile EnergyLabelFinal = new(0xcc, 0, highPriority: true);
    /// <summary>$81:B4A8: the time-field colon glyph.</summary>
    internal static readonly BackgroundTile TimeColonGlyph = new(0x8c, 0, highPriority: true);

    /// <summary>Native one-row patch extents at $81:B496, B4A8 and B4AC, excluding terminators.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPatch, int> PatchCellCounts =
        new Dictionary<FileSelectPatch, int>
        {
            [FileSelectPatch.Energy] = 4,
            [FileSelectPatch.TimeColon] = 1,
            [FileSelectPatch.NoData] = NoDataLabel.Length,
        };

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
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>The selected slot's helmet animation frames.</summary>
    public const int HelmetFrameCount = 8;

    /// <summary>The JSON name of the common BG2 background tilemap, separate from the foreground menu pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground pages, in document order.</summary>
    public static IReadOnlyList<FileSelectPage> Pages { get; } = Enum.GetValues<FileSelectPage>();
    /// <summary>The slot-field patches, in document order.</summary>
    public static IReadOnlyList<FileSelectPatch> Patches { get; } = Enum.GetValues<FileSelectPatch>();
    /// <summary>The bordered headings, in document order.</summary>
    public static IReadOnlyList<FileSelectBorder> Borders { get; } = Enum.GetValues<FileSelectBorder>();
    /// <summary>The copy and clear pages' slot-letter anchors, in document order.</summary>
    public static IReadOnlyList<FileSelectAnchor> Anchors { get; } = Enum.GetValues<FileSelectAnchor>();
    /// <summary>The JSON names of the cursor frame sprites, by frame.</summary>
    private static readonly string[] cursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];
    /// <summary>The JSON names of the helmet frame sprites, by frame.</summary>
    private static readonly string[] helmetSpriteNames =
        ["Helmet.0", "Helmet.1", "Helmet.2", "Helmet.3", "Helmet.4", "Helmet.5", "Helmet.6", "Helmet.7"];

    /// <summary>The JSON name of a foreground page.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPage, string> PageNames =
        new Dictionary<FileSelectPage, string>
        {
            [FileSelectPage.MainWithData] = "Main.WithData",
            [FileSelectPage.MainEmpty] = "Main.Empty",
            [FileSelectPage.CopySource] = "Copy.Source",
            [FileSelectPage.CopyDestination] = "Copy.Destination",
            [FileSelectPage.CopyConfirm] = "Copy.Confirm",
            [FileSelectPage.CopyCompleted] = "Copy.Completed",
            [FileSelectPage.ClearSelection] = "Clear.Selection",
            [FileSelectPage.ClearConfirm] = "Clear.Confirm",
            [FileSelectPage.ClearCompleted] = "Clear.Completed",
        };

    /// <summary>The JSON name of a slot-field patch.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPatch, string> PatchNames =
        new Dictionary<FileSelectPatch, string>
        {
            [FileSelectPatch.Energy] = "Energy",
            [FileSelectPatch.NoData] = "NoData",
            [FileSelectPatch.TimeColon] = "TimeColon",
        };

    /// <summary>The JSON name of a bordered heading, which keys its anchor.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectBorder, string> BorderNames =
        new Dictionary<FileSelectBorder, string>
        {
            [FileSelectBorder.Main] = "Main",
            [FileSelectBorder.Copy] = "Copy",
            [FileSelectBorder.Clear] = "Clear",
        };

    /// <summary>The JSON name of a bordered heading's sprite.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectBorder, string> BorderSpriteNames =
        new Dictionary<FileSelectBorder, string>
        {
            [FileSelectBorder.Main] = "Border.Main",
            [FileSelectBorder.Copy] = "Border.Copy",
            [FileSelectBorder.Clear] = "Border.Clear",
        };

    /// <summary>The JSON name of a copy or clear page's slot-letter anchor.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectAnchor, string> AnchorNames =
        new Dictionary<FileSelectAnchor, string>
        {
            [FileSelectAnchor.CopyDestinationSource] = "Copy.Destination.Source",
            [FileSelectAnchor.CopyConfirmSource] = "Copy.Confirm.Source",
            [FileSelectAnchor.CopyConfirmDestination] = "Copy.Confirm.Destination",
            [FileSelectAnchor.ClearConfirmSource] = "Clear.Confirm.Source",
        };

    /// <summary>The JSON name of a looping missile-cursor frame's sprite, for frames 0-3.</summary>
    public static string CursorSpriteName(int frame) =>
        Ensure.Element(cursorSpriteNames, frame);

    /// <summary>The JSON name of a selected-slot helmet frame's sprite, for frames 0-7.</summary>
    public static string HelmetSpriteName(int frame) =>
        Ensure.Element(helmetSpriteNames, frame);

    /// <summary>Every sprite the document names: the three borders, the four cursor frames and the eight helmet frames.</summary>
    public static IEnumerable<string> SpriteNames =>
        [.. Borders.Select(FileSelectParts.BorderSpriteName), .. cursorSpriteNames, .. helmetSpriteNames];

    /// <summary>Every frame of the file-select sheet: each sprite the document names, then each file-copy arrow.</summary>
    public static IEnumerable<string> SheetFrames =>
        SpriteNames.Concat(Enumerable.Range(0, FileCopyArrowDefinitions.ShapeCount).Select(FileCopyArrowDefinitions.FrameName));

    /// <summary>The menu heading a bordered heading draws.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectBorder, MenuHeading> Headings =
        new Dictionary<FileSelectBorder, MenuHeading>
        {
            [FileSelectBorder.Main] = MenuHeading.FileSelectMain,
            [FileSelectBorder.Copy] = MenuHeading.FileSelectCopy,
            [FileSelectBorder.Clear] = MenuHeading.FileSelectClear,
        };

    /// <summary>$82:CAE9: the helmet's first 24x24 body block begins at menu atlas cell $D0.</summary>
    internal const int HelmetFirstBodyCell = 0xd0;

    /// <summary>$82:CB2B: the helmet's first two-cell visor strip begins at menu atlas cell $D9.</summary>
    internal const int HelmetFirstVisorCell = 0xd9;
}
