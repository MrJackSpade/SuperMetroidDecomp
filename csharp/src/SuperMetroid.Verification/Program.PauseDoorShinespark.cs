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
    private static void VerifyPauseDoorShinespark()
    {
        foreach (var scenario in new[] { (Brightening: false, Terminal: false), (Brightening: false, Terminal: true), (Brightening: true, Terminal: false), (Brightening: true, Terminal: true) })
        {
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var installation=RepositoryInstallation.Installation;
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
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            var words = new ushort[32 * 16];
            var bts = new byte[words.Length];
            for (int row = 0; row < 16; row++)
            {
                words[row * 32 + 10] = 0x9000;
                bts[row * 32 + 10] = 3;
                words[row * 32 + 12] = 0x8000;
            }
            var level = new RoomLevelData(32, 16, words, bts, new ushort[512], new byte[8], doorListPointer: 0xdad5);
            typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.LevelData))!.SetValue(runtime, level);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.Health = 999;
            samus.Pose = SamusPoseId.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
            samus.Shinespark.BeginWindup(samus);
            samus.Shinespark.BeginDirectionalLaunch(bus, samus, SamusPoseId.ShinesparkHorizontalRightPose);
            samus.CommitPoseHistory(bus);
            samus.XPosition = 128;
            samus.YPosition = 128;
            typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            game.Step((ushort)SnesButton.Start);
            AssertEqual(SuperMetroidGameState.PausingDarkening, game.GameState, "Start begins darkening before reaching the open door");
            AssertTrue(!runtime.HasPendingDoorTransition, "initial Start frame is short of the door");
            if (scenario.Brightening)
                typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.Unpausing);
            typeof(SuperMetroidGame).GetField("pauseBrightness", flags)!.SetValue(game,
                (byte)(scenario.Brightening ? (scenario.Terminal ? 14 : 5) : (scenario.Terminal ? 1 : 15)));
            typeof(SuperMetroidGame).GetField("pauseFadeCounter", flags)!.SetValue(game, 0);
            // Put the leading edge immediately before the door for the next real mover call.
            samus.XPosition = (ushort)(160 - samus.Kinematics.XRadius - 1);
            game.Step(0);
            AssertTrue(runtime.HasPendingDoorTransition, "horizontal shinespark actually touches the open door during the fade");
            AssertEqual(ShinesparkPhase.Horizontal, samus.Shinespark.Phase, "door contact does not crash the shinespark");
            var expected = scenario.Terminal
                ? scenario.Brightening ? SuperMetroidGameState.MainGameplay : SuperMetroidGameState.LoadingNextRoomA
                : SuperMetroidGameState.HitDoorBlock;
            AssertEqual(expected, game.GameState, "door publication retains native fade completion ordering");
            if (!scenario.Brightening || !scenario.Terminal)
            {
                ushort contactX = samus.XPosition;
                game.Step(0);
                AssertEqual(SuperMetroidGameState.LoadingNextRoomB, game.GameState, "published door state starts the transition coroutine");
                AssertEqual(contactX, samus.XPosition, "no additional source-room shinespark movement before transition");
            }
        }
        Console.WriteLine("Pause-door shinespark: door hits during both fades preserve native normal/terminal state ordering and start transitions without extra source movement.");
    }
}
