namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Source-reviewed title, opening and Ceres visual providers; no scene, timing or pixel-parity claim.</summary>
internal static class CinematicClosedContractDefinitions
{
    /// <summary>Lists reviewed title, opening, and Ceres providers with the source paths that bound each visual claim.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Frontend.Title.Artwork.TitleGradientPresentation", "title-complete-masked-gradient-variants", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleGradientPresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Title.Artwork.TitleSpriteDrawing", "title-complete-compiled-sprite-identities", ["DrawSprite"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleSpriteDrawing.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleCardDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleCardStep.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleSprite.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleCards.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleSequenceDefinitions.cs")]),
        new("SuperMetroid.AssetExtraction.Frontend.Intro.Artwork.IntroEyeTilemapPresentation", "intro-complete-eye-rectangles", ["FrameTiles"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Frontend/Intro/Artwork/IntroEyeTilemapPresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.CinematicSpriteSheet", "intro-sheet-complete-owned-poses", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/CinematicSpriteSheet.cs")]),
        new("SuperMetroid.AssetExtraction.Areas.Ceres.Artwork.CeresDoorVisuals", "ceres-door-complete-tile-and-color-images",
            ["LoadTiles", "NormalColor", "EscapeColor", "AnimationColor"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Areas/Ceres/Artwork/CeresDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Ceres/Artwork/CeresDoorVisualDefinitions.cs")]),
        new("SuperMetroid.Core.Escape.Artwork.EscapeTimerPresentation", "escape-timer-complete-decimal-and-label-art", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerPresentationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerComposition.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Artwork/SpriteComposition.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerCharacters.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerGlyphs.cs")]),
    ];
}
