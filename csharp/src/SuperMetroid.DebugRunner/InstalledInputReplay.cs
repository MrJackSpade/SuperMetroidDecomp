using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;

/// <summary>
/// Replays a reported input interval using installed content and recording-owned SRAM.
/// It never subscribes to disk-save events or changes the player's installation.
/// The outer DebugRunner boundary owns no-dialog policy and error reporting.
/// </summary>
internal static class InstalledInputReplay
{
    internal static int Run(string recordingPath, string installationRoot,
        int firstFrame, int lastFrame, string tracePath)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        if (firstFrame < 0 || lastFrame < firstFrame || lastFrame >= recording.ControllerInputs.Length)
            throw new ArgumentOutOfRangeException(nameof(lastFrame));
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
        Bind(game, installation);
        var audio = new CartridgeAudioRenderer(installation.LoadAudio());
        using var output = new StreamWriter(tracePath);
        output.WriteLine("frame,room,state,input,latched,new,pose,x,y,xFixed,yFixed,base,baseSub,extra,extraSub,accel,momentum,yDirection,ySpeed,knockback,locked,health,cameraY");
        for (int frame = 0; frame <= lastFrame; frame++)
        {
            try
            {
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                var result = game.Step(recording.ControllerInputs[frame]);
                audio.RenderFrame(result.AudioCommands);
            }
            catch (Exception error)
            {
                throw new InvalidDataException($"Reported recording failed at input frame {frame}, room {game.GameplayActiveRoomPointer:X4}.", error);
            }
            if (frame < firstFrame || game.RuntimeForVerification is not { } runtime) continue;
            if (runtime.Samus is not { } samus) continue;
            output.WriteLine(string.Join(',', frame.ToString(CultureInfo.InvariantCulture),
                $"{runtime.ActiveRoom?.Pointer:X4}", game.GameState,
                $"{recording.ControllerInputs[frame]:X4}", $"{runtime.Controller1.Current:X4}",
                $"{runtime.Controller1.NewlyPressed:X4}", $"{samus.Pose:X2}",
                samus.XPosition, samus.YPosition, samus.Kinematics.XFixed, samus.Kinematics.YFixed,
                samus.HorizontalSpeed.BaseSpeed, samus.HorizontalSpeed.BaseSubspeed,
                samus.HorizontalSpeed.ExtraRunSpeed, samus.HorizontalSpeed.ExtraRunSubspeed,
                samus.HorizontalSpeed.AccelerationMode, samus.HorizontalSpeed.HasRunningMomentum,
                samus.Kinematics.YDirection, samus.Kinematics.YSpeed,
                samus.KnockbackActive, samus.InputLocked, samus.Health, runtime.Camera?.YPosition));
        }
        Console.WriteLine($"Reported input interval {firstFrame}..{lastFrame} written to {Path.GetFullPath(tracePath)}; total inputs={recording.ControllerInputs.Length}.");
        return 0;
    }

    // Match the current host's presentation bindings without opening a UI, installing
    // assets, loading disk saves or writing a new input recording.
    internal static void Bind(SuperMetroidGame game, GameInstallation installation)
    {
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
        var projectiles = installation.LoadProjectiles();
        game.BindProjectileCompositions(projectiles.Catalog);
        game.BindProjectileFrameBindings(projectiles.FrameBindings);
        game.BindBeamArtwork(projectiles.BeamTiles);
        game.BindTrailArtwork(projectiles.Trails);
        game.BindChargeFlarePlacement(projectiles.FlarePlacement);
        game.BindChargeFlareCompositions(projectiles.FlareCompositions);
        game.BindGrappleArtwork(projectiles.GrappleTiles);
        game.BindEnemyTileArtwork(installation.LoadEnemyTiles());
    }
}
