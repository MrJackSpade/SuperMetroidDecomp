using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// What the options screen's page shows (#1467): the page last loaded, and the language, bindings and special settings its
/// language highlight, controller labels and toggle choices were drawn from when it was loaded. The menu loads the page again
/// whenever any of them changes, as the game rewrites the page's labels and highlights.
/// </summary>
/// <param name="Page">The page shown.</param>
/// <param name="JapaneseText">Whether the primary page highlights the Japanese text option.</param>
/// <param name="Bindings">The bindings the controller page's labels name.</param>
/// <param name="IconCancelEnabled">Whether the special page selects Icon Cancel's enabled choice.</param>
/// <param name="MoonwalkEnabled">Whether the special page selects Moonwalk's enabled choice.</param>
[IndependentFlag("Japanese text, Icon Cancel and Moonwalk are three independent saved options the page shows.")]
internal sealed record ShownOptionsPage(GameOptionsScreen Page, bool JapaneseText, ControllerBindings Bindings,
    bool IconCancelEnabled, bool MoonwalkEnabled)
{
    /// <summary>The menus' pictures each screen shows, in drawing order: the page, then what is drawn over it.</summary>
    private static IdentityTable<GameOptionsScreen, Func<ShownOptionsPage, string[]>> FramesShown => field ??= new(new Dictionary<GameOptionsScreen, Func<ShownOptionsPage, string[]>>
    {
        [GameOptionsScreen.Primary] = static shown => [GameOptionsNames.PageFrame(shown.Page), GameOptionsNames.LanguageFrame(shown.JapaneseText)],
        [GameOptionsScreen.ControllerEnglish] = static shown => [GameOptionsNames.PageFrame(shown.Page), .. GameOptionsPageRows.ControllerActions.Select(action => GameOptionsNames.ControllerLabelFrame(action, shown.LabelledBinding(action)))],
        [GameOptionsScreen.ControllerJapanese] = static shown => [GameOptionsNames.PageFrame(shown.Page), .. GameOptionsPageRows.ControllerActions.Select(action => GameOptionsNames.ControllerLabelFrame(action, shown.LabelledBinding(action)))],
        [GameOptionsScreen.SpecialEnglish] = static shown => [GameOptionsNames.PageFrame(shown.Page), GameOptionsNames.ToggleFrame(shown.Page, GameOptionsToggle.IconCancel, shown.IconCancelEnabled), GameOptionsNames.ToggleFrame(shown.Page, GameOptionsToggle.Moonwalk, shown.MoonwalkEnabled)],
        [GameOptionsScreen.SpecialJapanese] = static shown => [GameOptionsNames.PageFrame(shown.Page), GameOptionsNames.ToggleFrame(shown.Page, GameOptionsToggle.IconCancel, shown.IconCancelEnabled), GameOptionsNames.ToggleFrame(shown.Page, GameOptionsToggle.Moonwalk, shown.MoonwalkEnabled)],
    });

    /// <summary>The menus' pictures the page shows, in drawing order: the page, then what is drawn over it.</summary>
    internal string[] Frames() => FramesShown[Page](this);

    /// <summary>
    /// The button bound to <paramref name="action"/>, which its label on the controller page names: one of the assignable
    /// buttons, as the options menu's bindings are a unique permutation of them, checked as the menu opens
    /// (<see cref="ControllerBindings.RequireUniquePermutation"/>) and kept by its swaps.
    /// </summary>
    private ControllerButton LabelledBinding(ControllerAction action) => Bindings[action];
}
