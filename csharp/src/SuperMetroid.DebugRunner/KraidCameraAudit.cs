using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

/// <summary>Retail-room entry jump, exercising the live camera and enemy initialization.</summary>
internal static class KraidCameraAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false, playerInvincibilityEnabled: true);
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader.Load(bus, KraidAuditDefinitions.EntryDoor), 0, 256);
        // Use the safe entrance ledge, not the solid floor beneath the spikes:
        // damage recoil there would confound a standing-jump reproduction.
        runtime.InitializeDebugGroundedSamus(48, 190, 20);
        // Grounded debug placement computes its own viewport. Restore the door's actual
        // entry camera before the first emulated frame so the jump is the only stimulus.
        runtime.Camera!.SetPosition(0, 256);
        runtime.Samus!.InputLocked = false;
        runtime.Samus.Health = runtime.Samus.MaxHealth = 999;
        byte[] initialScrolls = runtime.Camera!.Scrolls.Storage[..4].ToArray();
        ushort minCameraY = runtime.Camera.YPosition;
        ushort maxCameraY = minCameraY;
        ushort minSamusY = runtime.Samus.YPosition;
        ushort initialSamusY = minSamusY;
        int bodyPixelsAboveCeiling = 0;
        bool sawJump = false;
        int firstWrappedFrame = -1;
        for (int frame = 0; frame < 900; frame++)
        {
            runtime.StepFrame(frame % 90 < 35 ? (ushort)SnesButton.A : (ushort)0);
            sawJump |= runtime.Samus.ReadMovementType(bus) == SamusMovementType.NormalJumping;
            minCameraY = Math.Min(minCameraY, runtime.Camera.YPosition);
            maxCameraY = Math.Max(maxCameraY, runtime.Camera.YPosition);
            minSamusY = Math.Min(minSamusY, runtime.Samus.YPosition);
            var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("Kraid entry frame has no gameplay display capture.");
            var actual = SoftwareLayeredSnapshotRenderer.Render(packet);
            var layers = packet.Layers.ToArray();
            var ordinary = (OrdinaryGameplayRenderLayer)layers[0];
            layers[0] = new OrdinaryGameplayRenderLayer(ordinary.Registers with
                { MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2 },
                ordinary.HorizontalScrolls, ordinary.VerticalScrolls);
            var withoutBody = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                packet.Memory, layers, packet.ObjectSelection, packet.Brightness));
            // Kraid is the BG2 body in this room. Removing only BG2 from the same
            // immutable render packet identifies its visible contribution without
            // confusing moving Samus, HUD pixels, or the BG1 ceiling with the body.
            // During this undamaged first-phase sequence none belongs in this band.
            for (int y = KraidAuditDefinitions.EntryCeilingBandTop;
                 y < KraidAuditDefinitions.EntryCeilingBandBottom; y++)
                for (int x = 0; x < 256; x++)
                    if (actual[y * 256 + x] != withoutBody[y * 256 + x])
                        bodyPixelsAboveCeiling++;
            bool firstWrapped = firstWrappedFrame == -1 && bodyPixelsAboveCeiling > 0;
            if (firstWrapped) firstWrappedFrame = frame;
            if (frame == 13 || firstWrapped)
            {
                Directory.CreateDirectory("csharp/test-temp");
                PngWriter.WriteRgba(firstWrapped ? "csharp/test-temp/kraid-camera-519-wrapped.png" :
                    "csharp/test-temp/kraid-camera-519-frame13.png", 256, 224, actual, scale: 3);
            }
        }
        Console.WriteLine($"Kraid entry jump: scrolls=[{string.Join(',', initialScrolls)}], " +
            $"camera minimum={minCameraY}, Samus {initialSamusY}->{minSamusY}->{runtime.Samus.YPosition}, " +
            $"BG2 ceiling pixels={bodyPixelsAboveCeiling}, first wrapped frame={firstWrappedFrame}.");
        if (minSamusY >= initialSamusY || !sawJump)
            throw new InvalidDataException("Kraid entry fixture did not actually jump.");
        if (bodyPixelsAboveCeiling != 0)
            throw new InvalidDataException($"Kraid's BG2 body wrapped above the ceiling: " +
                $"{bodyPixelsAboveCeiling} pixel-frame differences, first at frame {firstWrappedFrame}.");
        if (!initialScrolls.SequenceEqual(new byte[] { 0, 0, 1, 0 }) ||
            minCameraY != 256 || maxCameraY != 256 || runtime.Enemies.Kraid!.CameraDistanceIndex != 2)
            throw new InvalidDataException("Kraid entry jump escaped the cartridge first-phase camera lock.");
        return 0;
    }
}
