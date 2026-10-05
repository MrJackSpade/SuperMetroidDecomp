using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static readonly Lazy<GameInstallation> runtimeFixtureInstallation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });

    private static readonly Lazy<InstalledProjectilePresentation> projectileFixtureArt =
        new(() => runtimeFixtureInstallation.Value.LoadProjectiles());

    private static SamusProjectileSystem CreateProjectileFixture() =>
        new() { FrameBindings = projectileFixtureArt.Value.FrameBindings };

    private static SamusBombProjectileSystem CreateBombFixture() =>
        new() { FrameBindings = projectileFixtureArt.Value.FrameBindings, PowerBombExplosion = { PresentationColors = RetailPresentationFixture().PowerBombFixedColors } };
    private static readonly Lazy<Action<SuperMetroidRuntime>> runtimeFixtureBindings =
        new(CreateRuntimeFixtureBindings);

    // Tests using retail gameplay must supply the host's installed presentation
    // contract explicitly. Each fixture retains its own bus and mutable runtime.
    private static SuperMetroidRuntime CreateRetailRuntimeFixture(
        ISnesAddressSpace addressSpace,
        bool playerInvincibilityEnabled = false,
        bool infiniteAmmoEnabled = false,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        bool preventEscapeTimeout = false)
    {
        var runtime = new SuperMetroidRuntime(addressSpace, playerInvincibilityEnabled,
            infiniteAmmoEnabled, mapRevealMode, preventEscapeTimeout,
            runtimeFixtureInstallation.Value.LoadGameplayBasePalettes());
        runtimeFixtureBindings.Value(runtime);
        return runtime;
    }

    private static Action<SuperMetroidRuntime> CreateRuntimeFixtureBindings()
    {
        var installation = runtimeFixtureInstallation.Value;
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
