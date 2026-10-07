using SuperMetroid.Core.Rendering;
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
internal static partial class Program
{
    private static void VerifyXrayNoFxDarkening()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = runtimeFixtureInstallation.Value;
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        game.BindMapPresentation(installation.LoadMaps());
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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.ParlorAndAlcatraz);
        AssertTrue(runtime.RoomLayer3Fx.CaptureForDisplay() is null, "fixture has no visible room FX");
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        AssertTrue(samus.Xray.TryBegin(bus, samus, SamusMovementType.Standing), "fixture enters X-ray");
        // The closed-beam setup state makes the selected pixels unambiguously outside.
        typeof(SamusXrayState).GetProperty(nameof(SamusXrayState.SetupStage))!.SetValue(samus.Xray, (byte)3);
        runtime.RunNmi(0, mainLoopRequestedNmi: true);
        var tile = new byte[32];
        for (int row = 0; row < 8; row++) tile[row * 2] = 255;
        runtime.Vram.LoadBytes(0x3fe * 32, tile);
        runtime.Vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x3fe, 2048).ToArray(),
            SnesPpuLayout.GameplayBg1TilemapWord, 1);
        runtime.Cgram.SetColor(1, 24 | 20 << 5 | 16 << 10);
        // Native Clear_FX_Tilemap's $184E selects BG3 palette six, color three.
        runtime.Cgram.SetColor(6 * 4 + 3, 0);
        var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
        var expected = new Rgba32(99, 82, 66, 255); // RGB5(12,10,8)
        foreach (int index in new[] { 80 * 256 + 64, 120 * 256 + 128, 180 * 256 + 192 })
            AssertEqual(expected, pixels[index], "opaque black BG3 halves outside-beam foreground instead of brightening");
        var math = (GameplayColorMathRenderLayer)snapshot.Layers[0];
        AssertTrue(math.Subscreen is not null, "no-FX X-ray retains the actual BG3 plane");
        AssertEqual((ushort)0x184e, runtime.Vram.ReadWord(0x5880), "capture retains native cleared tilemap");
        // A transparent replacement must still take the hardware fixed-color fallback.
        runtime.Vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x006f, 0x780).ToArray(), 0x5880, 1);
        runtime.Vram.LoadBytes(runtime.GameplayHudCharacterBaseWord * 2 + 0x6f * 16, new byte[16]);
        var transparent = SoftwareLayeredSnapshotRenderer.Render(GameplayDisplayCapture.TryCaptureFrame(runtime)!);
        AssertEqual(new Rgba32(255, 222, 189, 255), transparent[80 * 256 + 64],
            "transparent BG3 preserves native fixed-color fallback without halving");
        typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.Type))!
            .SetValue(runtime.RoomLayer3Fx, RoomFxType.Fireflea);
        var fireflea = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        var firefleaMath = (GameplayColorMathRenderLayer)fireflea.Layers[0];
        AssertTrue(!firefleaMath.AddSubscreen && firefleaMath.Subscreen is null,
            "Fireflea retains fixed-color subtraction without a BG3 operand");
        var darkPixels = SoftwareLayeredSnapshotRenderer.Render(fireflea);
        AssertEqual(new Rgba32(140, 107, 74, 255), darkPixels[80 * 256 + 64],
            "Fireflea subtracts seven without halving");
        Console.WriteLine("X-ray no-FX capture: opaque BG3 darkening, transparent fallback and Fireflea exception verified.");
    }
}
