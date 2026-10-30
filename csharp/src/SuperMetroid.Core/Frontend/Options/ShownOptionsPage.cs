using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// What the options screen's page shows (#1467): the page last loaded, and the language, bindings and special settings its
/// language rows, controller labels and toggle choices were drawn from when it was loaded. The menu loads the page again
/// whenever any of them changes, as the cartridge rewrites the page's labels and highlights.
/// </summary>
/// <param name="Page">The page shown.</param>
/// <param name="JapaneseText">Whether the primary page highlights the Japanese text option.</param>
/// <param name="Bindings">The bindings the controller page's labels name.</param>
/// <param name="IconCancelEnabled">Whether the special page selects Icon Cancel's enabled choice.</param>
/// <param name="MoonwalkEnabled">Whether the special page selects Moonwalk's enabled choice.</param>
[IndependentFlag("Japanese text, Icon Cancel and Moonwalk are three independent saved options the page shows.")]
internal sealed record ShownOptionsPage(GameOptionsTilemap Page, bool JapaneseText, ControllerBindings Bindings,
    bool IconCancelEnabled, bool MoonwalkEnabled)
{
    /// <summary>The menus' pictures the page shows, in drawing order: the page, then what is drawn over it.</summary>
    internal string[] Frames() => Page switch
    {
        GameOptionsTilemap.Primary => [GameOptionsNames.PageFrame(Page),
            .. Enumerable.Range(0, GameOptionsDefinitions.LanguagePaletteRegionCount).Select(region => GameOptionsNames.LanguageFrame(region, JapaneseText))],
        GameOptionsTilemap.ControllerEnglish or GameOptionsTilemap.ControllerJapanese => [GameOptionsNames.PageFrame(Page),
            .. Enumerable.Range(0, GameOptionsDefinitions.Rows.ControllerActionCount).Select(action => GameOptionsNames.ControllerLabelFrame(action,
                GameOptionsNames.ControllerLabelButton(ControllerBindings.AssignableButtonIndex(Bindings[action]) ?? 0)))],
        GameOptionsTilemap.SpecialEnglish or GameOptionsTilemap.SpecialJapanese => [GameOptionsNames.PageFrame(Page),
            GameOptionsNames.ToggleFrame(Page, GameOptionsToggle.IconCancel, IconCancelEnabled),
            GameOptionsNames.ToggleFrame(Page, GameOptionsToggle.Moonwalk, MoonwalkEnabled)],
        _ => throw new InvalidOperationException($"Undefined options page {Page}."),
    };
}
