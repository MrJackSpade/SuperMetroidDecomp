using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.PlmPrograms;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    /// <summary>Reviewed descriptor, importer, artwork-admission, and runtime-routing sources that bound native DMA ownership.</summary>
    /// <remarks>The installation source is reviewed because installed format 85 requires both bounded invalid-selection beam sheets.</remarks>
    /// <remarks>A changed source is an explicit contract gap until its new behavior is reviewed.</remarks>
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.Core/Samus/Abilities/SamusSpecialSequenceRomData.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/GrappleTileExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/BeamTileExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectileTrailAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerTileAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/SamusDeathTileArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/SamusArmCannonArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleBeamTilePatterns.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs"),
        new("csharp/src/SuperMetroid.Core/World/RuntimeArtwork.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/ProjectileTrailAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/ProjectileTrailAtlasDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamTileCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Progression/SamusEquipmentFlags.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/BeamTileAtlasDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SpazerCompositionGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusArmCannonPlacementDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyDefinitionLayout.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyFrameDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPoseDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusBodyPixelDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleSwingFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Death/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Movement/SamusMovementRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusSpritemapPoseDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Poses/SamusPoseId.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Artwork/SamusArmCannonArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusComboMechanicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Projectiles/SamusComboRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxAnimatedTileAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/Artwork/RoomFxAnimatedTileArtworkDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/Artwork/TourianStatueTileStripDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomSkyTilemapCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Areas/Ceres/CeresEscapeVramTransferDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Enemies/Corpses/DeadMonsterRottingDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Enemies/Corpses/DeadTorizoVramTransferDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Enemies/Corpses/DeadTourianBehavior.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/RoomFxAnimatedTileMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Areas/Tourian/TourianStatueAnimatedTileMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/WreckedShipTreadmillMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/Scrolling/ScrollingSkyChunkDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Samus/Movement/ScrollBoundaryCamera.cs"),
        new("csharp/src/SuperMetroid.Core/Fx/RoomFxRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateSelectionDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/Scrolling/RoomScrollDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/ProjectilePresentationFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Frontend/Map/MapPresentationExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Fx/RoomFxAnimatedTileAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Samus/SamusBodyArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomSkyTilemapArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Installation/GameAssetInstaller.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/Installation/GameInstallation.cs"),
    ];
    /// <summary>Reports a missing reviewed provider source from the PLM, area-map, or additional DMA contract sets.</summary>
    internal static void Verify(string root, VramDmaReport report)
    {
        foreach (ReviewedSource source in PlmVramArtworkSourceContract.Sources
                     .Concat(AreaMapClosedContractDefinitions.All[0].Sources).Concat(Additional).DistinctBy(source => source.Path))
            if (!File.Exists(Path.Combine(root, source.Path)))
                report.Findings.Add(new("unresolved-provider", source.Path, source.Path,
                    "Installed-artwork contract source is missing."));
    }
}
