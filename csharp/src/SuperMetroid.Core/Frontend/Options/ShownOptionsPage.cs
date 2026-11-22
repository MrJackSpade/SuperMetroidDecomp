using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// What the options screen's page shows (#1467): the page last loaded, and the language, bindings and special settings its
/// language highlight, controller labels and toggle choices were drawn from when it was loaded. The menu loads the page again
/// whenever any of them changes, as the game rewrites the page's labels and highlights.
/// </summary>
/// <param name="Page">The page shown.</param>
/// <param name="Language">The chosen text language, which the primary page highlights and the settings pages are written in.</param>
/// <param name="Bindings">The bindings the controller page's labels name.</param>
/// <param name="IconCancelEnabled">Whether the special page selects Icon Cancel's enabled choice.</param>
/// <param name="MoonwalkEnabled">Whether the special page selects Moonwalk's enabled choice.</param>
[IndependentFlag("Icon Cancel and Moonwalk are two independent saved options the page shows.")]
internal sealed partial record ShownOptionsPage(GameOptionsPage Page, TextLanguage Language, ControllerBindings Bindings,
    bool IconCancelEnabled, bool MoonwalkEnabled)
{
    /// <summary>The screen the page shows in the chosen language (#2560).</summary>
    internal GameOptionsScreen Screen => GameOptionsScreen.Shown(Page, Language);

    /// <summary>The menus' pictures the page shows, in drawing order: the page, then what is drawn over it.</summary>
    internal string[] Frames() => Page switch
    {
        GameOptionsPage.Primary => [GameOptionsNames.PageFrame(Screen), GameOptionsNames.LanguageFrame(Language)],
        GameOptionsPage.Controller => [GameOptionsNames.PageFrame(Screen),
            .. GameOptionsPageRows.ControllerActions.Select(action => GameOptionsNames.ControllerLabelFrame(action, LabelledBinding(action)))],
        GameOptionsPage.Special => [GameOptionsNames.PageFrame(Screen),
            GameOptionsNames.ToggleFrame(Screen, GameOptionsToggle.IconCancel, IconCancelEnabled),
            GameOptionsNames.ToggleFrame(Screen, GameOptionsToggle.Moonwalk, MoonwalkEnabled)],
        _ => throw new InvalidOperationException($"Undefined options page {Page}."),
    };

    /// <summary>
    /// The button bound to <paramref name="action"/>, which its label on the controller page names: one of the assignable
    /// buttons, as the options menu's bindings are a unique permutation of them, checked as the menu opens
    /// (<see cref="ControllerBindings.RequireUniquePermutation"/>) and kept by its swaps.
    /// </summary>
    private ControllerButton LabelledBinding(ControllerAction action) => Bindings[action];
}
