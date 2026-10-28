namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Reviewed complete menu providers; named arguments still need independent key coverage.</summary>
internal static class MenuClosedPresentationContractDefinitions
{
    /// <summary>Closed reviewed sources for options, game-over, and reserve menu presentation domains.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Frontend.Options.Artwork.GameOptionsPresentation", "options-v1-complete-menu-domains",
            ["CreatePage", "LoadBackground", "ApplyLanguage", "ApplyControllerLabel", "ApplySpecialToggle",
                "CursorPosition", "DrawHeading", "DrawCursor"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Options/Artwork/GameOptionsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapSpriteCompositions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileSpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuCursorParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuHeadingBorder.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuHeadingBorderDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuBorderParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Options/GameOptionsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Options/GameOptionsLayout.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Options/GameOptionsPositions.cs")]),
        new("SuperMetroid.Core.Frontend.GameOver.Artwork.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Frontend/GameOver/Artwork/GameOverPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapSpriteCompositions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileAnimationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuMissileSpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/Artwork/MenuCursorParts.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Frontend/GameOver/Artwork/GameOverSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOver/Artwork/GameOverBabyPaint.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOver/GameOverMenuDefinitions.cs")]),
        new("SuperMetroid.Core.Frontend.Pause.Artwork.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Pause/Artwork/PauseReserveUiPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Pause/Artwork/PauseReserveUiDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Pause/Artwork/PauseReserveUiTiles.cs")]),
    ];
}
