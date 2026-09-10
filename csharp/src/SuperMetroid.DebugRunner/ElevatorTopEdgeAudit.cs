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
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var samus = runtime.Samus!;
        samus.CollectedItems = samus.EquippedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Missiles = samus.MaxMissiles = 5;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.MorphBallRoom);
        var platform = runtime.Enemies.Slots.Single(s => s.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.XPosition = platform.XPosition;
        samus.YPosition = (ushort)(platform.YPosition - ElevatorActorDefinitions.SamusYOffset);
        runtime.Enemies.PublishElevatorDoorContact();
        for (int i = 0; runtime.PendingDoorTransition is null && i < 1200; i++)
        {
            ushort input = i % 30 == 0 ? (ushort)SnesButton.Up : (ushort)0;
            runtime.StepFrame(input);
            runtime.RunNmi(input, true);
        }
        if (runtime.PendingDoorTransition?.Pointer != DoorPointers.BlueBrinstarElevatorFromMorphBall)
            throw new InvalidDataException("The upward ride did not reach the reported elevator door.");
        using var trace = new StreamWriter(Path.Combine(directory, "frames.csv"));
        trace.WriteLine("frame,phase,room,samusY,cameraY,displayY,screenY,status");
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
            if (phase is DoorTransitionPhase.HandleTransition or DoorTransitionPhase.FadeInDestinationPalette &&
                runtime.Camera.YPosition != beforeCameraY)
                throw new InvalidDataException($"Door fade ran gameplay camera: {beforeCameraY} -> {runtime.Camera.YPosition} during {phase}.");
            Capture(phase.ToString());
        }
        if (transition.IsActive) throw new InvalidDataException("Elevator transition timed out.");
        int settled = 0;
        for (int i = 0; i < 600 && settled < 30; i++)
        {
            runtime.StepFrame(0); runtime.RunNmi(0, true);
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
                $"{runtime.DisplayedGameplayPpu.Layer1YPosition},{relativeY},{runtime.Enemies.ElevatorStatus}");
            // Capture each potentially wrapped arrival frame, plus periodic settled
            // views. These are observations; no camera or sprite state is modified.
            if (runtime.ActiveRoom.Pointer == RoomHeaderPointers.BlueBrinstarElevatorRoom &&
                (relativeY >= 224 || frame % 8 == 0))
            {
                var packet = GameplayDisplayCapture.TryCaptureFrame(runtime);
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
                                wrappedPixelFrames++;
                                if (firstWrappedFrame < 0) firstWrappedFrame = frame;
                            }
                    }
                }
            }
            frame++;
        }
    }
}
