using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Frontend.Options.Artwork.GameOptionsPresentationDefinitions;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Names the options document's tilemaps, menus, headings, controller labels and toggles, and counts each menu page's
/// cursor rows (<see cref="GameOptionsPresentationDefinitions"/>).
/// </summary>
public static class GameOptionsNames
{
    /// <summary>The JSON name of a foreground page.</summary>
    public static string TilemapName(GameOptionsScreen tilemap) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.TilemapNames, tilemap);

    /// <summary>The JSON name of a menu layout, which keys its heading and cursor anchors.</summary>
    internal static string MenuName(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.MenuNames, page);

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static string HeadingSpriteName(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.HeadingSpriteNames, page);


    /// <summary>The JSON name of an assignable button's glyph.</summary>
    public static string ControllerLabelName(ControllerButton button) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.ControllerLabelNames, button);

    /// <summary>The JSON name of a special setting's visual.</summary>
    public static string ToggleName(GameOptionsToggle toggle) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.ToggleNames, toggle);

    /// <summary>Returns the number of selection-row anchors required by a menu layout.</summary>
    /// <returns>Five for primary, nine for controller including Exit and Reset, or three for special including Exit.</returns>
    internal static int CursorCount(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.CursorCounts, page);

    /// <summary>The JSON name of a looping missile-cursor frame's sprite, for frames 0-3.</summary>
    public static string CursorSpriteName(int frame) =>
        Ensure.Element(CursorSpriteNames, frame);

    /// <summary>The assignable button a native selector names.</summary>
    /// <param name="index">Zero-based index 0-6 in X, A, B, Select, Y, L, R order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0-6.</exception>
    public static ControllerButton ControllerLabelButton(int index) =>
        Ensure.Element(ControllerLabelButtons, index);

    /// <summary>The menus' picture of the options screen's backdrop page (#1467).</summary>
    public const string BackgroundFrame = "Options.Background";

    /// <summary>The menus' picture of <paramref name="page"/>, less its labels, language highlights and toggle choices (#1467).</summary>
    public static string PageFrame(GameOptionsScreen page) => $"Options.Page.{TilemapName(page)}";

    /// <summary>The menus' picture of the primary page's language row <paramref name="region"/> while the text is Japanese or English (#1467).</summary>
    public static string LanguageFrame(int region, bool japanese) =>
        $"Options.Language.{Ensure.BetweenInclusive(0, GameOptionsDefinitions.LanguagePaletteRegionCount - 1, region)}.{(japanese ? "Japanese" : "English")}";

    /// <summary>The menus' picture of controller action <paramref name="action"/>'s label naming <paramref name="button"/> (#1467).</summary>
    public static string ControllerLabelFrame(int action, ControllerButton button) =>
        $"Options.Controller.{Ensure.BetweenInclusive(0, GameOptionsDefinitions.Rows.ControllerActionCount - 1, action)}.{ControllerLabelName(button)}";

    /// <summary>
    /// The menus' picture of <paramref name="toggle"/>'s two choices on <paramref name="page"/>, the enabled one or the
    /// disabled one selected (#1467).
    /// </summary>
    public static string ToggleFrame(GameOptionsScreen page, GameOptionsToggle toggle, bool enabled) =>
        $"Options.Toggle.{TilemapName(page)}.{ToggleName(toggle)}.{(enabled ? "On" : "Off")}";

    /// <summary>Every frame of the options sheet: each page's heading, then each cursor frame.</summary>
    public static IEnumerable<string> SheetFrames =>
        MenuPages.Select(HeadingSpriteName).Concat(Enumerable.Range(0, CursorFrameCount).Select(CursorSpriteName));
}
