using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Reviewed source-owned interface dispatch sets; external injected implementations are not certified.</summary>
internal static class InterfacePresentationContractDefinitions
{
    /// <summary>Reviewed palette-source and cinematic-sprite interface dispatches with their source-owned implementations and audit files.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource", "palette-interface-total-membership-query", ["TryReadColor"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/IPaletteFxColorSource.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomPaletteFxPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/HeatPaletteColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/PaletteFx/PaletteFxHeatInstructionListDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/HeatPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitlePalettePresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation", "cinematic-interface-complete-owned-frame-domains", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IIntroCinematicSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroDiscoveryActorSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroDiscoveryActorSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroEggEffectSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroEggEffectSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroRinkaSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroRinkaSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroScientistSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroScientistSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/CeresCinematics/Artwork/CeresFlightSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/CeresCinematics/Artwork/CeresDestructionSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/CeresCinematics/Artwork/CeresSceneSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingCloudSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingCompletionTextSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingExplosionSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingLogoSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingRewardSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/EndingAnimalEscapeDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroCinematicSpriteCompiler.cs")]),
        // Gameplay reads the frontend's atomic map and presentation install through these narrow interfaces; the
        // catalog is their only implementation, so its map-atomic proof carries over.
        new("SuperMetroid.Core.Gameplay.IGameplayPresentationContent", "gameplay-content-single-catalog", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Gameplay/IGameplayPresentationContent.cs"),
             new("csharp/src/SuperMetroid.Core/Map/IAreaMapSource.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/AreaMapPresentationCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidBackgroundRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),        new("SuperMetroid.Core.Map.IAreaMapSource", "area-map-source-single-catalog", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Map/IAreaMapSource.cs"),
             new("csharp/src/SuperMetroid.Core/Gameplay/IGameplayPresentationContent.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/AreaMapPresentationCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Areas/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidBackgroundRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),    ];

    /// <summary>Locates a reviewed production asset source file by its catalog type name.</summary>

    /// <summary>Lists the source-owned implementations for audited palette or cinematic dispatch, returning null for contracts outside this closed set.</summary>
    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource" => [(typeof(RoomPaletteFxPresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(RoomPaletteFxPresentation).FullName.")), (typeof(TitlePalettePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(TitlePalettePresentation).FullName."))],
        "SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation" =>
            [(typeof(IntroDiscoveryActorSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroDiscoveryActorSpritePresentation).FullName.")), (typeof(IntroEggEffectSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroEggEffectSpritePresentation).FullName.")),
             (typeof(IntroRinkaSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroRinkaSpritePresentation).FullName.")), (typeof(IntroScientistSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(IntroScientistSpritePresentation).FullName.")),
             (typeof(CeresFlightSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresFlightSpritePresentation).FullName.")), (typeof(CeresDestructionSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresDestructionSpritePresentation).FullName.")),
             (typeof(CeresSceneSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(CeresSceneSpritePresentation).FullName.")), (typeof(EndingCloudSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingCloudSpritePresentation).FullName.")),
             (typeof(EndingCompletionTextSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingCompletionTextSpritePresentation).FullName.")), (typeof(EndingExplosionSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingExplosionSpritePresentation).FullName.")),
             (typeof(EndingLogoSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingLogoSpritePresentation).FullName.")), (typeof(EndingRewardSpritePresentation).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingRewardSpritePresentation).FullName.")),
             (typeof(SuperMetroid.Core.Frontend.Ending.EndingAnimalEscapeDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(EndingAnimalEscapeDefinitions).FullName."))],
        "SuperMetroid.Core.Gameplay.IGameplayPresentationContent" => [(typeof(AreaMapPresentationCatalog).FullName ?? throw new InvalidOperationException("Verification requires typeof(AreaMapPresentationCatalog).FullName."))],
        "SuperMetroid.Core.Map.IAreaMapSource" => [(typeof(AreaMapPresentationCatalog).FullName ?? throw new InvalidOperationException("Verification requires typeof(AreaMapPresentationCatalog).FullName.")),
            (typeof(IGameplayPresentationContent).FullName ?? throw new InvalidOperationException("Verification requires typeof(IGameplayPresentationContent).FullName."))],
        _ => null,
    };
}
