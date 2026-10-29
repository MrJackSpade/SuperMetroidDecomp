using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>The menu heading each options menu layout's heading composition is recognised from.</summary>
internal static class GameOptionsSpriteDefinitions
{
    /// <summary>The menu heading a menu layout draws.</summary>
    internal static readonly IReadOnlyDictionary<GameOptionsPage, MenuHeading> Headings =
        new Dictionary<GameOptionsPage, MenuHeading>
        {
            [GameOptionsPage.Primary] = MenuHeading.OptionsPrimary,
            [GameOptionsPage.Controller] = MenuHeading.OptionsController,
            [GameOptionsPage.Special] = MenuHeading.OptionsSpecial,
        };
}
