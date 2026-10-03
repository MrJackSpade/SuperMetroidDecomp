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
    private static void VerifyChozoGrabAnimation()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation=new GameInstallation(GameAssetInstaller.DesktopRoot);
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
        runtime.System.SetBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xc98e);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.CollectedItems = samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.CommitPoseHistory(bus);
        samus.AnimationFrame = 2;
        typeof(SamusState).GetProperty(nameof(SamusState.AnimationFrameTimer))!.SetValue(samus, (ushort)3);
        samus.XPosition = 0x4a * 16 + 8;
        samus.YPosition = (ushort)(0x17 * 16 - samus.Kinematics.YRadius);
        var level = runtime.LevelData!;
        var block = level.GetCollisionBlockByIndex(level.GetBlockIndex(0x4a, 0x17));
        AssertEqual(ChozoStatuePlmRomData.WreckedShipHandBts.Value, block.Bts.Value,
            "fixture uses the retail statue hand block");
        runtime.Plms.NotifyChozoStatueHandCollision(level, block, samus, samus.Pose, movingDown: true);
        AssertTrue(samus.StationaryScriptControlLocked && samus.InputLocked,
            "hand trigger installs native stationary handlers");
        AssertEqual((ushort)2, samus.AnimationFrame, "grab preserves captured ball frame");
        AssertEqual((ushort)3, samus.AnimationFrameTimer, "grab preserves captured ball timer");
        ushort startX = samus.XPosition, startY = samus.YPosition;
        bool carried = false;
        int frames = 0;
        while (samus.StationaryScriptControlLocked && frames < 2000)
        {
            runtime.StepFrame((ushort)SnesButton.Right);
            if (samus.StationaryScriptControlLocked)
            {
                AssertEqual((ushort)2, samus.AnimationFrame, "ball frame remains frozen while carried");
                AssertEqual((ushort)3, samus.AnimationFrameTimer, "ball animation timer remains frozen while carried");
            }
            carried |= samus.XPosition != startX || samus.YPosition != startY;
            frames++;
        }
        AssertTrue(carried, "statue still moves Samus while animation is locked");
        AssertTrue(!samus.StationaryScriptControlLocked && !samus.InputLocked && runtime.GroundedSamusMovementEnabled,
            "native release restores handlers and movement");
        bool animationResumed = false;
        for (int frame = 0; frame < 12; frame++)
        {
            runtime.StepFrame((ushort)SnesButton.Right);
            animationResumed |= samus.AnimationFrame != 2 || samus.AnimationFrameTimer != 3;
        }
        AssertTrue(animationResumed, "ball animation resumes after release");
        Console.WriteLine($"Walking Chozo: native hand, frozen frame/timer through {frames} carry frames, movement and release verified.");
    }
}
