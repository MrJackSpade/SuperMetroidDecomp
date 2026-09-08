using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

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
        for (int frame = 0; frame < 600; frame++)
        {
            loaded.Game.Step(0);
            // The graphics-drawn hook uses the final camera, exactly like appendage OAM,
            // not the camera sampled before Samus movement/scrolling earlier in the frame.
            ushort cameraX = runtime.Camera!.XPosition;
            ushort cameraY = runtime.Camera.YPosition;
            var boss = runtime.Enemies.Draygon ?? throw new InvalidOperationException("Fixture must remain in Draygon's encounter.");
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
            }
            previousX = expectedX;
            previousY = expectedY;
            positions.Add((boss.Body.XPosition, boss.Body.YPosition));
            if (frame is 1 or 120 or 240 or 360 or 480)
                PngWriter.WriteRgba($"{output}/frame-{frame}.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }
        if (positions.Count < 20) throw new InvalidOperationException("Draygon fixture did not exercise a moving body.");
        Console.WriteLine($"Draygon body alignment: 600 frames, {positions.Count} distinct positions; native BG2 anchor and captured NMI registers agree.");
    }
}
