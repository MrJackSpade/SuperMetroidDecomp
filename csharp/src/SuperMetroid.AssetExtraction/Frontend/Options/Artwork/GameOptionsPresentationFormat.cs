using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>File identity, schema names and page geometry of the installed <c>options-menu.json</c>.</summary>
public static class GameOptionsPresentationFormat
{
    /// <summary>Supported options-menu presentation JSON schema version: 2 holds no hidden cursor anchor (#1775).</summary>
    public const int Version = 2;
    /// <summary>Extracted JSON filename for options artwork, highlight regions, sprite compositions, and anchors.</summary>
    public const string FileName = "options-menu.json";
    /// <summary>Import-data filename of the extracted options document: its pages, labels, compositions and layout (#1638).</summary>
    public const string ArtworkFileName = "options-menu-artwork.json";
    /// <summary>The JSON name of the common background page, separate from the foreground option pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground option pages in document order (#2567): primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsScreen> Tilemaps { get; } = GameOptionsScreen.All;
    /// <summary>The two special settings in document order (#2567), icon cancel then moonwalk.</summary>
    public static IReadOnlyList<GameOptionsToggle> Toggles { get; } = Ensure.EveryMemberOnce<GameOptionsToggle>(
    [
        GameOptionsToggle.IconCancel,
        GameOptionsToggle.Moonwalk,
    ]);
    /// <summary>The three menu layouts headings and cursors are anchored by, in primary, controller and special order (#2567).</summary>
    internal static IReadOnlyList<GameOptionsPage> MenuPages { get; } = Ensure.EveryMemberOnce<GameOptionsPage>(
    [
        GameOptionsPage.Primary,
        GameOptionsPage.Controller,
        GameOptionsPage.Special,
    ]);

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

    /// <summary>Every frame of the options sheet: each page's heading, then each of the missile's pictures.</summary>
    public static IEnumerable<string> SheetFrames =>
        MenuPages.Select(GameOptionsNames.HeadingSpriteName).Concat(MenuMissileFrames.Names);
}
