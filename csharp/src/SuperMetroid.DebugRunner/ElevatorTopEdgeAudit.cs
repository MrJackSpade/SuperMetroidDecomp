using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Captures #516's actual upward ride, not a fabricated pending door.</summary>
internal static class ElevatorTopEdgeAudit
{
    public static int Run(string rom, string directory, string? nativeArrivalCsv = null,
        bool greenBrinstar = false, string? nativeHandoffCsv = null)
    {
        if (greenBrinstar && nativeArrivalCsv is not null)
            throw new ArgumentException("The native arrival trace covers only the Blue Brinstar destination.");
        ushort sourceRoom = greenBrinstar ? RoomHeaderPointers.GreenBrinstarMainShaft : RoomHeaderPointers.MorphBallRoom;
        ushort destinationRoom = greenBrinstar ? RoomHeaderPointers.GreenBrinstarElevatorRoom : RoomHeaderPointers.BlueBrinstarElevatorRoom;
        ushort expectedDoor = greenBrinstar ? DoorPointers.GreenBrinstarElevatorFromMainShaft : DoorPointers.BlueBrinstarElevatorFromMorphBall;
        string[]? nativeArrival = nativeArrivalCsv is null ? null : File.ReadAllLines(nativeArrivalCsv);
        const int nativeArrivalFrames = 100;
        if (nativeArrival is not null && (nativeArrival.Length != nativeArrivalFrames + 1 ||
            nativeArrival[0] != "frame,samusY,cameraY,cameraSubY,status"))
            throw new InvalidDataException("Incomplete or unrecognized original-CPU elevator arrival trace.");
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.CollectedItems = samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Missiles = samus.MaxMissiles = 5;
        runtime.LoadCartridgeRoomForDebug(sourceRoom);
        var platform = runtime.Enemies.Slots.Single(s => s.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.XPosition = platform.XPosition;
        samus.YPosition = (ushort)(platform.YPosition - ElevatorActorDefinitions.SamusYOffset);
        runtime.Enemies.PublishElevatorDoorContact();
        for (int i = 0; runtime.PendingDoorTransition is null && i < 1200; i++)
        {
            ushort input = i % 30 == 0 ? (ushort)SnesButton.Up : (ushort)0;
            runtime.StepFrame(input);
        }
        if (runtime.PendingDoorTransition?.Pointer != expectedDoor)
            throw new InvalidDataException("The upward ride did not reach the reported elevator door.");
        Console.WriteLine($"Elevator source handoff: Samus={samus.XPosition:X4},{samus.Kinematics.YFixed:X8}, " +
            $"camera={runtime.Camera!.XPosition:X4},{runtime.Camera.YPosition:X4}, " +
            $"flags={runtime.Enemies.ElevatorFlags:X4}, status={(ushort)runtime.Enemies.ElevatorStatus}, " +
            $"direction={runtime.Enemies.ElevatorDirection:X4}.");
        using var trace = new StreamWriter(Path.Combine(directory, "frames.csv"));
        trace.WriteLine("frame,phase,room,samusY,cameraY,displayY,screenY,status,pose,animation,nmi");
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        int frame = 0;
        int wrappedPixelFrames = 0;
        int firstWrappedFrame = -1;
        while (transition.IsActive && frame < 600)
        {
            var phase = transition.Phase;
            ushort beforeCameraY = runtime.Camera!.YPosition;
            transition.Step(runtime, audio, 0);
            if (phase == DoorTransitionPhase.HandleTransition)
            {
                Console.WriteLine($"Destination handoff: Samus={samus.YPosition:X4}, camera={runtime.Camera.YPosition:X4}, " +
                    $"BG1={runtime.BackgroundScroll.Bg1VerticalScroll:X4}, displayedBG1={runtime.DisplayedGameplayPpu.Bg1VerticalScroll:X4}, " +
                    $"status={(ushort)runtime.Enemies.ElevatorStatus}, flags={runtime.Enemies.ElevatorFlags:X4}.");
                if (nativeHandoffCsv is not null)
                    ElevatorPpuWitness.CompareHandoff(runtime, nativeHandoffCsv);
            }
            if (phase is DoorTransitionPhase.HandleTransition or DoorTransitionPhase.FadeInDestinationPalette &&
                runtime.Camera.YPosition != beforeCameraY)
                throw new InvalidDataException($"Door fade ran gameplay camera: {beforeCameraY} -> {runtime.Camera.YPosition} during {phase}.");
            Capture(phase.ToString());
        }
        if (transition.IsActive) throw new InvalidDataException("Elevator transition timed out.");
        if (runtime.ActiveRoom!.Pointer != destinationRoom)
            throw new InvalidDataException("Elevator arrived in the wrong destination room.");
        int settled = 0;
        for (int i = 0; i < 600 && settled < 30; i++)
        {
            // StepFrame owns the accepted NMI, just as in SuperMetroidGame.
            // A second NMI uploads next-frame OAM early and defeats elevator flicker.
            runtime.StepFrame(0);
            if (nativeArrival is not null && i < nativeArrivalFrames)
            {
                string actual = $"{i},{samus.YPosition},{runtime.Camera!.YPosition}," +
                    $"{runtime.Camera.YSubposition},{(ushort)runtime.Enemies.ElevatorStatus}";
                if (actual != nativeArrival[i + 1])
                    throw new InvalidDataException($"Elevator arrival differs at frame {i}.\n" +
                        $"Native: {nativeArrival[i + 1]}\nManaged: {actual}");
                if (i == nativeArrivalFrames - 1)
                    Console.WriteLine($"Original-CPU arrival: {nativeArrivalFrames} frames match Samus Y, camera Y/subY and elevator status.");
            }
            Capture("Arrival");
            if (runtime.Enemies.ElevatorStatus == ElevatorActorStatus.Inactive) settled++;
        }
        if (settled != 30) throw new InvalidDataException("Elevator arrival did not settle.");
        Console.WriteLine($"Captured {frame} transition/arrival frames in {directory}; " +
            $"wrapped Samus pixel-frame differences={wrappedPixelFrames}, first frame={firstWrappedFrame}.");
        if (wrappedPixelFrames != 0)
            throw new InvalidDataException("Samus contributes visible top-edge pixels while her body is below the viewport.");
        return 0;

        void Capture(string phase)
        {
            int relativeY = unchecked((short)(samus.YPosition - runtime.DisplayedGameplayPpu.Layer1YPosition));
            trace.WriteLine($"{frame},{phase},{runtime.ActiveRoom!.Pointer:X4},{samus.YPosition},{runtime.Camera!.YPosition}," +
                $"{runtime.DisplayedGameplayPpu.Layer1YPosition},{relativeY},{runtime.Enemies.ElevatorStatus}," +
                $"{samus.Pose},{samus.AnimationFrame},{runtime.NmiFrameCounter}");
            // Capture EVERY displayed transition/arrival frame. Sampling every eighth
            // frame can alias the elevator's alternating visibility and systematically
            // omit Samus. Do not force a draw or add an NMI to compensate for flicker.
            {
                var packet = GameplayDisplayCapture.TryCaptureFrame(runtime);
                if (packet is null)
                    throw new InvalidDataException($"Missing display packet at elevator frame {frame} ({phase}).");
                if (packet is not null)
                {
                    var pixels = SoftwareLayeredSnapshotRenderer.Render(packet);
                    PngWriter.WriteRgba(Path.Combine(directory, $"frame-{frame:D4}.png"),
                        256, 224, pixels, scale: 3);
                    if (relativeY >= 256)
                    {
                        // Isolate visible Samus colors in the actual composite. Her
                        // dedicated OBJ palette is blackened only in a cloned packet;
                        // HUD, terrain, OAM ordering and live simulation are untouched.
                        ushort[] palette = packet.Memory.Cgram.ToArray();
                        Array.Clear(palette, 192, 16);
                        var memory = new PpuMemorySnapshot(packet.Memory.Vram, palette,
                            packet.Memory.Oam, packet.Memory.ModeledSpriteCount);
                        var blackSamus = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                            memory, packet.Layers, packet.ObjectSelection, packet.Brightness));
                        for (int pixel = 0; pixel < 256 * 80; pixel++)
                            if (pixels[pixel] != blackSamus[pixel])
                            {
                                if (wrappedPixelFrames < 8)
                                    Console.WriteLine($"Top-edge palette witness: frame={frame}, x={pixel % 256}, y={pixel / 256}, " +
                                        $"pose={samus.Pose:X2}, animation={samus.AnimationFrame}, nmi={runtime.NmiFrameCounter}.");
                                bool firstWitness = firstWrappedFrame < 0;
                                wrappedPixelFrames++;
                                if (firstWitness) firstWrappedFrame = frame;
                                if (firstWitness)
                                {
                                    ElevatorPpuWitness.Write(Path.Combine(directory, "local-ppu-witness.bin"), packet);
                                    ElevatorPpuWitness.InspectTerrain(runtime, packet, pixel % 256, pixel / 256);
                                    // Find the actual OAM owner instead of assuming that
                                    // every use of palette four belongs to Samus.
                                    for (int sprite = 0; sprite < packet.Memory.ModeledSpriteCount; sprite++)
                                    {
                                        byte[] oam = packet.Memory.Oam.ToArray();
                                        int offset = sprite * 4;
                                        if (((oam[offset + 3] >> 1) & 7) != 4) continue;
                                        byte originalY = oam[offset + 1];
                                        oam[offset + 1] = 240;
                                        var hiddenMemory = new PpuMemorySnapshot(packet.Memory.Vram,
                                            packet.Memory.Cgram, oam, packet.Memory.ModeledSpriteCount);
                                        var hidden = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                                            hiddenMemory, packet.Layers, packet.ObjectSelection, packet.Brightness));
                                        if (hidden[pixel] != pixels[pixel])
                                            Console.WriteLine($"Witness OAM owner={sprite}, x={oam[offset]}, y={originalY}, " +
                                                $"tile={oam[offset + 2]:X2}, attributes={oam[offset + 3]:X2}.");
                                    }
                                }
                            }
                    }
                }
            }
            frame++;
        }
    }
}
