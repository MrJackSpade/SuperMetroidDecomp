using System.Diagnostics;
using SuperMetroid.Android;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

/// <summary>Reaches a measured autonomous host frame without changing the demo sequence.</summary>
internal static class AutonomousPerformanceStateFixture
{
    public static int Export(int targetFrame, string destination)
    {
        if (targetFrame is < 1 or > 100000)
            throw new ArgumentOutOfRangeException(nameof(targetFrame));
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root)) throw new IOException($"Fixture directory already exists: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        var output = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        for (int frame = 1; frame <= targetFrame; frame++)
        {
            data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            data.Audio.RenderFrame(result.Frame.AudioCommands);
            // Rendering observes immutable packets and is not needed to advance earlier frames.
            if (frame != targetFrame) continue;
            var packet = result.Snapshot ?? throw new InvalidDataException("Target has no render snapshot.");
            for (int warm = 0; warm < 10; warm++) SoftwareFrameSnapshotRenderer.Render(packet, output);
            long start = Stopwatch.GetTimestamp();
            Rgba32[] pixels = output;
            for (int sample = 0; sample < 100; sample++) pixels = SoftwareFrameSnapshotRenderer.Render(packet, output);
            Console.WriteLine($"Frame {frame}: {result.Frame.GameState}, room {(byte?)data.Game.GameplayActiveAreaIndex:X2}/{data.Game.GameplayActiveRoomIndex:X2}, render={Stopwatch.GetElapsedTime(start).TotalMilliseconds / 100:F3}ms");
            if (packet.Layers is { } layers)
                foreach (var layer in layers.Layers) Console.WriteLine($"Layer: {layer.GetType().Name}");
            PngWriter.WriteRgba(Path.Combine(root, "frame.png"), packet.Width, packet.Height, pixels);
            Console.WriteLine(data.SaveSlot(9));
        }
        return 0;
    }
}
