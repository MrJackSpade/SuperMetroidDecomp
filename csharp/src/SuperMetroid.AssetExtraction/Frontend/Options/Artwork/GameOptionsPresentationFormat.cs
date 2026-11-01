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
    /// <summary>The JSON name of the common background page, separate from the foreground option pages.</summary>
    public const string BackgroundPageName = "Background";
    /// <summary>The foreground option pages in document order: primary, then English and Japanese controller and special pages.</summary>
    public static IReadOnlyList<GameOptionsScreen> Tilemaps { get; } = Enum.GetValues<GameOptionsScreen>();
    /// <summary>The two special settings, icon cancel then moonwalk.</summary>
    public static IReadOnlyList<GameOptionsToggle> Toggles { get; } = Enum.GetValues<GameOptionsToggle>();
    /// <summary>The missile cursor's looping animation frames.</summary>
    public const int CursorFrameCount = 4;

    /// <summary>Every frame of the options sheet: each page's heading, then each cursor frame.</summary>
    public static IEnumerable<string> SheetFrames =>
        GameOptionsPresentationDefinitions.MenuPages.Select(GameOptionsNames.HeadingSpriteName)
            .Concat(Enumerable.Range(0, CursorFrameCount).Select(GameOptionsNames.CursorSpriteName));
}
