namespace SuperMetroid.ResourceAudit;

/// <summary>Source-reviewed title, opening and Ceres visual providers; no scene, timing or pixel-parity claim.</summary>
internal static class CinematicClosedContractDefinitions
{
    /// <summary>Lists reviewed title, opening, and Ceres providers with the source paths that bound each visual claim.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Frontend.Title.Artwork.TitlePalettePresentation", "title-complete-initial-palette", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitlePalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/PaletteFx/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleAmbientColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleSequenceRomData.cs")]),
        new("SuperMetroid.Core.Frontend.Title.Artwork.TitleGradientPresentation", "title-complete-masked-gradient-variants", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleGradientPresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Title.Artwork.TitleGraphicsPresentation", "title-complete-compiled-sprite-identities", ["DrawSprite"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleGraphicsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitleSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleSequenceInstructionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/TitleSequenceRomData.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IntroEyeTilemapPresentation", "intro-complete-eye-rectangles", ["FrameWords"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroEyeTilemapPresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IntroCaretSpritePresentation", "intro-complete-visible-caret", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroCaretSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroCaretSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IntroMotherBrainSpritePresentation", "intro-complete-mother-brain-sprites", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroMotherBrainSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroMotherBrainSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IntroMotherBrainExplosionSpritePresentation", "intro-complete-mother-brain-explosions", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroMotherBrainExplosionSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroMotherBrainExplosionSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Areas.Ceres.Artwork.CeresDoorVisualCatalog", "ceres-door-complete-tile-and-color-images",
            ["LoadTiles", "LoadNormalColors", "LoadEscapeColors", "LoadAnimationColors", "LoadMode7DoorFrame"],
            [new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorAnimationPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorEscapePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorEscapeSurfacePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorNormalPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresDoorWarmTargetPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/CeresDoorVisualRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomCharacterAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/CeresMode7TransferDefinitions.cs")]),
        new("SuperMetroid.Core.Areas.Escape.Artwork.EscapeTimerPresentation", "escape-timer-complete-decimal-and-label-art", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerPresentationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SpriteComposition.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerGlyphDefinitions.cs")]),
        new("SuperMetroid.Core.Areas.Ceres.Artwork.CeresEscapeOverlayTilemapCatalog", "ceres-overlay-complete-owned-membership-query", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Areas/Ceres/Artwork/CeresEscapeOverlayTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Ceres/CeresEscapeVramTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Ridley/CeresRidleyPaletteRomData.cs")]),
    ];
}
