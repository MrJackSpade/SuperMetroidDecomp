using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

internal static class RenderRetailFixture
{
    private static readonly Lazy<Action<SuperMetroidGame>> retailGameFixtureBindings = new(CreateRetailGameFixtureBindings);

    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true)
    {
        var game = new SuperMetroidGame(bus, gameOptions, renderGameplayFrames);
        retailGameFixtureBindings.Value(game);
        return game;
    }

    private static Action<SuperMetroidGame> CreateRetailGameFixtureBindings()
    {
        var installation = runtimeFixtureInstallation.Value;
        var assetMapPresentation = installation.LoadMaps();
        var assetGameplayBasePalettes = installation.LoadGameplayBasePalettes();
        var assetStandardObjectArt = installation.LoadStandardObjects();
        var assetIntroCinematicArt = installation.LoadIntroCinematicArt();
        var assetSamusBodyArt = installation.LoadSamusBodyArt();
        var assetEndingMode7Art = installation.LoadEndingMode7Art();
        var assetEndingObjectArt = installation.LoadEndingObjectArt();
        var assetEndingPaletteArt = installation.LoadEndingPalettes();
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
        return game => {
            game.BindMapPresentation(assetMapPresentation);
            game.BindGameplayBasePalettes(assetGameplayBasePalettes);
            game.BindStandardObjectArt(assetStandardObjectArt);
            game.BindIntroCinematicArt(assetIntroCinematicArt);
            game.BindSamusBodyArt(assetSamusBodyArt);
            game.BindEndingMode7Art(assetEndingMode7Art);
            game.BindEndingObjectArt(assetEndingObjectArt);
            game.BindEndingPaletteArt(assetEndingPaletteArt);
            game.BindRoomCharacterArt(assetRoomCharacterArt);
            game.BindRoomPaletteArt(assetRoomPaletteArt);
            game.BindRoomMetatileArt(assetRoomMetatileArt);
            game.BindRoomVisualLayouts(assetRoomVisualLayouts);
            game.BindRoomPlmShotBlockVisuals(assetRoomPlmShotBlockVisuals);
            game.BindRoomPlmGrappleBlockVisuals(assetRoomPlmGrappleBlockVisuals);
            game.BindRoomPlmStationVisuals(assetRoomPlmStationVisuals);
            game.BindRoomPlmBlueDoorVisuals(assetRoomPlmBlueDoorVisuals);
            game.BindRoomPlmColoredDoorVisuals(assetRoomPlmColoredDoorVisuals);
            game.BindRoomPlmGreyDoorVisuals(assetRoomPlmGreyDoorVisuals);
            game.BindRoomPlmEyeDoorVisuals(assetRoomPlmEyeDoorVisuals);
            game.BindRoomPlmMotherBrainGlassVisuals(assetRoomPlmMotherBrainGlassVisuals);
            game.BindRoomPlmNoobTubeVisuals(assetRoomPlmNoobTubeVisuals);
            game.BindRoomPlmDownwardGateVisuals(assetRoomPlmDownwardGateVisuals);
            game.BindRoomPlmElevatorPlatformVisuals(assetRoomPlmElevatorPlatformVisuals);
            game.BindRoomPlmEscapeGateVisuals(assetRoomPlmEscapeGateVisuals);
            game.BindRoomPlmBombTorizoHandVisuals(assetRoomPlmBombTorizoHandVisuals);
            game.BindRoomPlmDraygonCannonVisuals(assetRoomPlmDraygonCannonVisuals);
            game.BindRoomPlmChozoStatueVisuals(assetRoomPlmChozoStatueVisuals);
            game.BindRoomPlmLinkedRestoreVisuals(assetRoomPlmLinkedRestoreVisuals);
            game.BindRoomPlmTourianAccessVisuals(assetRoomPlmTourianAccessVisuals);
            game.BindRoomPlmSpeedBoosterVisuals(assetRoomPlmSpeedBoosterVisuals);
            game.BindRoomPlmMaridiaElevatubeVisuals(assetRoomPlmMaridiaElevatubeVisuals);
            game.BindRoomPlmSporeSpawnCeilingVisuals(assetRoomPlmSporeSpawnCeilingVisuals);
            game.BindRoomPlmSamusEaterVisuals(assetRoomPlmSamusEaterVisuals);
            game.BindRoomPlmBotwoonWallVisuals(assetRoomPlmBotwoonWallVisuals);
            game.BindRoomPlmKraidVisuals(assetRoomPlmKraidVisuals);
            game.BindRoomPlmCrocomireVisuals(assetRoomPlmCrocomireVisuals);
            game.BindRoomPlmMotherBrainFakeDeathVisuals(assetRoomPlmMotherBrainFakeDeathVisuals);
            game.BindRoomPlmCollectibleVisuals(assetRoomPlmCollectibleVisuals);
            game.BindRoomPlmDynamicCollectibleArt(assetRoomPlmDynamicCollectibleArt);
            game.BindXrayRevealVisuals(assetXrayRevealVisuals);
            game.BindRoomBackgroundTilemapArt(assetRoomBackgroundTilemapArt);
            game.BindRoomSkyTilemapArt(assetRoomSkyTilemapArt);
            game.BindEnemyTileArtwork(assetEnemyTileArtwork);
            game.BindProjectileCompositions(projectiles.Catalog);
            game.BindProjectileFrameBindings(projectiles.FrameBindings);
            game.BindBeamArtwork(projectiles.BeamTiles);
            game.BindTrailArtwork(projectiles.Trails);
            game.BindChargeFlarePlacement(projectiles.FlarePlacement);
            game.BindChargeFlareCompositions(projectiles.FlareCompositions);
            game.BindGrappleArtwork(projectiles.GrappleTiles);
        };
    }

    private static readonly Lazy<GameInstallation> runtimeFixtureInstallation = new(() =>
    {
        string root = Path.GetFullPath("csharp/test-temp/render-verification-installed-content");
        return GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
    });
    private static readonly Lazy<AreaMapPresentationCatalog> maps =
        new(() => runtimeFixtureInstallation.Value.LoadMaps());
    internal static AreaMapPresentationCatalog Maps => maps.Value;

    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => new(bus,
        titleGradientPresentation: Maps.TitleGradient,
        titlePalettePresentation: Maps.TitlePalette,
        titleGraphicsPresentation: Maps.TitleGraphics);

    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        new(bus, fixedColors: Maps.PowerBombFixedColors,
            artwork: runtimeFixtureInstallation.Value.LoadIntroCinematicArt(),
            paletteFxColors: Maps.RoomPaletteFx);

    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus)
    {
        var installation = runtimeFixtureInstallation.Value;
        var projectiles = installation.LoadProjectiles();
        var intro = new IntroCinematicState(bus, introFont: Maps.IntroFont,
            characterArtwork: installation.LoadIntroCinematicArt(),
            beamArtwork: projectiles.BeamTiles, samusBodyArtwork: installation.LoadSamusBodyArt())
        {
            ProjectileCompositions = projectiles.Catalog,
            ProjectileFrameBindings = projectiles.FrameBindings,
            TrailArtwork = projectiles.Trails,
            NarrationPresentation = Maps.IntroNarration,
        };
        intro.BindSamusHurtColors(Maps.SamusHurtColors);
        return intro;
    }
}