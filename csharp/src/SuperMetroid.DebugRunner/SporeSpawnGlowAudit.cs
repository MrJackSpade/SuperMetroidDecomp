using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Observes the real Spore Spawn room's palette cycle and visible background contribution.</summary>
internal static class SporeSpawnGlowAudit
{
    public static int Run(string rom, string output)
    {
        Directory.CreateDirectory(output);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader.Load(bus, SporeSpawnAuditDefinitions.IncomingDoorPointer),
            SporeSpawnAuditDefinitions.CameraX, SporeSpawnAuditDefinitions.IncomingDoorFinalCameraY);
        runtime.Samus!.XPosition = SporeSpawnAuditDefinitions.BodyCenterX;
        runtime.Samus.YPosition = SporeSpawnAuditDefinitions.EntryAuditSamusY;
        runtime.Samus.Health = runtime.Samus.MaxHealth = 1499;
        runtime.Samus.InputLocked = true;
        int movingFrames = 0, minimumVisible = int.MaxValue;
        using var trace = new StreamWriter(Path.Combine(output, "glow.csv"));
        trace.WriteLine("frame,function,color1,color2,color3,visiblePixels");
        for (int frame = 0; frame < 980; frame++)
        {
            runtime.StepFrame(0);
            var state = runtime.Enemies.SporeSpawn ?? throw new InvalidDataException("Spore Spawn did not load.");
            if (state.Function == SporeSpawnFunction.Moving) movingFrames++;
            var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)
                ?? throw new InvalidDataException("Missing gameplay capture.");
            ushort[] expected = SporeSpawnGlowReference.Colors[(frame / 10) % 14];
            for (int color = 0; color < 3; color++)
                if (packet.Memory.Cgram[SporeSpawnGlowReference.FirstColor + color] != expected[color])
                    throw new InvalidDataException($"Glow frame {frame}, color {color}: expected {expected[color]:X4}, actual {packet.Memory.Cgram[SporeSpawnGlowReference.FirstColor + color]:X4}.");
            var actual = SoftwareLayeredSnapshotRenderer.Render(packet);
            // Counterfactual changes only these three background colors. Therefore
            // unrelated boss/spore movement cannot count as visible glow evidence.
            var colors = packet.Memory.Cgram.ToArray();
            colors.AsSpan(SporeSpawnGlowReference.FirstColor, 3).Clear();
            var memory = new PpuMemorySnapshot(packet.Memory.Vram, colors, packet.Memory.Oam, packet.Memory.ModeledSpriteCount);
            var unlit = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(memory, packet.Layers, packet.ObjectSelection, packet.Brightness));
            int visible = 0;
            for (int pixel = FrontendFrame.Width * 32; pixel < actual.Length; pixel++)
                if (actual[pixel] != unlit[pixel]) visible++;
            if (visible == 0) throw new InvalidDataException($"Palette glow has no visible background contribution at frame {frame}.");
            minimumVisible = Math.Min(minimumVisible, visible);
            trace.WriteLine($"{frame},{state.Function},{expected[0]:X4},{expected[1]:X4},{expected[2]:X4},{visible}");
            if (frame is 0 or 70 or 140 or 280 or 560 or 630)
                PngWriter.WriteRgba(Path.Combine(output, $"glow-{frame:D4}.png"), FrontendFrame.Width, FrontendFrame.Height, actual);
        }
        if (movingFrames < 140) throw new InvalidDataException($"Insufficient active-fight coverage: {movingFrames} moving frames.");
        Console.WriteLine($"Spore glow: 980 palette/render frames, {movingFrames} moving-fight frames, minimum {minimumVisible} visible background pixels.");
        return 0;
    }
}

/// <summary>Independent transcription of $8D:EE35..EEC4, Spore Spawn's blue-spore glow.</summary>
internal static class SporeSpawnGlowReference
{
    /// <summary>$8D:EE31 sets byte color index E2: background palette 7, colors 1..3.</summary>
    public const int FirstColor = 113;
    /// <summary>Fourteen ten-frame records, bright-to-dim-to-bright, followed by the native loop.</summary>
    public static readonly ushort[][] Colors = [
        [0x5d22,0x4463,0x1840], [0x5901,0x4042,0x1420], [0x54e0,0x3c21,0x1000],
        [0x50c0,0x3c21,0x1000], [0x4ca0,0x3800,0x0c00], [0x4880,0x3800,0x0c00],
        [0x4460,0x3400,0x0800], [0x4040,0x3400,0x0800], [0x4460,0x3400,0x0800],
        [0x4880,0x3800,0x0c00], [0x4ca0,0x3800,0x0c00], [0x50c0,0x3c21,0x1000],
        [0x54e0,0x3c21,0x1000], [0x5901,0x4042,0x1420]];
}
