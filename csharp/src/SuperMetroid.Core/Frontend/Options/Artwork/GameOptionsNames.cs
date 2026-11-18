using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Names the options document's pictures, headings, controller labels and toggles
/// (<see cref="GameOptionsPresentationDefinitions"/>).
/// </summary>
public static class GameOptionsNames
{
    /// <summary>The name the pictures of options page <paramref name="page"/> are filed under.</summary>
    public static string PageName(GameOptionsScreen page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.PageNames, page);

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static string HeadingSpriteName(GameOptionsPage page) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.HeadingSpriteNames, page);


    /// <summary>The JSON name of an assignable button's glyph.</summary>
    public static string ControllerLabelName(ControllerButton button) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.ControllerLabelNames, button);

    /// <summary>The JSON name of a special setting's visual.</summary>
    public static string ToggleName(GameOptionsToggle toggle) =>
        Ensure.Listed(GameOptionsPresentationDefinitions.ToggleNames, toggle);


    /// <summary>The menus' picture of the options screen's backdrop page (#1467).</summary>
    public const string BackgroundFrame = "Options.Background";

    /// <summary>The menus' picture of <paramref name="page"/>, less its labels, language highlights and toggle choices (#1467).</summary>
    public static string PageFrame(GameOptionsScreen page) => $"Options.Page.{PageName(page)}";

    /// <summary>
    /// The menus' picture of the primary page's English and Japanese labels with the Japanese one or the English one
    /// highlighted, which the import composes whole (#1467, #1780).
    /// </summary>
    public static string LanguageFrame(bool japanese) => $"Options.Language.{(japanese ? "Japanese" : "English")}";

    /// <summary>The menus' picture of controller action <paramref name="action"/>'s label naming <paramref name="button"/> (#1467, #1985).</summary>
    public static string ControllerLabelFrame(ControllerAction action, ControllerButton button) =>
        $"Options.Controller.{Ensure.Listed(GameOptionsPresentationDefinitions.ControllerActionNames, action)}.{ControllerLabelName(button)}";

    /// <summary>
    /// The menus' picture of <paramref name="toggle"/>'s two choices on <paramref name="page"/>, the enabled one or the
    /// disabled one selected (#1467).
    /// </summary>
    public static string ToggleFrame(GameOptionsScreen page, GameOptionsToggle toggle, bool enabled) =>
        $"Options.Toggle.{PageName(page)}.{ToggleName(toggle)}.{(enabled ? "On" : "Off")}";

}
