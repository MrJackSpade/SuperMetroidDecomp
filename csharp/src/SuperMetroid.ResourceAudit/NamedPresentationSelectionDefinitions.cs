using Microsoft.CodeAnalysis;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Finite named sets already enforced by the reviewed production loaders.</summary>
internal static class NamedPresentationSelectionDefinitions
{
    /// <summary>Reviewed finite name sets for file-select, options, reserve, and map-screen selectors.</summary>
    internal static readonly NamedPresentationSelection[] All =
    [
        new(nameof(FileSelectPresentation), "DynamicAnchor", "name", "file-select-dynamic-anchor",
            FileSelectPresentationDefinitions.DynamicAnchorNames),
        new(nameof(FileSelectPresentation), "ApplyPatch", "name", "file-select-patch",
            FileSelectPresentationDefinitions.PatchNames),
        new(nameof(FileSelectPresentation), "CopyPage", "name", "file-select-page",
            FileSelectPresentationDefinitions.PageNames),
        new(nameof(FileSelectPresentation), "DrawBorder", "page", "file-select-border",
            FileSelectPresentationDefinitions.BorderNames),
        new(nameof(GameOptionsPresentation), "CreatePage", "name", "options-page",
            GameOptionsPresentationDefinitions.PageNames.ToArray()),
        new(nameof(GameOptionsPresentation), "ApplySpecialToggle", "name", "options-toggle",
            GameOptionsPresentationDefinitions.SpecialToggleNames.ToArray()),
        new(nameof(GameOptionsPresentation), "CursorPosition", "page", "options-menu",
            GameOptionsPresentationDefinitions.MenuPageNames.ToArray()),
        new(nameof(GameOptionsPresentation), "DrawHeading", "page", "options-menu",
            GameOptionsPresentationDefinitions.MenuPageNames.ToArray()),
        new(nameof(PauseReserveUiPresentation), "ApplyLabel", "name", "reserve-label",
            ["Mode", "ReserveTank", "Manual", "Auto"]),
        new(nameof(MapScreenPresentation), "LoadTo", "page", "map-screen-page",
            MapScreenDefinitions.Pages().Select(page => page.Id).ToArray()),
    ];
}
