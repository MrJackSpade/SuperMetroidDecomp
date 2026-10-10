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
    private static int VerifyTorizoLandingLifetime()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=GameAssetInstaller.EnsureInstalled(Path.GetFullPath("out/workbook-investigation/crocomire-install"))!;
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
        runtime.LoadCartridgeRoomForDebug(0x9804);
        var body=runtime.Enemies.Slots.First(e=>e.EnemyDefinitionPointer==EnemyDefinitionId.BombTorizo);
        var state=(TorizoEnemyState)typeof(RoomEnemySystem).GetField("_torizoState",flags)!.GetValue(runtime.Enemies)!;
        state.Function=0xc6ff;state.PreInstruction=0xc82c;state.ReturnInstruction=0xbc88;
        state.VerticalVelocity=0x400;state.VerticalAcceleration=40;state.HorizontalVelocity=0;
        body.Health=body.Definition.Health;body.Parameter1=body.Parameter2=0;
        body.Properties=0x2000;body.CurrentInstruction=0xbc80;body.InstructionTimer=5;
        body.XPosition=128;body.YPosition=(ushort)(190-body.YRadius);
        var words=new ushort[256];for(int i=12*16;i<13*16;i++)words[i]=0x8000;
        var level=new RoomLevelData(16,16,words,new byte[256],new ushort[256],new byte[8]);
        typeof(SuperMetroidRuntime).GetProperty("LevelData")!.SetValue(runtime,level);
        var samus=runtime.Samus!;samus.Pose=SamusPoseId.FacingRightNormalPose;
        samus.Health=samus.MaxHealth=999;
        samus.RefreshCollisionRadii(bus);samus.InitializeAnimation(bus);samus.CommitPoseHistory(bus);
        samus.XPosition=240;samus.YPosition=(ushort)(192-samus.Kinematics.YRadius);samus.InputLocked=false;
        runtime.Camera!.SetPosition(0,0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game,SuperMetroidGameState.MainGameplay);
        var renderer=new CartridgeAudioRenderer(installation.LoadAudio());
        int shaken=0;
        for(int frame=0;frame<40;frame++)
        {
            var result=game.Step(0);
            renderer.RenderFrame(result.AudioCommands);game.SetAudioAcknowledgements(renderer.ReadAcknowledgements());
            // Every rendered earthquake type displaces BG1 or BG2, so only an unapplied frame leaves the default delta.
            var shake=runtime.Enemies.LastRoomShake;bool shakeApplied=shake!=default;if(shakeApplied)shaken++;
            if(frame%8==0 || frame>=30)Console.WriteLine($"frame={frame} actor={body.XPosition},{body.YPosition} hp={body.Health} pre={state.PreInstruction:X4} timer={runtime.Enemies.EarthquakeTimer} shake={shake}");
            if(shakeApplied!=(frame<32))throw new InvalidDataException("Full frontend landing-shake lifetime differs from 32 native frames.");
        }
        Console.WriteLine($"shaken={shaken}");
        return 0;
    }
}