using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Reproduces the player's detached upper body before any melting HDMA exists.</summary>
internal static class CrocomireBg2Audit
{
    private const ushort CrocomireRoom = 0xa98d;
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(CrocomireRoom, 956, 0);
        var samus = runtime.Samus!;
        samus.XPosition = 1052; samus.YPosition = 155;
        samus.InputLocked = true;
        for (int tick = 0; tick < 60; tick++)
        {
            runtime.StepFrame(0);
            var capture = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var layer = (OrdinaryGameplayRenderLayer)capture.Layers[0];
            if (!layer.VerticalScrolls.IsEmpty)
                throw new InvalidDataException($"Crocomire fight frame {tick} overrides body scroll {layer.Registers.Bg2Y:X4} with inactive HDMA {layer.VerticalScrolls[0]:X4}.");
            var pixels = SoftwareFrameSnapshotRenderer.Render(new RenderFrameSnapshot(new(tick + 1, 1, (ushort)tick), capture));
            if (!pixels.AsSpan().SequenceEqual(SuperMetroidRuntimeFrameRenderer.Render(runtime)))
                throw new InvalidDataException("Crocomire software/capture body alignment differs.");
            if (tick == 30)
            {
                string directory = Path.GetFullPath("csharp/test-temp/issue-329-inspection");
                Directory.CreateDirectory(directory);
                PngWriter.WriteRgba(Path.Combine(directory, "current-fight.png"), 256, 224, pixels);
            }
        }
        Console.WriteLine("Crocomire fight: 60 rendered frames use the body scroll register with no unspawned melting HDMA override.");
        return 0;
    }
}
