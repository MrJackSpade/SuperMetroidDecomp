using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>File identities, schema names and fixed dimensions of the installed <c>file-select.json</c> and its import-data artwork.</summary>
public static class FileSelectPresentationFormat
{
    /// <summary>
    /// Supported file-select layout JSON schema version: 2 holds each slot field's and slot letter's anchor as the page pixel
    /// the game shows it at, and 3 holds only the layout the game binds, the artwork being import data (#1638).
    /// </summary>
    public const int Version = 3;
    /// <summary>Installed JSON filename for the slot fields, anchors and durations the game binds.</summary>
    public const string FileName = "file-select.json";
    /// <summary>Supported file-select artwork JSON schema version.</summary>
    public const int ArtworkVersion = 1;
    /// <summary>Import-data JSON filename for the menu pages, field patches, glyphs and sprite compositions (#1638).</summary>
    public const string ArtworkFileName = "file-select-artwork.json";
    /// <summary>Cells across each menu page.</summary>
    public const int PageColumns = FileSelectPresentationDefinitions.PageWidthPixels / CharacterCellConversions.CellPixels;
    /// <summary>Cells down each menu page.</summary>
    public const int PageRows = FileSelectPresentationDefinitions.PageHeightPixels / CharacterCellConversions.CellPixels;
    /// <summary>Number of row-major tile cells required in each menu page.</summary>
    public const int CellCount = PageColumns * PageRows;
    /// <summary>Size of each menu tilemap page in tile cells.</summary>
    public static Extent16<CharacterCells> PageSize => new(PageColumns, PageRows);
    /// <summary>Width in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterColumns = 32;
    /// <summary>Height in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;
    /// <summary>The selected slot's helmet animation frames.</summary>
    public const int HelmetFrameCount = 8;
    /// <summary>The JSON name of the common background tilemap, separate from the foreground menu pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground pages, in document order.</summary>
    public static IReadOnlyList<FileSelectPage> Pages { get; } = Enum.GetValues<FileSelectPage>();
    /// <summary>The slot-field patches, in document order.</summary>
    public static IReadOnlyList<FileSelectPatch> Patches { get; } = Enum.GetValues<FileSelectPatch>();
    /// <summary>The bordered headings, in document order.</summary>
    public static IReadOnlyList<FileSelectBorder> Borders { get; } = Enum.GetValues<FileSelectBorder>();
    /// <summary>The copy and clear pages' slot-letter anchors, in document order.</summary>
    public static IReadOnlyList<FileSelectAnchor> Anchors { get; } = Enum.GetValues<FileSelectAnchor>();

    /// <summary>The JSON name of a bordered heading, which keys its anchor.</summary>
    private static readonly IReadOnlyDictionary<FileSelectBorder, string> BorderNames =
        new Dictionary<FileSelectBorder, string>
        {
            [FileSelectBorder.Main] = "Main",
            [FileSelectBorder.Copy] = "Copy",
            [FileSelectBorder.Clear] = "Clear",
        };

    /// <summary>The JSON name of a copy or clear page's slot-letter anchor.</summary>
    private static readonly IReadOnlyDictionary<FileSelectAnchor, string> AnchorNames =
        new Dictionary<FileSelectAnchor, string>
        {
            [FileSelectAnchor.CopyDestinationSource] = "Copy.Destination.Source",
            [FileSelectAnchor.CopyConfirmSource] = "Copy.Confirm.Source",
            [FileSelectAnchor.CopyConfirmDestination] = "Copy.Confirm.Destination",
            [FileSelectAnchor.ClearConfirmSource] = "Clear.Confirm.Source",
        };

    /// <summary>The JSON name of a bordered heading, which keys its anchor.</summary>
    public static string BorderName(FileSelectBorder border) => Ensure.Listed(BorderNames, border);

    /// <summary>The JSON name of a copy or clear page's slot-letter anchor.</summary>
    public static string AnchorName(FileSelectAnchor anchor) => Ensure.Listed(AnchorNames, anchor);

    /// <summary>Every sprite the document names: the three borders, the four cursor frames and the eight helmet frames.</summary>
    public static IEnumerable<string> SpriteNames =>
        [.. Borders.Select(FileSelectParts.BorderSpriteName), .. FileSelectPresentationDefinitions.CursorSpriteNames,
            .. FileSelectPresentationDefinitions.HelmetSpriteNames];

    /// <summary>Every frame of the file-select sheet: each sprite the document names, then each file-copy arrow.</summary>
    public static IEnumerable<string> SheetFrames =>
        SpriteNames.Concat(FileCopyArrowDefinitions.Shapes.Select(FileCopyArrowDefinitions.FrameName));
}
