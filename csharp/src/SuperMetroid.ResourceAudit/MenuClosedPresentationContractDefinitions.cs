namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete menu providers; named arguments still need independent key coverage.</summary>
internal static class MenuClosedPresentationContractDefinitions
{
    /// <summary>Closed reviewed sources for options, game-over, and reserve menu presentation domains.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameOptionsPresentation", "options-v1-complete-menu-domains",
            ["CreatePage", "LoadBackground", "ApplyLanguage", "ApplyControllerLabel", "ApplySpecialToggle",
                "CursorPosition", "DrawHeading", "DrawCursor"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs")]),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs")]),
    ];
}
