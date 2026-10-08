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
    private static void VerifyWaterfallRooms()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        string fixtureRoot = Path.GetFullPath("out/verification/WaterfallRooms-install");
        var installation = GameAssetInstaller.EnsureInstalled(fixtureRoot)
            ?? GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), fixtureRoot);
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
        foreach (ushort room in new ushort[] { 0xd72a, 0xd913 })
        {
            runtime.LoadCartridgeRoomForDebug(room);
            if (runtime.RoomLayer3Fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
                throw new InvalidDataException($"Reported waterfall room {room:X4} lost configuration $16.");
            typeof(SuperMetroidRuntime).GetMethod("PrepareGameplayWindowRegisters", flags)!.Invoke(runtime, null);
            runtime.RoomLayer3Fx.Step(bus, runtime.Vram, runtime.Camera!.XPosition,
                runtime.Camera.YPosition, timeIsFrozen: false, mainGameLoopCarry: true);
            runtime.RunNmi(0, mainLoopRequestedNmi: true);
            var registers = runtime.DisplayedGameplayWindowRegisters;
            AssertEqual((byte)0x11, registers.MainScreen, "waterfall native TM");
            AssertEqual((byte)0x06, registers.Subscreen, "waterfall native TS");
            var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("Waterfall room capture is absent.");
            var math = snapshot.Layers[0] as GameplayColorMathRenderLayer
                ?? throw new InvalidDataException("Waterfall room still captures an ordinary opaque background.");
            AssertTrue(math.SubscreenUsesBg2, "waterfall capture admits BG2 subscreen");
            AssertEqual((byte)0xb1, (byte)math.ColorMath, "waterfall native CGADSUB");
            AssertEqual(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj,
                math.Gameplay.Registers.MainScreenLayers, "waterfall main excludes BG2");
            AssertTrue(!snapshot.Layers.ToArray().Any(layer => layer is Bg2BppColorMathRenderLayer),
                "waterfall has no second post-composition liquid overlay");
            var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            AssertEqual(256 * 224, pixels.Length, "reported room composes full frame");
            Console.WriteLine($"Waterfall room {room:X4}: TM=11 TS=06 CGADSUB=B1, combined subscreen capture passed.");
        }
    }
}