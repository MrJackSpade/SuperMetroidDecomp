using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class GpuTimingTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer, RenderFrameSnapshot frame)
    {
        using var timer = new D3D11GpuTimer(device, capacity: 2);
        if (timer.TryRead(out _)) throw new InvalidOperationException("Empty timer produced a measurement.");
        var samples = new List<double>();
        for (int batch = 0; batch < 8; batch++)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!timer.TryBegin(new(batch * 2 + i + 1, 1, 0))) throw new InvalidOperationException("Free timer slot refused.");
                renderer.Render(frame);
                timer.End();
            }
            if (timer.TryBegin(new(100, 1, 0)) || timer.PendingSamples != 2 || timer.SkippedSamples != batch + 1)
                throw new InvalidOperationException("Full timestamp ring did not bound/record skipped telemetry.");
            // Only this diagnostic waits via readback. Production TryRead uses DoNotFlush
            // and returns immediately if the GPU has not finished the oldest sample.
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.Readback(), "timed GPU composition");
            for (int i = 0; i < 2; i++)
            {
                if (!timer.TryRead(out var sample) || sample.Identity.Sequence != batch * 2 + i + 1)
                    throw new InvalidOperationException("Completed GPU timer lost FIFO frame identity.");
                if (!sample.Valid || !double.IsFinite(sample.Milliseconds) || sample.Milliseconds < 0)
                    throw new InvalidOperationException("Timing fixture returned an invalid GPU clock sample.");
                samples.Add(sample.Milliseconds);
            }
        }
        samples.Sort();
        Console.WriteLine($"{device.Kind}: bounded timestamp ring/parity passed; 16 diagnostic composition samples median={samples[8]:F3}ms max={samples[^1]:F3}ms (not a paced soak).");
    }
}
