namespace SuperMetroid.ResourceAudit;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    // These reviewed sources connect descriptor geometry to required PNG/JSON
    // admission, importer manifests and the runtime's native/typed dispatch.
    // A changed contract is an explicit gap until its new behavior is reviewed.
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Validation.cs"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Components.cs"),
        // Format 85 requires both bounded invalid-selection beam sheets.
        new("csharp/src/SuperMetroid.AssetExtraction/GameInstallation.cs"),
    ];
    internal static void Verify(string root, VramDmaReport report)
    {
        foreach (ReviewedSource source in PlmVramArtworkSourceContract.Sources
                     .Concat(AreaMapClosedContractDefinitions.All[0].Sources).Concat(Additional).DistinctBy(source => source.Path))
            if (!File.Exists(Path.Combine(root, source.Path)))
                report.Findings.Add(new("unresolved-provider", source.Path, source.Path,
                    "Installed-artwork contract source is missing."));
    }
}
