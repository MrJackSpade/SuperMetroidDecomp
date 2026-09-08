using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyStatueEntry()
    {
        var loaded = DebuggerFixtureLoader.Load("issue-481-statue-room-entry", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        for (int frame = 0; frame < 3400; frame++)
        {
            var next = loaded.Game.Step(frame < 180 ? (ushort)SnesButton.Right : (ushort)0);
            if (frame == 240)
            {
                Console.WriteLine($"BG2={runtime.DisplayedGameplayPpu.Bg2HorizontalScroll},{runtime.DisplayedGameplayPpu.Bg2VerticalScroll} background={runtime.ActiveRoom!.State.BackgroundDataPointer:X4} FX={runtime.RoomLayer3Fx.Type}");
                var capture = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
                var registers = ((OrdinaryGameplayRenderLayer)capture.Layers[0]).Registers;
                if (registers.Bg2WidthTiles != 32 || registers.Bg2HeightTiles != 64)
                    throw new InvalidOperationException("Statue FX did not apply native BG2SC=$4A vertical tilemap layout.");
                var correct = SoftwareLayeredSnapshotRenderer.Render(capture);
                var bad = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(capture.Memory,
                    [new OrdinaryGameplayRenderLayer(registers with { Bg2WidthTiles = 64, Bg2HeightTiles = 32 })],
                    capture.ObjectSelection, capture.Brightness));
                int restoredPixels = 0;
                for (int y = 80; y < 180; y++)
                    for (int x = 60; x < 180; x++)
                        if (correct[y * 256 + x] != bad[y * 256 + x]) restoredPixels++;
                if (restoredPixels < 1000)
                    throw new InvalidOperationException("The saved entry no longer exercises the missing background statue artwork.");
                Console.WriteLine($"Restored statue artwork differs at {restoredPixels} pixels in the reported region.");
                Directory.CreateDirectory("csharp/test-temp/issue-481");
                PngWriter.WriteRgba("csharp/test-temp/issue-481/entry.png", 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
            }
            if (frame % 120 == 0)
                Console.WriteLine($"f={frame} room={runtime.ActiveRoom?.Pointer:X4} phase={next.Phase} samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition} statues={runtime.Enemies.TourianEntranceStatueAnimationState:X4} finished={runtime.Enemies.TourianEntranceStatueFinished}");
        }
        if (!runtime.System.HasEvent(EventNumber.TourianUnlocked))
            throw new InvalidOperationException("The four defeated bosses never opened Tourian after the statue sequence.");
        PngWriter.WriteRgba("csharp/test-temp/issue-481/open.png", 256, 224,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        for (int frame = 0; frame < 240; frame++)
            loaded.Game.Step(frame < 18 ? (ushort)SnesButton.Left : (ushort)0);
        Console.WriteLine($"After shaft approach Samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}, camera={runtime.Camera!.YPosition}");
        if (runtime.Samus.YPosition <= 256 || runtime.Camera.YPosition == 0)
            throw new InvalidOperationException("Tourian unlock did not make the shaft physically traversable with camera tracking.");
    }
}
