using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Runtime;

/// <summary>Binds one installation's runtime presentation catalogs to bare <see cref="SuperMetroidRuntime"/> fixtures.</summary>
internal static class InstalledRuntimeBindings
{
    /// <summary>Loads every runtime catalog once; the returned action binds them to each new runtime.</summary>
    internal static Action<SuperMetroidRuntime> Create(GameInstallation installation)
    {
        var assetMapPresentation = installation.LoadMaps();
        var assetStandardObjectArt = installation.LoadStandardObjects();
        var assetSamusBodyArt = installation.LoadSamusBodyArt();
        var assetRoomCharacterArt = installation.LoadRoomCharacters();
        var assetRoomPaletteArt = installation.LoadRoomPalettes();
        var assetRoomMetatileArt = installation.LoadRoomMetatiles();
        var assetRoomVisualLayouts = installation.LoadRoomVisualLayouts();
        var assetRoomPlmShotBlockVisuals = installation.LoadRoomPlmShotBlockVisuals();
        var assetRoomPlmGrappleBlockVisuals = installation.LoadRoomPlmGrappleBlockVisuals();
        var assetRoomPlmStationVisuals = installation.LoadRoomPlmStationVisuals();
        var assetRoomPlmBlueDoorVisuals = installation.LoadRoomPlmBlueDoorVisuals();
        var assetRoomPlmColoredDoorVisuals = installation.LoadRoomPlmColoredDoorVisuals();
        var assetRoomPlmGreyDoorVisuals = installation.LoadRoomPlmGreyDoorVisuals();
        var assetRoomPlmEyeDoorVisuals = installation.LoadRoomPlmEyeDoorVisuals();
        var assetRoomPlmMotherBrainGlassVisuals = installation.LoadRoomPlmMotherBrainGlassVisuals();
        var assetRoomPlmNoobTubeVisuals = installation.LoadRoomPlmNoobTubeVisuals();
        var assetRoomPlmDownwardGateVisuals = installation.LoadRoomPlmDownwardGateVisuals();
        var assetRoomPlmElevatorPlatformVisuals = installation.LoadRoomPlmElevatorPlatformVisuals();
        var assetRoomPlmEscapeGateVisuals = installation.LoadRoomPlmEscapeGateVisuals();
        var assetRoomPlmBombTorizoHandVisuals = installation.LoadRoomPlmBombTorizoHandVisuals();
        var assetRoomPlmDraygonCannonVisuals = installation.LoadRoomPlmDraygonCannonVisuals();
        var assetRoomPlmChozoStatueVisuals = installation.LoadRoomPlmChozoStatueVisuals();
        var assetRoomPlmLinkedRestoreVisuals = installation.LoadRoomPlmLinkedRestoreVisuals();
        var assetRoomPlmTourianAccessVisuals = installation.LoadRoomPlmTourianAccessVisuals();
        var assetRoomPlmSpeedBoosterVisuals = installation.LoadRoomPlmSpeedBoosterVisuals();
        var assetRoomPlmMaridiaElevatubeVisuals = installation.LoadRoomPlmMaridiaElevatubeVisuals();
        var assetRoomPlmSporeSpawnCeilingVisuals = installation.LoadRoomPlmSporeSpawnCeilingVisuals();
        var assetRoomPlmSamusEaterVisuals = installation.LoadRoomPlmSamusEaterVisuals();
        var assetRoomPlmBotwoonWallVisuals = installation.LoadRoomPlmBotwoonWallVisuals();
        var assetRoomPlmKraidVisuals = installation.LoadRoomPlmKraidVisuals();
        var assetRoomPlmCrocomireVisuals = installation.LoadRoomPlmCrocomireVisuals();
        var assetRoomPlmMotherBrainFakeDeathVisuals = installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
        var assetRoomPlmCollectibleVisuals = installation.LoadRoomPlmCollectibleVisuals();
        var assetRoomPlmDynamicCollectibleArt = installation.LoadRoomPlmDynamicCollectibleArt();
        var assetXrayRevealVisuals = installation.LoadXrayRevealVisuals();
        var assetRoomBackgroundTilemapArt = installation.LoadRoomBackgroundTilemaps();
        var assetRoomSkyTilemapArt = installation.LoadRoomSkyTilemaps();
        var assetEnemyTileArtwork = installation.LoadEnemyTiles();
        var projectiles = installation.LoadProjectiles();
        return runtime =>
        {
            runtime.MapPresentation = assetMapPresentation;
            runtime.StandardObjectArt = assetStandardObjectArt;
            runtime.SamusBodyArt = assetSamusBodyArt;
            runtime.RoomCharacterArt = assetRoomCharacterArt;
            runtime.RoomPaletteArt = assetRoomPaletteArt;
            runtime.RoomMetatileArt = assetRoomMetatileArt;
            runtime.RoomVisualLayouts = assetRoomVisualLayouts;
            runtime.RoomPlmShotBlockVisuals = assetRoomPlmShotBlockVisuals;
            runtime.RoomPlmGrappleBlockVisuals = assetRoomPlmGrappleBlockVisuals;
            runtime.RoomPlmStationVisuals = assetRoomPlmStationVisuals;
            runtime.RoomPlmBlueDoorVisuals = assetRoomPlmBlueDoorVisuals;
            runtime.RoomPlmColoredDoorVisuals = assetRoomPlmColoredDoorVisuals;
            runtime.RoomPlmGreyDoorVisuals = assetRoomPlmGreyDoorVisuals;
            runtime.RoomPlmEyeDoorVisuals = assetRoomPlmEyeDoorVisuals;
            runtime.RoomPlmMotherBrainGlassVisuals = assetRoomPlmMotherBrainGlassVisuals;
            runtime.RoomPlmNoobTubeVisuals = assetRoomPlmNoobTubeVisuals;
            runtime.RoomPlmDownwardGateVisuals = assetRoomPlmDownwardGateVisuals;
            runtime.RoomPlmElevatorPlatformVisuals = assetRoomPlmElevatorPlatformVisuals;
            runtime.RoomPlmEscapeGateVisuals = assetRoomPlmEscapeGateVisuals;
            runtime.RoomPlmBombTorizoHandVisuals = assetRoomPlmBombTorizoHandVisuals;
            runtime.RoomPlmDraygonCannonVisuals = assetRoomPlmDraygonCannonVisuals;
            runtime.RoomPlmChozoStatueVisuals = assetRoomPlmChozoStatueVisuals;
            runtime.RoomPlmLinkedRestoreVisuals = assetRoomPlmLinkedRestoreVisuals;
            runtime.RoomPlmTourianAccessVisuals = assetRoomPlmTourianAccessVisuals;
            runtime.RoomPlmSpeedBoosterVisuals = assetRoomPlmSpeedBoosterVisuals;
            runtime.RoomPlmMaridiaElevatubeVisuals = assetRoomPlmMaridiaElevatubeVisuals;
            runtime.RoomPlmSporeSpawnCeilingVisuals = assetRoomPlmSporeSpawnCeilingVisuals;
            runtime.RoomPlmSamusEaterVisuals = assetRoomPlmSamusEaterVisuals;
            runtime.RoomPlmBotwoonWallVisuals = assetRoomPlmBotwoonWallVisuals;
            runtime.RoomPlmKraidVisuals = assetRoomPlmKraidVisuals;
            runtime.RoomPlmCrocomireVisuals = assetRoomPlmCrocomireVisuals;
            runtime.RoomPlmMotherBrainFakeDeathVisuals = assetRoomPlmMotherBrainFakeDeathVisuals;
            runtime.RoomPlmCollectibleVisuals = assetRoomPlmCollectibleVisuals;
            runtime.RoomPlmDynamicCollectibleArt = assetRoomPlmDynamicCollectibleArt;
            runtime.XrayRevealVisuals = assetXrayRevealVisuals;
            runtime.RoomBackgroundTilemapArt = assetRoomBackgroundTilemapArt;
            runtime.RoomSkyTilemapArt = assetRoomSkyTilemapArt;
            runtime.Enemies.TileArtwork = assetEnemyTileArtwork;
            runtime.ProjectileCompositions = projectiles.Catalog;
            runtime.ProjectileFrameBindings = projectiles.FrameBindings;
            runtime.BeamArtwork = projectiles.BeamTiles;
            runtime.TrailArtwork = projectiles.Trails;
            runtime.ChargeFlarePlacement = projectiles.FlarePlacement;
            runtime.ChargeFlareCompositions = projectiles.FlareCompositions;
            runtime.GrappleArtwork = projectiles.GrappleTiles;
        };
    }
}
