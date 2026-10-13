namespace SuperMetroid.ResourceAudit;

/// <summary>Source-reviewed title, opening and Ceres visual providers; no scene, timing or pixel-parity claim.</summary>
internal static class CinematicClosedContractDefinitions
{
    /// <summary>Lists reviewed title, opening, and Ceres providers with the source paths that bound each visual claim.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.TitlePalettePresentation", "title-complete-initial-palette", ["Apply"],
            [new("csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Game/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/TitleAmbientColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs")]),
        new("SuperMetroid.Core.Assets.TitleGradientPresentation", "title-complete-masked-gradient-variants", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/TitleGradientPresentation.cs")]),
        new("SuperMetroid.Core.Assets.TitleGraphicsPresentation", "title-complete-compiled-sprite-identities", ["DrawSprite"],
            [new("csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs")]),
        new("SuperMetroid.Core.Assets.IntroEyeTilemapPresentation", "intro-complete-eye-rectangles", ["FrameWords"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroEyeTilemapPresentation.cs")]),
        new("SuperMetroid.Core.Assets.IntroCaretSpritePresentation", "intro-complete-visible-caret", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroCaretSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.IntroMotherBrainSpritePresentation", "intro-complete-mother-brain-sprites", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroMotherBrainSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroMotherBrainSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.IntroMotherBrainExplosionSpritePresentation", "intro-complete-mother-brain-explosions", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroMotherBrainExplosionSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroMotherBrainExplosionSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.CeresDoorVisualCatalog", "ceres-door-complete-tile-and-color-images",
            ["LoadTiles", "LoadNormalColors", "LoadEscapeColors", "LoadAnimationColors", "LoadMode7DoorFrame"],
            [new("csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresDoorAnimationPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresDoorEscapePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresDoorEscapeSurfacePaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresDoorNormalPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CeresDoorWarmTargetPaintDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresDoorVisualRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.EscapeTimerPresentation", "escape-timer-complete-decimal-and-label-art", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SpriteComposition.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.CeresEscapeOverlayTilemapCatalog", "ceres-overlay-complete-owned-membership-query", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CeresRidleyPaletteRomData.cs")]),
    ];
}
