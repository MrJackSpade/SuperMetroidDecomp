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

        WriteOpaque(
            Path.Combine(outputDirectory, "LandingComplete.png"),
            SuperMetroidRuntimeFrameRenderer.Render(runtime),
            "Landing complete");
        Console.WriteLine(
            $"Ceres/Zebes audit: cinematic {cinematicFrames} calls; station 0:18 gunship " +
            $"landing {landingFrames} gameplay frames from Y=${stationSamusY:X4}; Samus " +
            $"(${runtime.Samus!.XPosition:X4},${runtime.Samus.YPosition:X4}).");
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
