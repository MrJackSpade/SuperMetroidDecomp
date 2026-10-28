using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Names the options document's tilemaps, menus, headings, controller labels and toggles, and counts each menu page's
/// cursor rows (<see cref="GameOptionsPresentationDefinitions"/>).
/// </summary>
public static class GameOptionsNames
{
    /// <summary>The JSON name of a foreground page.</summary>
    public static string TilemapName(GameOptionsTilemap tilemap) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.TilemapNames, tilemap);

    /// <summary>The JSON name of a menu layout, which keys its heading and cursor anchors.</summary>
    internal static string MenuName(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.MenuNames, page);

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static string HeadingSpriteName(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.HeadingSpriteNames, page);

    /// <summary>The menu heading a menu layout draws.</summary>
    internal static MenuHeading Heading(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.Headings, page);

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
}
