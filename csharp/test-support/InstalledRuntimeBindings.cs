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
            runtime.Artwork.MapPresentation = assetMapPresentation;
            runtime.Artwork.StandardObjectArt = assetStandardObjectArt;
            runtime.Artwork.SamusBodyArt = assetSamusBodyArt;
            runtime.Artwork.RoomCharacterArt = assetRoomCharacterArt;
            runtime.Artwork.RoomPaletteArt = assetRoomPaletteArt;
            runtime.Artwork.RoomMetatileArt = assetRoomMetatileArt;
            runtime.Artwork.RoomVisualLayouts = assetRoomVisualLayouts;
            runtime.ShotBlockVisuals.RoomPlmShotBlockVisuals = assetRoomPlmShotBlockVisuals;
            runtime.ShotBlockVisuals.RoomPlmGrappleBlockVisuals = assetRoomPlmGrappleBlockVisuals;
            runtime.ShotBlockVisuals.RoomPlmStationVisuals = assetRoomPlmStationVisuals;
            runtime.ShotBlockVisuals.RoomPlmBlueDoorVisuals = assetRoomPlmBlueDoorVisuals;
            runtime.ShotBlockVisuals.RoomPlmColoredDoorVisuals = assetRoomPlmColoredDoorVisuals;
            runtime.ShotBlockVisuals.RoomPlmGreyDoorVisuals = assetRoomPlmGreyDoorVisuals;
            runtime.ShotBlockVisuals.RoomPlmEyeDoorVisuals = assetRoomPlmEyeDoorVisuals;
            runtime.ShotBlockVisuals.RoomPlmMotherBrainGlassVisuals = assetRoomPlmMotherBrainGlassVisuals;
            runtime.ShotBlockVisuals.RoomPlmNoobTubeVisuals = assetRoomPlmNoobTubeVisuals;
            runtime.ShotBlockVisuals.RoomPlmDownwardGateVisuals = assetRoomPlmDownwardGateVisuals;
            runtime.ShotBlockVisuals.RoomPlmElevatorPlatformVisuals = assetRoomPlmElevatorPlatformVisuals;
            runtime.ShotBlockVisuals.RoomPlmEscapeGateVisuals = assetRoomPlmEscapeGateVisuals;
            runtime.ShotBlockVisuals.RoomPlmBombTorizoHandVisuals = assetRoomPlmBombTorizoHandVisuals;
            runtime.ShotBlockVisuals.RoomPlmDraygonCannonVisuals = assetRoomPlmDraygonCannonVisuals;
            runtime.ShotBlockVisuals.RoomPlmChozoStatueVisuals = assetRoomPlmChozoStatueVisuals;
            runtime.ShotBlockVisuals.RoomPlmLinkedRestoreVisuals = assetRoomPlmLinkedRestoreVisuals;
            runtime.ShotBlockVisuals.RoomPlmTourianAccessVisuals = assetRoomPlmTourianAccessVisuals;
            runtime.ShotBlockVisuals.RoomPlmSpeedBoosterVisuals = assetRoomPlmSpeedBoosterVisuals;
            runtime.ShotBlockVisuals.RoomPlmMaridiaElevatubeVisuals = assetRoomPlmMaridiaElevatubeVisuals;
            runtime.ShotBlockVisuals.RoomPlmSporeSpawnCeilingVisuals = assetRoomPlmSporeSpawnCeilingVisuals;
            runtime.ShotBlockVisuals.RoomPlmSamusEaterVisuals = assetRoomPlmSamusEaterVisuals;
            runtime.ShotBlockVisuals.RoomPlmBotwoonWallVisuals = assetRoomPlmBotwoonWallVisuals;
            runtime.ShotBlockVisuals.RoomPlmKraidVisuals = assetRoomPlmKraidVisuals;
            runtime.ShotBlockVisuals.RoomPlmCrocomireVisuals = assetRoomPlmCrocomireVisuals;
            runtime.ShotBlockVisuals.RoomPlmMotherBrainFakeDeathVisuals = assetRoomPlmMotherBrainFakeDeathVisuals;
            runtime.ShotBlockVisuals.RoomPlmCollectibleVisuals = assetRoomPlmCollectibleVisuals;
            runtime.ShotBlockVisuals.RoomPlmDynamicCollectibleArt = assetRoomPlmDynamicCollectibleArt;
            runtime.Xray.XrayRevealVisuals = assetXrayRevealVisuals;
            runtime.Artwork.RoomBackgroundTilemapArt = assetRoomBackgroundTilemapArt;
            runtime.Artwork.RoomSkyTilemapArt = assetRoomSkyTilemapArt;
            runtime.Enemies.TileArtwork = assetEnemyTileArtwork;
            runtime.Artwork.ProjectileCompositions = projectiles.Catalog;
            runtime.Artwork.ProjectileFrameBindings = projectiles.FrameBindings;
            runtime.Artwork.BeamArtwork = projectiles.BeamTiles;
            runtime.Artwork.TrailArtwork = projectiles.Trails;
            runtime.Artwork.ChargeFlarePlacement = projectiles.FlarePlacement;
            runtime.Artwork.ChargeFlareCompositions = projectiles.FlareCompositions;
            runtime.Artwork.GrappleArtwork = projectiles.GrappleTiles;
        };
    }
}
