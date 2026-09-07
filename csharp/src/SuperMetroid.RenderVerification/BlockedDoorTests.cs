using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    internal static void VerifyBlockedDoor(D3D11RenderDevice device, D3D11FrameRenderer renderer,
        SuperMetroidRuntime runtime, ISnesAddressSpace bus, ushort door, ushort destination, string context)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden door backlog verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        D3D11RenderWorker? worker = null;
        try
        {
            worker = new(window, 640, 480, 1, device.Kind, () =>
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(60))) throw new TimeoutException("Door consumer release missing.");
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            worker.Ready.GetAwaiter().GetResult();
            long sequence = 1;
            worker.Publish(new(new(sequence, 1, 0), GameplayDisplayCapture.TryCaptureFrame(runtime)!));
            PumpUntil(() => entered.IsSet);
            var clock = Stopwatch.StartNew();
            RenderFrameSnapshot? latest = null;
            RetailDoorCaptureTests.VerifyTransition(device, renderer, runtime, bus, door, destination, context,
                packet =>
                {
                    latest = new(new(++sequence, 1, packet.Identity.SimulationFrame), packet.Layers!, packet.BrightnessPasses);
                    worker.Publish(latest);
                });
            PumpUntil(() => clock.ElapsedMilliseconds >= 250);
            if (latest is null || worker.LastConsumedSequence != 0 || !worker.MailboxMetrics.HasPendingFrame ||
                worker.MailboxMetrics.Replaced != sequence - 2)
                throw new InvalidOperationException("Door transition failed to advance under the held consumer with a bounded mailbox.");
            release.Set();
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == sequence; });
            if (worker.MailboxMetrics.HasPendingFrame || worker.MailboxMetrics.Taken != 2)
                throw new InvalidOperationException("Resumed door consumer did not skip superseded transition packets.");
            PixelComparison.Verify(latest, SuperMetroidRuntimeFrameRenderer.Render(runtime),
                renderer.RenderForReadback(latest), $"{device.Kind}: retained destination after blocked {context} door");
            Console.WriteLine($"{device.Kind}: blocked {context} door completed; {sequence - 2} replaced frames, latest destination consumed.");
        }
        finally
        {
            release.Set();
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
