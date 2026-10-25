using System.Diagnostics;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.Diagnostics.Sessions;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Reaches a measured autonomous host frame without changing the demo sequence.</summary>
internal static class AutonomousPerformanceStateFixture
{
    /// <summary>Advances the autonomous session through the requested frame, measures repeated renders of its immutable packet, and exports the frame and save slot.</summary>
    public static int Export(int targetFrame, string destination)
    {
        if (targetFrame is < 1 or > 100000)
            throw new ArgumentOutOfRangeException(nameof(targetFrame));
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root)) throw new IOException($"Fixture directory already exists: {root}");
        Directory.CreateDirectory(root);
        using var data = new InstalledGameSession(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        for (int frame = 1; frame <= targetFrame; frame++)
        {
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            // Rendering observes immutable packets and is not needed to advance earlier frames.
            if (frame != targetFrame) continue;
            var packet = result.Snapshot;
            for (int warm = 0; warm < 10; warm++) DisplayRendering.Render(packet);
            long start = Stopwatch.GetTimestamp();
            RgbaPixel[] pixels = [];
            for (int sample = 0; sample < 100; sample++) pixels = DisplayRendering.Render(packet);
            Console.WriteLine($"Frame {frame}: {result.Frame.GameState}, room {data.Game.GameplayActiveRoomIdentity}, render={Stopwatch.GetElapsedTime(start).TotalMilliseconds / 100:F3}ms");
            if (packet.Layers is { } layers)
                foreach (var layer in layers.Layers) Console.WriteLine($"Layer: {layer.GetType().Name}");
            PngWriterTooling.WriteRgba(Path.Combine(root, "frame.png"), packet.Size, pixels);
            Console.WriteLine(data.SaveSlot(9));
        }
        return 0;
    }
}
