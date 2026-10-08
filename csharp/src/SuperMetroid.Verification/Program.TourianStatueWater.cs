using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
internal static partial class Program
{
    private static int VerifyTourianStatueWater()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        string fixtureRoot = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(fixtureRoot) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), fixtureRoot);
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        var maps = installation.LoadMaps();
        game.BindMapPresentation(maps);
        game.BindGameplayBasePalettes(installation.LoadGameplayBasePalettes());
        game.BindStandardObjectArt(installation.LoadStandardObjects());
        game.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        game.BindSamusBodyArt(installation.LoadSamusBodyArt());
        game.BindEndingMode7Art(installation.LoadEndingMode7Art());
        game.BindEndingObjectArt(installation.LoadEndingObjectArt());
        game.BindEndingPaletteArt(installation.LoadEndingPalettes());
        game.BindRoomCharacterArt(installation.LoadRoomCharacters());
        game.BindRoomPaletteArt(installation.LoadRoomPalettes());
        game.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        game.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
        game.BindRoomPlmShotBlockVisuals(installation.LoadRoomPlmShotBlockVisuals());
        game.BindRoomPlmGrappleBlockVisuals(installation.LoadRoomPlmGrappleBlockVisuals());
        game.BindRoomPlmStationVisuals(installation.LoadRoomPlmStationVisuals());
        game.BindRoomPlmBlueDoorVisuals(installation.LoadRoomPlmBlueDoorVisuals());
        game.BindRoomPlmColoredDoorVisuals(installation.LoadRoomPlmColoredDoorVisuals());
        game.BindRoomPlmGreyDoorVisuals(installation.LoadRoomPlmGreyDoorVisuals());
        game.BindRoomPlmEyeDoorVisuals(installation.LoadRoomPlmEyeDoorVisuals());
        game.BindRoomPlmMotherBrainGlassVisuals(installation.LoadRoomPlmMotherBrainGlassVisuals());
        game.BindRoomPlmNoobTubeVisuals(installation.LoadRoomPlmNoobTubeVisuals());
        game.BindRoomPlmDownwardGateVisuals(installation.LoadRoomPlmDownwardGateVisuals());
        game.BindRoomPlmElevatorPlatformVisuals(installation.LoadRoomPlmElevatorPlatformVisuals());
        game.BindRoomPlmEscapeGateVisuals(installation.LoadRoomPlmEscapeGateVisuals());
        game.BindRoomPlmBombTorizoHandVisuals(installation.LoadRoomPlmBombTorizoHandVisuals());
        game.BindRoomPlmDraygonCannonVisuals(installation.LoadRoomPlmDraygonCannonVisuals());
        game.BindRoomPlmChozoStatueVisuals(installation.LoadRoomPlmChozoStatueVisuals());
        game.BindRoomPlmLinkedRestoreVisuals(installation.LoadRoomPlmLinkedRestoreVisuals());
        game.BindRoomPlmTourianAccessVisuals(installation.LoadRoomPlmTourianAccessVisuals());
        game.BindRoomPlmSpeedBoosterVisuals(installation.LoadRoomPlmSpeedBoosterVisuals());
        game.BindRoomPlmMaridiaElevatubeVisuals(installation.LoadRoomPlmMaridiaElevatubeVisuals());
        game.BindRoomPlmSporeSpawnCeilingVisuals(installation.LoadRoomPlmSporeSpawnCeilingVisuals());
        game.BindRoomPlmSamusEaterVisuals(installation.LoadRoomPlmSamusEaterVisuals());
        game.BindRoomPlmBotwoonWallVisuals(installation.LoadRoomPlmBotwoonWallVisuals());
        game.BindRoomPlmKraidVisuals(installation.LoadRoomPlmKraidVisuals());
        game.BindRoomPlmCrocomireVisuals(installation.LoadRoomPlmCrocomireVisuals());
        game.BindRoomPlmMotherBrainFakeDeathVisuals(installation.LoadRoomPlmMotherBrainFakeDeathVisuals());
        game.BindRoomPlmCollectibleVisuals(installation.LoadRoomPlmCollectibleVisuals());
        game.BindRoomPlmDynamicCollectibleArt(installation.LoadRoomPlmDynamicCollectibleArt());
        game.BindXrayRevealVisuals(installation.LoadXrayRevealVisuals());
        game.BindRoomBackgroundTilemapArt(installation.LoadRoomBackgroundTilemaps());
        game.BindRoomSkyTilemapArt(installation.LoadRoomSkyTilemaps());
        var projectiles=installation.LoadProjectiles();
        game.BindProjectileCompositions(projectiles.Catalog);
        game.BindProjectileFrameBindings(projectiles.FrameBindings);
        game.BindBeamArtwork(projectiles.BeamTiles);
        game.BindEnemyTileArtwork(installation.LoadEnemyTiles());
        game.BindTrailArtwork(projectiles.Trails);
        game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        game.BindGrappleArtwork(projectiles.GrappleTiles);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",flags)!.Invoke(game,new object[]{false});
        var runtime=(SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime",flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertEqual(RoomFxType.TourianEntranceStatue, runtime.RoomLayer3Fx.Type, "Actual four-statues room owns special FX 26");
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.BaseYPosition, "Retail statues water begins at Y=B0");
        AssertEqual(true, runtime.RoomLayer3Fx.IsRenderable, "Statue special FX must supply its water plane");
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 64;
        samus.YPosition = 120;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.InputLocked = false;
        runtime.Camera!.SetPosition(0, 0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.MainGameplay);
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertEqual(RoomFxType.TourianEntranceStatue, samus.LiquidPhysics.FxType, "Samus retains native special FX identity");
        AssertEqual((ushort)0xb0, samus.LiquidPhysics.FxYPosition, "Water physics receives the visible surface");
        samus.YPosition = 190;
        samus.LiquidPhysics.InitializeRememberedMedium(samus);
        AssertEqual(SamusLiquidPhysicsState.Water, samus.LiquidPhysics.LiquidPhysicsType,
            "Native low-nibble FX dispatch selects water below the statue surface");
        var displayed = runtime.DisplayedRoomLayer3Fx ?? throw new InvalidOperationException("No displayed water snapshot");
        AssertEqual((short)0xb0, displayed.WaterSurfaceScreenY, "Displayed water surface stays at native room height");
        AssertEqual(LayerBlendingConfiguration.LiquidOrFogAdditive, displayed.LayerBlendConfiguration, "Native configuration 18");
        var basis = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var full = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        AssertEqual(1, full.Layers.ToArray().Count(layer => layer is Bg2BppColorMathRenderLayer), "One BG3 water plane accompanies the statue");
        var dryPixels = SoftwareLayeredSnapshotRenderer.Render(basis);
        var wetPixels = SoftwareLayeredSnapshotRenderer.Render(full);
        for (int y = 32; y < 169; y++)
            for (int x = 0; x < 256; x++)
                AssertEqual(dryPixels[y * 256 + x], wetPixels[y * 256 + x], "BG3 water does not tint pixels above its native surface tile");
        AssertTrue(Enumerable.Range(176 * 256, (224 - 176) * 256).Any(index => dryPixels[index] != wetPixels[index]),
            "Actual water artwork visibly changes submerged room pixels");
        var direct = dryPixels.ToArray();
        SnesGameplayFrameRenderer.ApplyRoomLayer3FxColorMath(direct, runtime.Vram, runtime.Cgram, displayed);
        AssertTrue(direct.SequenceEqual(wetPixels), "Immediate and captured composition agree for the actual water room");
        AssertEqual<ushort[]?>(null, SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(displayed, 0, 0),
            "Special statue FX does not install ordinary-water BG2 X-wave HDMA");
        typeof(TourianStatueSequence).GetField("descent", flags)!.SetValue(runtime.TourianStatues, 64 << 16);
        runtime.TourianStatues.LatchDisplay();
        runtime.RunNmi(0, true);
        var descending = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
        AssertEqual(unchecked((ushort)(runtime.DisplayedGameplayPpu.Layer1YPosition + runtime.TourianStatues.DisplayedVerticalOffset)),
            descending.Registers.Bg2Y, "Statue descent retains its independent BG2 vertical scroll");
        AssertEqual((short)0xb0, runtime.DisplayedRoomLayer3Fx!.Value.WaterSurfaceScreenY,
            "Statue motion does not move its water surface");
        runtime.RoomLayer3Fx.ApplyCartridgeMotionWrites(targetYPosition: 0xb4,
            packedYVelocity: 0x0100, timer: 1);
        game.Step(0);
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.CurrentYPosition, "Water motion first arms its native wait");
        game.Step(0);
        AssertEqual((ushort)0xb0, runtime.RoomLayer3Fx.CurrentYPosition, "Water wait expires before movement");
        game.Step(0);
        AssertEqual((ushort)0xb1, runtime.RoomLayer3Fx.CurrentYPosition, "Special water follows the native one-pixel velocity");
        AssertEqual((ushort)0xb1, samus.LiquidPhysics.FxYPosition, "The moved water surface reaches live Samus physics");
        AssertEqual<RoomFxEarthquakeRequest?>(null, runtime.RoomLayer3Fx.EarthquakeRequest,
            "Water motion does not invoke the lava/acid earthquake branch");
        AssertEqual(0, runtime.RoomLayer3Fx.SoundRequests.Count, "Water motion does not invoke lava/acid sound feedback");
        runtime.RoomLayer3Fx.Step(bus, runtime.Vram, 0, 0, timeIsFrozen: true, mainGameLoopCarry: true);
        AssertEqual((ushort)0xb1, runtime.RoomLayer3Fx.CurrentYPosition, "Frozen HDMA retains the current water surface");
        Console.WriteLine("Four-statues water: actual FX, liquid publication, visible BG3 pixels and independent statue descent verified.");
        return 0;
    }
}
