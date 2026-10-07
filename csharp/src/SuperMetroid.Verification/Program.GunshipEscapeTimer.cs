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
    private static int VerifyGunshipEscapeTimer()
    {
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var installation = runtimeFixtureInstallation.Value;
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
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(0x91f8);
        runtime.Camera!.SetPosition(0x0400, 0x0400);
        var samus = runtime.Samus!;
        var top = runtime.Enemies.Slots.First(slot => slot.Definition.InitializationAiPointer == 0xa644);
        samus.InputLocked = false;
        samus.Pose = 1;
        samus.XPosition = top.XPosition;
        samus.YPosition = (ushort)(top.YPosition - 30);
        samus.InitializeAnimation(bus);
        samus.PrimeGraphics(bus);
        typeof(EscapeTimer).GetProperty(nameof(EscapeTimer.RawStatus))!.SetValue(runtime.EscapeTimer, (ushort)0x8006);
        runtime.EscapeTimer.SetTime(2, 0x59, 0x50);
        runtime.QueueEscapeTimerSpriteTiles();
        runtime.StepFrame(0);
        var timerTilesBefore = runtime.Vram.Bytes.Slice(0xfc00, 0x400).ToArray();
        AssertTrue(!runtime.Enemies.HasGunshipHealthHandler, "Idle ship retains normal Samus handler");
        AssertTrue(TimerSpritesPresent(), "Ordinary escape timer remains visible before boarding");
        samus.Pose = 1;
        samus.YPosition = 0x0440;
        samus.InitializeAnimation(bus);
        var timeBeforeBoarding = Time();
        runtime.StepFrame(0x0400);
        AssertEqual(GunshipFrameEvent.EntryStarted, runtime.Enemies.LastGunshipEvent, "Down starts actual Landing Site boarding");
        AssertEqual(timeBeforeBoarding, Time(), "Boarding handler stops the timer on its installation frame");
        AssertTrue(!TimerSpritesPresent(), "Boarding removes timer OAM immediately");
        bool sawDustUpload = false;
        for (int frame = 0; frame < 500; frame++)
        {
            runtime.StepFrame(0);
            AssertTrue(runtime.EscapeTimer.IsActive, "Gunship preserves native active timer status");
            AssertEqual(timeBeforeBoarding, Time(), "Gunship handler omits timer countdown");
            AssertTrue(!TimerSpritesPresent(), "No timer sprites reference repurposed dust tiles");
            if (top.VariableF == 0xac1b)
            {
                sawDustUpload = true;
                break;
            }
        }
        AssertTrue(sawDustUpload, "Confirmation reaches the actual liftoff dust upload");
        runtime.StepFrame(0); // NMI consumes the final queued dust tile transfer.
        AssertTrue(!runtime.Vram.Bytes.Slice(0xfc00, 0x400).SequenceEqual(timerTilesBefore), "Liftoff actually repurposes the timer tiles");
        AssertTrue(!TimerSpritesPresent(), "No stale timer OAM remains after dust artwork reaches VRAM");
        AssertEqual(timeBeforeBoarding, Time(), "Liftoff keeps the timer suspended");
        Console.WriteLine("Gunship escape timer: normal timer OAM, same-frame boarding suppression, preserved active status and no timer sprites through liftoff dust upload passed.");
        return 0;

        (byte, byte, byte) Time() => (runtime.EscapeTimer.MinutesBcd, runtime.EscapeTimer.SecondsBcd, runtime.EscapeTimer.CentisecondsBcd);
        bool TimerSpritesPresent()
        {
            var expected = new OamBuffer();
            EscapeTimerRenderer.Draw(runtime.EscapeTimer, expected, maps.EscapeTimer);
            for (int offset = 0; offset < runtime.Oam.LastFinalizedSpriteCount * 4; offset += 4)
                for (int timerOffset = 0; timerOffset < expected.NextByteOffset; timerOffset += 4)
                    if (runtime.Oam.LowTable.Slice(offset, 4).SequenceEqual(expected.LowTable.Slice(timerOffset, 4))) return true;
            return false;
        }
    }
}
