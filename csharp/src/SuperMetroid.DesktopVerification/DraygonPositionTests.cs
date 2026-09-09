using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Replays the preserved #381 encounter, checking the native draw hook and accepted-NMI handoff.</summary>
    private static void VerifyDraygonPosition()
    {
        var loaded = DebuggerFixtureLoader.Load("draygon-body-position", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var positions = new HashSet<(ushort, ushort)>();
        string output = "csharp/test-temp/issue-381-draygon";
        Directory.CreateDirectory(output);
        ushort previousX = 0, previousY = 0;
        int wrappedPixelsPrevented = 0;
        long terrainOverlapPixels = 0, bodyOverlapPixels = 0;
        long verifiedTilemapWords = 0;
        for (int frame = 0; frame < 600; frame++)
        {
            var before = runtime.Enemies.Draygon!;
            var window = DraygonMainScreenWindow.Select(before.Body.XPosition, before.Body.YPosition,
                runtime.Camera!.XPosition, runtime.Camera.YPosition, ((ushort)before.Body.Properties & 0x200) != 0);
            loaded.Game.Step(0);
            // The graphics-drawn hook uses the final camera, exactly like appendage OAM,
            // not the camera sampled before Samus movement/scrolling earlier in the frame.
            ushort cameraX = runtime.Camera!.XPosition;
            ushort cameraY = runtime.Camera.YPosition;
            var boss = runtime.Enemies.Draygon ?? throw new InvalidOperationException("Fixture must remain in Draygon's encounter.");
            verifiedTilemapWords += VerifyDraygonBodyTilemap(runtime);
            // Independent transcription of $A5:9342, including unsigned PPU-word wrapping.
            ushort expectedX = unchecked((ushort)(boss.BodyGraphicsXDisplacement + cameraX - boss.Body.XPosition - 450));
            ushort expectedY = unchecked((ushort)(boss.BodyGraphicsYDisplacement + cameraY - boss.Body.YPosition - 192));
            if (runtime.BackgroundScroll.Bg2HorizontalScroll != expectedX || runtime.BackgroundScroll.Bg2VerticalScroll != expectedY)
                throw new InvalidOperationException($"Draygon frame {frame}: body ({boss.Body.XPosition},{boss.Body.YPosition}), BG2 actual ({runtime.BackgroundScroll.Bg2HorizontalScroll},{runtime.BackgroundScroll.Bg2VerticalScroll}), native ({expectedX},{expectedY}).");
            if (frame > 0)
            {
                var layer = (OrdinaryGameplayRenderLayer)GameplayDisplayCapture.CaptureOrdinaryBase(runtime).Layers[0];
                if (layer.Registers.Bg2X != previousX || layer.Registers.Bg2Y != previousY)
                    throw new InvalidOperationException($"Draygon frame {frame}: accepted-NMI body scroll is detached from the matching sprite frame.");
                if (layer.Registers.Bg2FirstScanline != window.First || layer.Registers.Bg2EndScanline != window.End)
                    throw new InvalidOperationException("Draygon native main-screen window was not captured with its OAM.");
                if (frame % 30 == 0)
                {
                    var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
                    var overlap = VerifyDraygonTerrainPriority(capture);
                    terrainOverlapPixels += overlap.Terrain;
                    bodyOverlapPixels += overlap.Body;
                    var referenceLayer = new OrdinaryGameplayRenderLayer(layer.Registers with
                        { MainScreenLayers = layer.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2 });
                    var reference = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                        capture.Memory, new RenderLayer[] { referenceLayer }, capture.ObjectSelection, capture.Brightness));
                    var actual = SoftwareLayeredSnapshotRenderer.Render(capture);
                    var oldLayer = new OrdinaryGameplayRenderLayer(layer.Registers with { Bg2FirstScanline = 32, Bg2EndScanline = 224 });
                    var oldPixels = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                        capture.Memory, new RenderLayer[] { oldLayer }, capture.ObjectSelection, capture.Brightness));
                    for (int y = 32; y < 224; y++)
                        if (y < window.First || y >= window.End)
                            for (int x = 0; x < 256; x++)
                            {
                                if (actual[y * 256 + x] != reference[y * 256 + x])
                                    throw new InvalidOperationException("Wrapped Draygon BG2 remains visible outside the native HDMA band.");
                                if (oldPixels[y * 256 + x] != actual[y * 256 + x]) wrappedPixelsPrevented++;
                            }
                }
            }
            previousX = expectedX;
            previousY = expectedY;
            positions.Add((boss.Body.XPosition, boss.Body.YPosition));
            if (frame is 1 or 120 or 240 or 360 or 480)
                PngWriter.WriteRgba($"{output}/frame-{frame}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }
        if (positions.Count < 20) throw new InvalidOperationException("Draygon fixture did not exercise a moving body.");
        // Also move the captured BG2 artwork one complete tilemap height offscreen.
        // The PPU sampler alone repeats it unchanged; the native offscreen TM table
        // must suppress it. This constructed placement preserves the real ROM artwork.
        var finalCapture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
        var finalLayer = (OrdinaryGameplayRenderLayer)finalCapture.Layers[0];
        var offscreenRegisters = finalLayer.Registers with
            { Bg2Y = unchecked((ushort)(finalLayer.Registers.Bg2Y + 256)), Bg2FirstScanline = 32, Bg2EndScanline = 32 };
        var clipped = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(finalCapture.Memory,
            new RenderLayer[] { new OrdinaryGameplayRenderLayer(offscreenRegisters) }, finalCapture.ObjectSelection, finalCapture.Brightness));
        var unmasked = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(finalCapture.Memory,
            new RenderLayer[] { new OrdinaryGameplayRenderLayer(offscreenRegisters with { Bg2EndScanline = 224 }) }, finalCapture.ObjectSelection, finalCapture.Brightness));
        for (int i = 32 * 256; i < clipped.Length; i++)
            if (clipped[i] != unmasked[i]) wrappedPixelsPrevented++;
        if (wrappedPixelsPrevented == 0) throw new InvalidOperationException("Offscreen placement did not expose wrapping in the unmasked renderer.");
        Console.WriteLine($"Native BG2 HDMA removed {wrappedPixelsPrevented} wrapped pixels across sampled frames.");
        if (terrainOverlapPixels + bodyOverlapPixels == 0)
            throw new InvalidOperationException("Draygon priority fixture never overlapped body and terrain.");
        Console.WriteLine($"Draygon native BG overlap: terrain wins {terrainOverlapPixels} pixels; body wins {bodyOverlapPixels} pixels.");
        if (verifiedTilemapWords == 0) throw new InvalidOperationException("No native Draygon tilemap words checked.");
        Console.WriteLine($"Draygon retained ROM map commands: {verifiedTilemapWords} word comparisons verified, including priority bits.");
        foreach (var sample in new (int Y, int First, int End)[] { (-17, 32, 32), (-16, 32, 96),
            (39, 32, 96), (40, 32, 224), (191, 32, 224), (192, 128, 224), (303, 128, 224), (304, 32, 32) })
            if (DraygonMainScreenWindow.Select(100, unchecked((ushort)sample.Y), 0, 0, false) != (sample.First, sample.End))
                throw new InvalidOperationException("Draygon HDMA branch boundary differs from $88:DF94.");
        Console.WriteLine($"Draygon body alignment: 600 frames, {positions.Count} distinct positions; native BG2 anchor and captured NMI registers agree.");
    }
}
