using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Private-ROM smoke path for state $22 and station-eighteen Landing Site entry.</summary>
internal static class CeresDestructionAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        Directory.CreateDirectory(outputDirectory);
        var cinematic = new CeresDestructionCinematicState(bus);
        bool capturedExplosion = false;
        bool capturedZebes = false;
        int cinematicFrames = 0;
        int explosionFlightFrames = 0;

        while (!cinematic.Finished && cinematicFrames < 5000)
        {
            cinematic.Step();
            cinematicFrames++;
            if (cinematic.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
                explosionFlightFrames++;
            if (!capturedExplosion && explosionFlightFrames == 24)
            {
                // The phase boundary creates the terminal blast on its brightest first
                // map. Capture after several list frames so this audit shows recognizable
                // scene composition rather than an intentionally saturated flash onset.
                WriteOpaque(
                    Path.Combine(outputDirectory, "CeresExplosion.png"),
                    cinematic.Render(),
                    "Ceres explosion");
                capturedExplosion = true;
            }
            if (!capturedZebes &&
                cinematic.Phase == CeresDestructionPhase.PlanetZebesTitle)
            {
                // Let the actor list publish its first visible planet/title frame.
                for (int frame = 0; frame < 80; frame++)
                    cinematic.Step();
                cinematicFrames += 80;
                WriteOpaque(
                    Path.Combine(outputDirectory, "PlanetZebes.png"),
                    cinematic.Render(),
                    "Planet Zebes");
                capturedZebes = true;
            }
        }

        if (!cinematic.Finished || !capturedExplosion || !capturedZebes)
        {
            throw new InvalidDataException(
                $"Ceres cinematic audit ended in {cinematic.Phase} after {cinematicFrames} calls; " +
                $"explosion capture={capturedExplosion}, Zebes capture={capturedZebes}.");
        }

        // Retain a genuine fresh-game Samus/system owner, then execute the exact special
        // loader branch that follows CADF. This avoids fabricating inventory or progression
        // while still skipping the several minutes of interactive Ceres controller input.
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();

        if (runtime.ActiveLoadStation is not { RequestedAreaIndex: 0, StationIndex: 18 })
        {
            throw new InvalidDataException(
                "Post-Ceres loader did not retain Crateria station eighteen.");
        }

        ushort stationSamusY = runtime.Samus?.YPosition
            ?? throw new InvalidDataException("Post-Ceres loader did not retain Samus.");
        int landingFrames = 0;
        while (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted &&
               landingFrames < 1200)
        {
            runtime.StepFrame(0);
            landingFrames++;

            // Native function three translates the top, bottom, pad, and Samus by the
            // same fixed-point delta. Fail at the first broken frame so a later scheduler
            // cutoff cannot hide which runtime owner overwrote the carried Samus position.
            RoomEnemySlot movingTop = runtime.Enemies.Slots[0];
            if (movingTop.VariableF == 0xa80c &&
                runtime.Samus is { } carriedSamus &&
                carriedSamus.YPosition != unchecked((ushort)(movingTop.YPosition + 17)))
            {
                throw new InvalidDataException(
                    $"Landing Site gunship lost rigid Samus carry on frame {landingFrames}: " +
                    $"top Y ${movingTop.YPosition:X4}.{movingTop.YSubposition:X4}, " +
                    $"Samus Y ${carriedSamus.YPosition:X4}.{carriedSamus.Kinematics.YSubposition:X4}, " +
                    $"camera Y ${runtime.Camera?.YPosition:X4}.");
            }
        }
        if (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted)
        {
            RoomEnemySlot top = runtime.Enemies.Slots[0];
            throw new InvalidDataException(
                $"Landing Site gunship did not restore control within {landingFrames} frames: " +
                $"function $A2:{top.VariableF:X4}, top Y ${top.YPosition:X4}.{top.YSubposition:X4}, " +
                $"timer A ${top.VariableA:X4}, Samus Y ${runtime.Samus?.YPosition:X4}.");
        }
        if (!runtime.GroundedSamusMovementEnabled || runtime.Samus?.InputLocked != false)
            throw new InvalidDataException("Landing completed without restoring ordinary Samus control.");

        // Feed the naturally produced GunshipTop_7 event into the same automatic-checkpoint
        // coordinator used by SuperMetroidGame, then restart the entire frontend and select
        // that slot through controller input. This closes the loop from gameplay event to
        // physical SRAM to cartridge load-station resolution—no seeded snapshot participates.
        if (!AutomaticCheckpointSaver.TrySaveGunshipLanding(bus, runtime, selectedSaveSlot: 0))
            throw new InvalidDataException("Natural gunship completion did not request its automatic save.");
        SuperMetroidSaveSlot landingSave = new SuperMetroidSaveRam(bus).ReadSlot(0)
            ?? throw new InvalidDataException("Gunship checkpoint failed SRAM checksum validation.");
        if (landingSave.Area != 0 || landingSave.SaveStation != 0 ||
            (landingSave.UsedSaveStationBytes[0] & 1) == 0 ||
            landingSave.GameTimeFrames != runtime.GameTime.Frames ||
            landingSave.GameTimeSeconds != runtime.GameTime.Seconds)
        {
            throw new InvalidDataException(
                $"Gunship checkpoint decoded as {landingSave.Area}:{landingSave.SaveStation}, " +
                $"stationBits=${landingSave.UsedSaveStationBytes[0]:X2}, " +
                $"time={landingSave.GameTimeSeconds:D2}.{landingSave.GameTimeFrames:D2}, " +
                $"runtime={runtime.GameTime.Seconds:D2}.{runtime.GameTime.Frames:D2}.");
        }

        var restarted = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame restartedFrame = FrontendAuditDriver.EnterSelectedSlot(restarted);
        if (restartedFrame.GameState != SuperMetroidGameState.MainGameplay ||
            restarted.GameplayActiveRoomPointer != runtime.ActiveRoom?.Pointer ||
            restarted.GameplayHealth != landingSave.Health ||
            restarted.GameplayTimeSeconds != landingSave.GameTimeSeconds ||
            restarted.GameplayTimeFrames != landingSave.GameTimeFrames ||
            !restarted.GameplayMovementEnabled)
        {
            throw new InvalidDataException(
                $"Fresh frontend did not resume the natural gunship save: " +
                $"state={restartedFrame.GameState}, " +
                $"room=${restarted.GameplayActiveRoomPointer.GetValueOrDefault():X4}/" +
                $"${runtime.ActiveRoom?.Pointer:X4}, energy={restarted.GameplayHealth}, " +
                $"time={restarted.GameplayTimeSeconds:D2}.{restarted.GameplayTimeFrames:D2}, " +
                $"movement={restarted.GameplayMovementEnabled}.");
        }

        WriteOpaque(
            Path.Combine(outputDirectory, "LandingComplete.png"),
            SuperMetroidRuntimeFrameRenderer.Render(runtime),
            "Landing complete");
        Console.WriteLine(
            $"Ceres/Zebes audit: cinematic {cinematicFrames} calls; station 0:18 gunship " +
            $"landing {landingFrames} gameplay frames from Y=${stationSamusY:X4}; Samus " +
            $"(${runtime.Samus!.XPosition:X4},${runtime.Samus.YPosition:X4}); automatic " +
            $"checkpoint resumed at {landingSave.GameTimeSeconds:D2}." +
            $"{landingSave.GameTimeFrames:D2}.");
        return 0;
    }

    private static void WriteOpaque(string path, Rgba32[] pixels, string description)
    {
        int transparentPixel = Array.FindIndex(pixels, pixel => pixel.A != byte.MaxValue);
        if (transparentPixel >= 0)
        {
            throw new InvalidDataException(
                $"{description} has a non-opaque pixel at index {transparentPixel}.");
        }
        PngWriter.WriteRgba(path, FrontendFrame.Width, FrontendFrame.Height, pixels);
        Console.WriteLine($"Captured {description} to {Path.GetFullPath(path)}.");
    }
}
