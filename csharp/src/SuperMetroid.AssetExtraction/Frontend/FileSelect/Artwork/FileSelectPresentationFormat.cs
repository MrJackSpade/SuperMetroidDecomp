using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;
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
    public const int Version = 4;
    /// <summary>Installed JSON filename for the slot fields, anchors and durations the game binds.</summary>
    public const string FileName = "file-select.json";
    /// <summary>Supported file-select artwork JSON schema version.</summary>
    public const int ArtworkVersion = 1;
    /// <summary>Import-data JSON filename for the menu pages, field patches, glyphs and sprite compositions (#1638).</summary>
    public const string ArtworkFileName = "file-select-artwork.json";
    /// <summary>The width in pixels of each menu page the import pictures.</summary>
    public const int PageWidthPixels = 256;
    /// <summary>The height in pixels of each menu page the import pictures.</summary>
    public const int PageHeightPixels = 256;
    /// <summary>Cells across each menu page.</summary>
    public const int PageColumns = PageWidthPixels / CharacterCellConversions.CellPixels;
    /// <summary>Cells down each menu page.</summary>
    public const int PageRows = PageHeightPixels / CharacterCellConversions.CellPixels;
    /// <summary>Number of row-major tile cells required in each menu page.</summary>
    public const int CellCount = PageColumns * PageRows;
    /// <summary>Size of each menu tilemap page in tile cells.</summary>
    public static Extent16<CharacterCells> PageSize => new(PageColumns, PageRows);
    /// <summary>Width in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterColumns = 32;
    /// <summary>Height in characters of the sheet addressed by authored background tile cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>The JSON name of the common background tilemap, separate from the foreground menu pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground pages, in document order (#2567).</summary>
    public static IReadOnlyList<FileSelectPage> Pages { get; } = Ensure.EveryMemberOnce<FileSelectPage>(
    [
        FileSelectPage.MainWithData,
        FileSelectPage.MainEmpty,
        FileSelectPage.CopySource,
        FileSelectPage.CopyDestination,
        FileSelectPage.CopyConfirm,
        FileSelectPage.CopyCompleted,
        FileSelectPage.ClearSelection,
        FileSelectPage.ClearConfirm,
        FileSelectPage.ClearCompleted,
    ]);
    /// <summary>The slot-field patches, in document order (#2567).</summary>
    public static IReadOnlyList<FileSelectPatch> Patches { get; } = Ensure.EveryMemberOnce<FileSelectPatch>(
    [
        FileSelectPatch.Energy,
        FileSelectPatch.NoData,
        FileSelectPatch.TimeColon,
    ]);
    /// <summary>The bordered headings, in document order (#2567).</summary>
    public static IReadOnlyList<FileSelectBorder> Borders { get; } = Ensure.EveryMemberOnce<FileSelectBorder>(
    [
        FileSelectBorder.Main,
        FileSelectBorder.Copy,
        FileSelectBorder.Clear,
    ]);
    /// <summary>The copy and clear pages' slot-letter anchors, in document order (#2567).</summary>
    public static IReadOnlyList<FileSelectAnchor> Anchors { get; } = Ensure.EveryMemberOnce<FileSelectAnchor>(
    [
        FileSelectAnchor.CopyDestinationSource,
        FileSelectAnchor.CopyConfirmSource,
        FileSelectAnchor.CopyConfirmDestination,
        FileSelectAnchor.ClearConfirmSource,
    ]);

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

    /// <summary>Every sprite the document names: the three borders, the missile's pictures and the helmet's pictures.</summary>
    public static IEnumerable<string> SpriteNames =>
        [.. Borders.Select(FileSelectParts.BorderSpriteName), .. MenuMissileFrames.Names,
            .. FileSelectHelmetAnimation.Turn.Select(FileSelectParts.HelmetSpriteName)];

    /// <summary>Every frame of the file-select sheet: each sprite the document names, then each file-copy arrow.</summary>
    public static IEnumerable<string> SheetFrames =>
        SpriteNames.Concat(FileCopyArrowDefinitions.Shapes.Select(FileCopyArrowDefinitions.FrameName));
}
