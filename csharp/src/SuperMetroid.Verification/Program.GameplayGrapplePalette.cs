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
    private static void VerifyGameplayGrapplePalette()
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
        for (int i = 12 * 32; i < 13 * 32; i++) words[i] = 0x8000;
        var level = new RoomLevelData(32, 16, words, new byte[512], new ushort[512], new byte[8]);
        typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.LevelData))!.SetValue(runtime, level);
        var samus = runtime.Samus!;
        var expected = new SnesCgram();
        var palettes = projectiles.BeamTiles.Palettes ?? throw new InvalidDataException("Missing beam palettes.");
        palettes.LoadTo(expected, 2);
        foreach (ushort beam in new ushort[] { 0, 4 })
        {
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.InputLocked = false;
            samus.EquippedItems = samus.CollectedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
            samus.EquippedBeams = beam;
            samus.SelectedHudItem = SamusHudRomData.GrappleSelectedItem;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            samus.XPosition = 128;
            samus.YPosition = (ushort)(192 - samus.Kinematics.YRadius);
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            runtime.StepFrame(0);
            palettes.LoadTo(runtime.Cgram, beam);
            runtime.Cgram.SetColor(223, Bgr555.FromWord(0x1234));
            runtime.StepFrame((ushort)SnesButton.X);
            AssertTrue(runtime.LastGrappleMovement is { Fired: true }, "normal HUD input fires grapple");
            AssertPalette("normal firing");
            samus.Pose = SamusPoseIds.StandingAimDiagonalUpRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            palettes.LoadTo(runtime.Cgram, beam);
            runtime.Cgram.SetColor(223, Bgr555.FromWord(0x1234));
            runtime.StepFrame((ushort)SnesButton.X);
            AssertTrue(runtime.LastGrappleMovement is { Fired: true }, "native pose-change window refires grapple");
            AssertPalette("pose-change refiring");
            for (int frame = 0; frame < 4 && samus.Grapple.Phase != GrapplePhase.Inactive; frame++)
                runtime.StepFrame(0);
            AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "release cancels extending grapple");
            var ordinary = new SnesCgram();
            palettes.LoadTo(ordinary, beam);
            AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(ordinary.Colors.Slice(224, 16)),
                "cancel restores equipped beam palette");
        }
        Console.WriteLine("Gameplay grapple palette: normal HUD firing, refire, fixed flare color and cancellation restoration verified.");
        void AssertPalette(string phase)
        {
            AssertTrue(runtime.Cgram.Colors.Slice(224, 16).SequenceEqual(expected.Colors.Slice(224, 16)),
                phase + " loads native palette two");
            AssertEqual((ushort)0x7f91, runtime.Cgram.Colors[223], phase + " sets native flare color");
        }
    }
}
