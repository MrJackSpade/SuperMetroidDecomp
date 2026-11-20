using SuperMetroid.Core.Input;
namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>The options menu's pages, controller labels and the names its pieces are pictured under.</summary>
public static class GameOptionsPresentationDefinitions
{
    /// <summary>The assignable buttons whose glyphs the controller page shows, in selector order: X, A, B, Select, Y, L, R.</summary>
    public static IReadOnlyList<ControllerButton> ControllerLabelButtons { get; } =
        [ControllerButton.X, ControllerButton.A, ControllerButton.B, ControllerButton.Select, ControllerButton.Y, ControllerButton.L, ControllerButton.R];

    /// <summary>The name each options page's pictures are filed under, before a settings page's language.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, string> PageNames =
        new Dictionary<GameOptionsPage, string>
        {
            [GameOptionsPage.Primary] = "Primary",
            [GameOptionsPage.Controller] = "Controller",
            [GameOptionsPage.Special] = "Special",
        };

    /// <summary>The name each text language's pictures are filed under (#2560).</summary>
    internal static readonly IReadOnlyDictionary<TextLanguage, string> LanguageNames =
        new Dictionary<TextLanguage, string>
        {
            [TextLanguage.English] = "English",
            [TextLanguage.Japanese] = "Japanese",
        };

    /// <summary>The JSON name of a menu layout's heading sprite.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, string> HeadingSpriteNames =
        new Dictionary<GameOptionsPage, string>
        {
            [GameOptionsPage.Primary] = "Heading.Primary",
            [GameOptionsPage.Controller] = "Heading.Controller",
            [GameOptionsPage.Special] = "Heading.Special",
        };


    /// <summary>The JSON name of an assignable button's glyph.</summary>
    internal static readonly IReadOnlyDictionary<ControllerButton, string> ControllerLabelNames =
        new Dictionary<ControllerButton, string>
        {
            [ControllerButton.X] = "X",
            [ControllerButton.A] = "A",
            [ControllerButton.B] = "B",
            [ControllerButton.Select] = "Select",
            [ControllerButton.Y] = "Y",
            [ControllerButton.L] = "L",
            [ControllerButton.R] = "R",
        };

    /// <summary>The name each controller action's label is pictured under.</summary>
    internal static readonly IReadOnlyDictionary<ControllerAction, string> ControllerActionNames =
        new Dictionary<ControllerAction, string>
        {
            [ControllerAction.Shoot] = "Shoot",
            [ControllerAction.Jump] = "Jump",
            [ControllerAction.Dash] = "Dash",
            [ControllerAction.ItemSelect] = "ItemSelect",
            [ControllerAction.ItemCancel] = "ItemCancel",
            [ControllerAction.AimUp] = "AimUp",
            [ControllerAction.AimDown] = "AimDown",
        };

    /// <summary>The JSON name of a special setting's visual.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsToggle, string> ToggleNames =
        new Dictionary<GameOptionsToggle, string>
        {
            [GameOptionsToggle.IconCancel] = "IconCancel",
            [GameOptionsToggle.Moonwalk] = "Moonwalk",
        };

}
