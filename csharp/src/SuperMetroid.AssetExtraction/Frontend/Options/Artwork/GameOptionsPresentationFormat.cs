using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>File identity, schema names and page geometry of the installed <c>options-menu.json</c>.</summary>
public static class GameOptionsPresentationFormat
{
    /// <summary>Supported options-menu presentation JSON schema version.</summary>
    public const int Version = 1;
    /// <summary>Extracted JSON filename for options artwork, highlight regions, sprite compositions, and anchors.</summary>
    public const string FileName = "options-menu.json";
    /// <summary>Import-data filename of the extracted options document: its pages, labels, compositions and layout (#1638).</summary>
    public const string ArtworkFileName = "options-menu-artwork.json";
    /// <summary>The JSON name of the common background page, separate from the foreground option pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground option pages in document order: primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsScreen> Tilemaps { get; } = Enum.GetValues<GameOptionsScreen>();
    /// <summary>The two special settings, icon cancel then moonwalk.</summary>
    public static IReadOnlyList<GameOptionsToggle> Toggles { get; } = Enum.GetValues<GameOptionsToggle>();
    /// <summary>The three menu layouts headings and cursors are anchored by, in primary, controller and special order.</summary>
    internal static IReadOnlyList<GameOptionsPage> MenuPages { get; } = Enum.GetValues<GameOptionsPage>();
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;

    /// <summary>The JSON name of each menu layout, which keys its heading and cursor anchors.</summary>
    private static readonly IReadOnlyDictionary<GameOptionsPage, string> MenuNames =
        new Dictionary<GameOptionsPage, string>
        {
            [GameOptionsPage.Primary] = "Primary",
            [GameOptionsPage.Controller] = "Controller",
            [GameOptionsPage.Special] = "Special",
        };

    /// <summary>The JSON name of a menu layout, which keys its heading and cursor anchors.</summary>
    internal static string MenuName(GameOptionsPage page) => Ensure.Listed(MenuNames, page);

    /// <summary>Every frame of the options sheet: each page's heading, then each cursor frame.</summary>
    public static IEnumerable<string> SheetFrames =>
        MenuPages.Select(GameOptionsNames.HeadingSpriteName)
            .Concat(Enumerable.Range(0, CursorFrameCount).Select(GameOptionsNames.CursorSpriteName));
}
