using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    private static void VerifySurfaceSuspension(D3D11DeviceKind kind)
    {
        // Exercise the real worker and swapchain without showing or minimizing the
        // player's windows. Zero-width/height are the host's suspended-surface input.
        nint window = CreateWindowExW(0, "STATIC", "Hidden surface suspension verification",
            0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, 640, 480, 1, kind);
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            long sequence = 1, generation = 1;
            worker.Publish(new(new(sequence, generation, 0), new Rgba32(23, 71, 127)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });

            foreach (var size in new[] { (0, 480), (640, 0), (0, 0) })
            {
                worker.Resize(size.Item1, size.Item2);
                PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
                long consumed = worker.LastConsumedSequence;
                long submissions = worker.SubmittedUploadCalls;
                long presentations = worker.PresentedFrames + worker.OccludedFrames;
                long replacements = worker.MailboxMetrics.Replaced;
                for (int i = 0; i < 32; i++)
                    worker.Publish(new(new(++sequence, generation, 0), new Rgba32(127, 71, 23)));

                // The acknowledgement prevents an arbitrary sleep from masking an
                // unprocessed resize. Wait across multiple actual worker wake cycles.
                var elapsed = Stopwatch.StartNew();
                PumpUntil(() => { worker.ThrowIfFaulted(); return elapsed.ElapsedMilliseconds >= 250; });
                if (worker.LastConsumedSequence != consumed || worker.SubmittedUploadCalls != submissions ||
                    worker.PresentedFrames + worker.OccludedFrames != presentations ||
                    !worker.MailboxMetrics.HasPendingFrame || worker.MailboxMetrics.Replaced != replacements + 31)
                    throw new InvalidOperationException("Suspended surface consumed/rendered frames or lost bounded latest-frame accounting.");

                worker.AdvanceGeneration(++generation);
                if (worker.MailboxMetrics.HasPendingFrame)
                    throw new InvalidOperationException("Generation change retained a queued pre-load frame while suspended.");
                worker.Publish(new(new(++sequence, generation, 0), new Rgba32(43, 91, 173)));
                worker.Resize(641, 481);
                PumpUntil(() => { worker.ThrowIfFaulted(); return !worker.IsSurfaceSuspended &&
                    worker.LastConsumedSequence == sequence && worker.LastDrawnSize == (641, 481); });
                var metrics = worker.MailboxMetrics;
                if (metrics.HasPendingFrame || metrics.Generation != generation ||
                    metrics.Published != metrics.Taken + metrics.Replaced + metrics.Invalidated)
                    throw new InvalidOperationException("Surface restore lost latest generation/mailbox accounting.");
            }

            worker.Resize(0, 0);
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.IsSurfaceSuspended; });
            worker.Publish(new(new(++sequence, generation, 0), new Rgba32(1, 2, 3)));
            Task stop = worker.StopAsync();
            PumpUntil(() => stop.IsCompleted);
            stop.GetAwaiter().GetResult();
            Console.WriteLine($"{kind}: zero width/height suspension, bounded publication, generation invalidation, latest-frame restore and suspended shutdown passed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync();
                PumpUntil(() => stop.IsCompleted);
                stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
