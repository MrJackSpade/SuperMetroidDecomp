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
        var approachPhaseFrames = new Dictionary<CeresDestructionPhase, int>();
        int zebesSlideFrames = 0;
        bool sawVisibleZebesDuringSlide = false;
        bool zebesLeftSlideViewport = false;
        int maximumZebesPalettePixels = 0;

        while (!cinematic.Finished && cinematicFrames < 5000)
        {
            cinematic.Step();
            cinematicFrames++;
            if (cinematic.Phase == CeresDestructionPhase.FlyingAwayFromExplosion)
                explosionFlightFrames++;
            if (cinematic.Phase is CeresDestructionPhase.FlyingTowardZebesA or
                CeresDestructionPhase.FlyingTowardZebesB or
                CeresDestructionPhase.FlyingTowardZebesC)
            {
                int phaseFrame = approachPhaseFrames.GetValueOrDefault(cinematic.Phase) + 1;
                approachPhaseFrames[cinematic.Phase] = phaseFrame;
                if (phaseFrame is 1 or 48 or 80 or 96 or 140 or 160 or 280)
                {
                    WriteOpaque(
                        Path.Combine(outputDirectory, $"{cinematic.Phase}.{phaseFrame:D3}.png"),
                        cinematic.Render(),
                        $"{cinematic.Phase} frame {phaseFrame}");
                }
            }
            if (cinematic.Phase == CeresDestructionPhase.SlideZebesSceneAway)
            {
                zebesSlideFrames++;
                Rgba32[] slideFrame = cinematic.Render();
                int zebesComponentPixels = FindLargestNonBlackComponent(slideFrame);
                maximumZebesPalettePixels = Math.Max(maximumZebesPalettePixels, zebesComponentPixels);
                if (zebesComponentPixels >= 128)
                {
                    if (zebesLeftSlideViewport)
                    {
                        throw new InvalidDataException(
                            $"Planet Zebes re-entered the visible slide after leaving it: " +
                            $"frame {zebesSlideFrames}, connected pixels {zebesComponentPixels}.");
                    }
                    sawVisibleZebesDuringSlide = true;
                }
                else if (sawVisibleZebesDuringSlide)
                {
                    zebesLeftSlideViewport = true;
                }
                if (zebesSlideFrames is 1 or 32 or 48 or 64)
                {
                    WriteOpaque(
                        Path.Combine(outputDirectory, $"ZebesSlide.{zebesSlideFrames:D3}.png"),
                        slideFrame,
                        $"Zebes slide frame {zebesSlideFrames}");
                }
            }
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

        if (!cinematic.Finished || !capturedExplosion || !capturedZebes ||
            !sawVisibleZebesDuringSlide || !zebesLeftSlideViewport)
        {
            throw new InvalidDataException(
                $"Ceres cinematic audit ended in {cinematic.Phase} after {cinematicFrames} calls; " +
                $"explosion capture={capturedExplosion}, Zebes capture={capturedZebes}, " +
                $"slide visible={sawVisibleZebesDuringSlide}, left={zebesLeftSlideViewport}, " +
                $"maximum planet-component pixels={maximumZebesPalettePixels}.");
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
            if (landingFrames == 1)
                AssertLandingPaletteFxFirstFrame(runtime);
            if (landingFrames is 160 or 320 or 480)
            {
                WriteOpaque(
                    Path.Combine(outputDirectory, $"GunshipLanding.{landingFrames:D3}.png"),
                    SuperMetroidRuntimeFrameRenderer.Render(runtime),
                    $"gunship landing frame {landingFrames}");
            }

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

        AssertVisibleLandingSkyRowsMatchRom(runtime, bus);

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
            restarted.GameplayTimeFrames != landingSave.GameTimeFrames)
        {
            throw new InvalidDataException(
                $"Fresh frontend did not resume the natural gunship save: " +
                $"state={restartedFrame.GameState}, " +
                $"room=${restarted.GameplayActiveRoomPointer.GetValueOrDefault():X4}/" +
                $"${runtime.ActiveRoom?.Pointer:X4}, energy={restarted.GameplayHealth}, " +
                $"time={restarted.GameplayTimeSeconds:D2}.{restarted.GameplayTimeFrames:D2}, " +
                $"movement={restarted.GameplayMovementEnabled}.");
        }

        // `$82:E4B6` deliberately enters main gameplay while the 360-frame load-station
        // appearance handler still owns Samus. Requiring movement on the first state-eight
        // frame would reject the real load animation added for issue #43. Advance through
        // that cartridge-owned sequence and require its ordinary-handler handoff instead.
        restartedFrame = FrontendAuditDriver.StepUntil(
            restarted,
            restartedFrame,
            _ => restarted.GameplayMovementEnabled,
            maximumFrames: 361,
            "saved-game load appearance did not restore ordinary movement");

        WriteOpaque(
            Path.Combine(outputDirectory, "LandingComplete.png"),
            SuperMetroidRuntimeFrameRenderer.Render(runtime),
            "Landing complete");

        // Stay inside Landing Site while exercising the exact camera range where a stale
        // horizontal BG layout used to run into empty/foreign VRAM. This is intentionally
        // a practical single-room input slice, not a scripted attempt to traverse Crateria.
        for (int frame = 1; frame <= 360; frame++)
        {
            runtime.StepFrame(controller1Input: 0x0200); // SNES Left
            if (frame % 120 == 0)
            {
                WriteOpaque(
                    Path.Combine(outputDirectory, $"LandingTravelLeft.{frame:D3}.png"),
                    SuperMetroidRuntimeFrameRenderer.Render(runtime),
                    $"Landing Site left travel frame {frame}");
            }
        }
        Console.WriteLine(
            $"Ceres/Zebes audit: cinematic {cinematicFrames} calls; station 0:18 gunship " +
            $"landing {landingFrames} gameplay frames from Y=${stationSamusY:X4}; Samus " +
            $"(${runtime.Samus!.XPosition:X4},${runtime.Samus.YPosition:X4}); automatic " +
            $"checkpoint resumed at {landingSave.GameTimeSeconds:D2}." +
            $"{landingSave.GameTimeFrames:D2}.");
        return 0;
    }

    /// <summary>
    /// Finds the largest four-connected, non-black shape in the final slide composite.
    /// Zebes is thousands of connected pixels while each surrounding star is only a small
    /// isolated shape. A wrapped OAM Y coordinate that makes the planet pass the screen
    /// repeatedly necessarily makes the large component return after it first disappears.
    /// </summary>
    private static int FindLargestNonBlackComponent(Rgba32[] pixels)
    {
        var visited = new bool[pixels.Length];
        var queue = new Queue<int>();
        int largest = 0;
        for (int origin = 0; origin < pixels.Length; origin++)
        {
            if (visited[origin] || IsBlack(pixels[origin]))
                continue;
            visited[origin] = true;
            queue.Enqueue(origin);
            int size = 0;
            while (queue.Count != 0)
            {
                int current = queue.Dequeue();
                size++;
                int x = current % FrontendFrame.Width;
                int y = current / FrontendFrame.Width;
                Visit(x - 1, y);
                Visit(x + 1, y);
                Visit(x, y - 1);
                Visit(x, y + 1);
            }
            largest = Math.Max(largest, size);
        }
        return largest;

        void Visit(int x, int y)
        {
            if ((uint)x >= FrontendFrame.Width || (uint)y >= FrontendFrame.Height)
                return;
            int index = y * FrontendFrame.Width + x;
            if (visited[index] || IsBlack(pixels[index]))
                return;
            visited[index] = true;
            queue.Enqueue(index);
        }

        static bool IsBlack(Rgba32 pixel) => pixel.R == 0 && pixel.G == 0 && pixel.B == 0;
    }

    /// <summary>
    /// Requires every currently visible circular BG2 row to equal its bank-$8A land chunk.
    /// </summary>
    private static void AssertVisibleLandingSkyRowsMatchRom(
        SuperMetroidRuntime runtime,
        SuperMetroidAddressSpace bus)
    {
        const int landChunkPointerTable = 0x88ad9c;
        const int bg2TilemapBaseWord = 0x4800;
        ushort cameraY = runtime.Camera?.YPosition
            ?? throw new InvalidDataException("Landing Site sky has no camera.");

        // Gameplay begins on physical scanline 32 and ends at 223. BG2VOFS is not reset by
        // the HUD IRQ, so these are the exact world rows sampled by the software PPU and the
        // cartridge. Comparing complete 32-word rows is independent of per-band BG2HOFS.
        int firstWorldY = (cameraY + SnesGameplayFrameRenderer.HudHeight) & ~7;
        int finalWorldY = (cameraY + FrontendFrame.Height - 1) & ~7;
        for (int worldY = firstWorldY; worldY <= finalWorldY; worldY += 8)
        {
            int chunk = (worldY >> 8) & 0xff;
            ushort chunkPointer = unchecked((ushort)(
                bus.ReadByte(landChunkPointerTable + chunk * 2) |
                (bus.ReadByte(landChunkPointerTable + chunk * 2 + 1) << 8)));
            int source = 0x8a0000 |
                unchecked((ushort)(chunkPointer + (worldY & 0xf8) * 8));
            int destination = bg2TilemapBaseWord + ((worldY >> 3) & 0x3f) * 32;
            for (int column = 0; column < 32; column++)
            {
                ushort expected = unchecked((ushort)(
                    bus.ReadByte(source + column * 2) |
                    (bus.ReadByte(source + column * 2 + 1) << 8)));
                ushort actual = runtime.Vram.ReadWord(destination + column);
                if (actual != expected)
                {
                    throw new InvalidDataException(
                        $"Landing Site visible sky row ${worldY:X4}, column {column} " +
                        $"contains ${actual:X4} at VRAM ${destination + column:X4}; " +
                        $"bank-$8A source ${source + column * 2:X6} contains ${expected:X4}.");
                }
            }
        }
    }

    /// <summary>Proves Landing Site spawns Crateria's cartridge palette program.</summary>
    private static void AssertLandingPaletteFxFirstFrame(SuperMetroidRuntime runtime)
    {
        // Object $8D:F765 begins at list $EB3B. Its first handler call executes SetPreInstr,
        // SetColorIndex($00A8), then the eight literal colors in the $00F0-frame record at
        // $EB43. These are the palette-five colors used by the scrolling-sky horizon.
        ushort[] expected =
        [
            0x2d6c, 0x294b, 0x252a, 0x2109,
            0x1ce8, 0x18c7, 0x14a6, 0x1085,
        ];
        for (int index = 0; index < expected.Length; index++)
        {
            ushort actual = runtime.Cgram.Colors[84 + index];
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"Landing palette-FX color {84 + index} is ${actual:X4}; " +
                    $"$8D:EB43 requires ${expected[index]:X4} on its first handler call.");
            }
        }
        if (runtime.RoomPaletteFx.ActiveCount != 1)
        {
            throw new InvalidDataException(
                $"Landing Site has {runtime.RoomPaletteFx.ActiveCount} room palette-FX objects; expected one.");
        }
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
