using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class GpuTimingTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer, RenderFrameSnapshot frame)
    {
        VerifyTimingWindow();
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

    private static void VerifyTimingWindow()
    {
        var window = new RenderTimingWindow(capacity: 4, warmup: 2);
        var empty = window.Snapshot();
        if (empty.Retained != 0 || !double.IsNaN(empty.P50Milliseconds))
            throw new InvalidOperationException("Empty timing history must not report zero latency.");
        foreach (double value in new double[] { 999, 999, 1, 2, 3, 4, 5, 6 }) window.Record(value);
        var result = window.Snapshot();
        if (result.Observed != 8 || result.WarmupExcluded != 2 || result.Retained != 4 ||
            result.P50Milliseconds != 4 || result.P95Milliseconds != 6 || result.P99Milliseconds != 6)
            throw new InvalidOperationException("Rolling timing warmup/eviction/nearest-rank calculation failed.");
        foreach (double invalid in new[] { double.NaN, double.PositiveInfinity, -1.0 })
        {
            bool rejected = false;
            try { window.Record(invalid); } catch (ArgumentOutOfRangeException) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("Invalid timing sample was accepted.");
        }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) window.Record(i);
        if (GC.GetAllocatedBytesForCurrentThread() != before)
            throw new InvalidOperationException("Timing recording allocated managed memory.");
        var wholeRun = new RenderTimingWindow(RenderTelemetryLimits.MaximumHistoryCapacity, warmup: 0);
        for (int i = 0; i < 18000; i++) wholeRun.Record(i < 1800 ? 10 : 1);
        var whole = wholeRun.Snapshot();
        if (whole.Retained != 18000 || whole.Observed != 18000 || whole.P95Milliseconds != 10)
            throw new InvalidOperationException("Whole-run history lost early slow samples.");
        foreach (int invalid in new[] { 0, RenderTelemetryLimits.MaximumHistoryCapacity + 1 })
        {
            bool rejected = false;
            try { _ = new D3D11RenderWorker(0, 1, 1, 1, D3D11DeviceKind.Hardware, invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            if (!rejected) throw new InvalidOperationException("Unbounded worker timing capacity was accepted.");
        }
    }
}
