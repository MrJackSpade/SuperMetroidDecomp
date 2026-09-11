using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>First/subsequent scan frame captures and a controlled stale-reveal-memory experiment.</summary>
internal static class XrayFirstUseAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        SuperMetroidRuntime Create(byte staleByte)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.VramWrites.DrainTo(runtime.Vram, bus);
            runtime.LoadCartridgeRoomForDebug(XrayFirstUseFixtureData.RoomPointer);
            runtime.InitializeDebugGroundedSamus(128, 139, 12);
            var samus = runtime.Samus!;
            // The placement helper also moves the camera. This one-screen room was
            // filled at origin; keep its loaded viewport and test subject aligned.
            runtime.Camera!.SetPosition(0, 0);
            samus.XPosition = 128;
            samus.YPosition = 139;
            samus.InputLocked = false;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
            runtime.StepFrame(0);
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            runtime.StepFrame(0);
            if (samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
                throw new InvalidDataException("Controller input did not select X-ray.");
            for (int i = 0; i < XrayTilemapLayout.BufferWords * 2; i++)
                bus.WriteByte(XraySetupMemory.RevealTilemap + i, staleByte);
            return runtime;
        }
        var zero = Create(0);
        var filled = Create(0xff);
        CapturePacket(GameplayDisplayCapture.TryCaptureFrame(zero)!, 0);
        using var trace = new StreamWriter(Path.Combine(directory, "frames.csv"));
        trace.WriteLine("activation,frame,active,stage,phase,pixelDifferences");
        int comparisons = 0;
        for (int activation = 0; activation < 2; activation++)
        for (int frame = 0; frame < 75; frame++)
        {
            ushort input = frame < 45 ? zero.ControllerBindings.Dash : (ushort)0;
            zero.StepFrame(input);
            filled.StepFrame(input);
            var left = GameplayDisplayCapture.TryCaptureFrame(zero)!;
            var right = GameplayDisplayCapture.TryCaptureFrame(filled)!;
            CapturePacket(left, comparisons + 1);
            var pixels = SoftwareLayeredSnapshotRenderer.Render(left);
            var other = SoftwareLayeredSnapshotRenderer.Render(right);
            int differences = pixels.Zip(other).Count(pair => pair.First != pair.Second);
            var xray = zero.Samus!.Xray;
            trace.WriteLine($"{activation},{frame},{xray.IsActive},{xray.SetupStage},{xray.BeamPhase},{differences}");
            if (frame < 16 || frame is 30 or 45 or 46 or 47 or 48 or 49)
                PngWriter.WriteRgba(Path.Combine(directory, $"scan-{activation}-frame-{frame:D2}.png"), 256, 224, pixels);
            if (differences != 0)
                throw new InvalidDataException($"Stale reveal buffer visibly leaks: activation {activation}, frame {frame}, {differences} pixels.");
            if (frame == 30 && (!xray.IsActive || xray.SetupStage != 0))
                throw new InvalidDataException("Scan never reached active post-setup display.");
            if (frame == 74 && xray.IsActive) throw new InvalidDataException("Scan did not release.");
            comparisons++;
        }
        Console.WriteLine($"First/repeated X-ray: {comparisons} frame comparisons have no pixel dependence on zero versus FF stale reveal memory. This does not rule out other first-use corruption causes.");
        return 0;

        void CapturePacket(LayeredRenderSnapshot snapshot, int sequence)
            => File.WriteAllBytes(Path.Combine(directory, $"frame-{sequence:D4}.smframe"),
                RenderFrameSnapshotCodec.Serialize(new(new(sequence + 1, 1, (ushort)sequence), snapshot)));
    }
}
