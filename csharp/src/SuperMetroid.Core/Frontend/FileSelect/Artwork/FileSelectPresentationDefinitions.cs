using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Schema names and fixed dimensions for <c>file-select.json</c>.</summary>
public static class FileSelectPresentationDefinitions
{
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
    /// <summary>Ten consecutive digit cells in the import's digit glyphs.</summary>
    public const int DigitCount = 10;
    /// <summary>Three consecutive save-slot letters in the import's slot-letter glyphs.</summary>
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
    internal static readonly string[] CursorSpriteNames = ["Cursor.0", "Cursor.1", "Cursor.2", "Cursor.3"];
    /// <summary>The JSON names of the helmet frame sprites, by frame.</summary>
    internal static readonly string[] HelmetSpriteNames =
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
}
