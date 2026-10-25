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
    /// <remarks>
    /// Gameplay reads the frontend's atomic map and presentation install through narrow interfaces whose only
    /// implementation is the presentation catalog, so the catalog's map-atomic proof carries over to them.
    /// </remarks>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource", "palette-interface-total-membership-query", ["TryReadColor"],
            [new("csharp/src/SuperMetroid.Core/Fx/Artwork/IPaletteFxColorSource.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomPaletteFxPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/LoadingPaletteColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/HeatPaletteColorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/PaletteFx/PaletteFxHeatProgramDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/PaletteFx/PaletteFxColorRows.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/Artwork/HeatPaletteInputView.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Title/Artwork/TitlePalettePresentation.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation", "cinematic-interface-complete-owned-frame-domains", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IIntroCinematicSpritePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/CinematicSpriteSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroDiscoveryActorSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroEggEffectSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroRinkaSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroScientistSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/CeresCinematics/Artwork/CeresSceneSpriteSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingCloudSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingCompletionTextSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingExplosionSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingLogoSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingRewardSpriteDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/EndingAnimalEscapeDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroCinematicSpriteCompiler.cs")]),
        new("SuperMetroid.Core.Gameplay.IGameplayPresentationContent", "gameplay-content-single-catalog", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Gameplay/IGameplayPresentationContent.cs"),
             new("csharp/src/SuperMetroid.Core/Map/IAreaMapSource.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/AreaMapPresentationCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidHudRestoration.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),        new("SuperMetroid.Core.Map.IAreaMapSource", "area-map-source-single-catalog", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Map/IAreaMapSource.cs"),
             new("csharp/src/SuperMetroid.Core/Gameplay/IGameplayPresentationContent.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/AreaMapPresentationCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidHudRestoration.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs")]),    ];

    /// <summary>Locates a reviewed production asset source file by its catalog type name.</summary>

    /// <summary>Lists the source-owned implementations for audited palette or cinematic dispatch, returning null for contracts outside this closed set.</summary>
    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource" => [ContractTypeNames.MetadataName(typeof(RoomPaletteFxPresentation)), ContractTypeNames.MetadataName(typeof(TitlePalettePresentation))],
        "SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation" =>
            [ContractTypeNames.MetadataName(typeof(CinematicSpriteSheet)),
             ContractTypeNames.MetadataName(typeof(CeresSceneSpriteSheet))],
        "SuperMetroid.Core.Gameplay.IGameplayPresentationContent" => [ContractTypeNames.MetadataName(typeof(AreaMapPresentationCatalog))],
        "SuperMetroid.Core.Map.IAreaMapSource" => [ContractTypeNames.MetadataName(typeof(AreaMapPresentationCatalog)),
            ContractTypeNames.MetadataName(typeof(IGameplayPresentationContent))],
        _ => null,
    };
}
