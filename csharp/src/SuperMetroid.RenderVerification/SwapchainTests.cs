using System.ComponentModel;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class SwapchainTests
{
    internal static void RunWorker(D3D11RenderDevice selection)
    {
        nint window = CreateWindowExW(0, "STATIC", "Hidden GPU worker verification", 0, 0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        D3D11RenderWorker? worker = null;
        try
        {
            int injectedLoss = 0;
            worker = new(window, 640, 480, 1, selection.Kind, () =>
            {
                int failure = Interlocked.Exchange(ref injectedLoss, 0);
                if (failure != 0) new SharpGen.Runtime.Result(failure).CheckError();
            });
            PumpUntil(() => worker.Ready.IsCompleted);
            string adapter = worker.Ready.GetAwaiter().GetResult();
            if (!adapter.Contains($"backend={selection.Kind}") ||
                !adapter.Contains($"feature level={selection.ActualFeatureLevel}"))
                throw new InvalidOperationException("Worker startup omitted actual backend/feature-level diagnostics.");
            // Allow the worker to observe initial readiness while the mailbox is empty.
            var idle = System.Diagnostics.Stopwatch.StartNew();
            PumpUntil(() => idle.ElapsedMilliseconds >= 40);
            for (int sequence = 1; sequence <= 1000; sequence++)
                worker.Publish(new(new(sequence, 1, 0), new Rgba32(19,73,129)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.LastConsumedSequence == 1000; });
            long previousRedraws = worker.RetainedRedraws;
            worker.Resize(800, 600);
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.RetainedRedraws > previousRedraws && worker.LastDrawnSize == (800, 600); });
            if (worker.MailboxMetrics.Published != 1000 || worker.LastConsumedSequence != 1000)
                throw new InvalidOperationException("Retained redraw consumed or manufactured a simulation frame.");
            previousRedraws = worker.RetainedRedraws;
            Interlocked.Exchange(ref injectedLoss, D3D11RecoveryPolicy.DeviceRemoved);
            worker.Resize(801, 601);
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.DeviceRecoveries == 1 &&
                worker.RetainedRedraws > previousRedraws && worker.LastDrawnSize == (801, 601); });
            if (worker.MailboxMetrics.Published != 1000 || worker.LastConsumedSequence != 1000)
                throw new InvalidOperationException("Device recovery manufactured a simulation frame.");
            var loss = worker.LastDeviceLoss;
            if (loss is null || loss.FailureHResult != D3D11RecoveryPolicy.DeviceRemoved ||
                loss.RemovalReason != 0 || loss.Adapter != selection.AdapterDescription || loss.Backend != selection.Kind ||
                loss.Width != 801 || loss.Height != 601 || loss.Frame?.Sequence != 1000)
                throw new InvalidOperationException("Device-loss diagnostic lost original failure/device/frame context.");
            worker.AdvanceGeneration(2);
            worker.Resize(319, 601);
            Interlocked.Exchange(ref injectedLoss, D3D11RecoveryPolicy.DeviceReset);
            worker.Publish(new(new(1001,2,0), new Rgba32(91,27,173)));
            PumpUntil(() => { worker.ThrowIfFaulted(); return worker.DeviceRecoveries == 2 && worker.LastConsumedSequence == 1001; });
            if (worker.LastDeviceLoss?.FailureHResult != D3D11RecoveryPolicy.DeviceReset ||
                worker.LastDeviceLoss.Frame?.Generation != 2)
                throw new InvalidOperationException("Reset diagnostic retained obsolete frame/failure context.");
            var metrics = worker.MailboxMetrics;
            var timings = worker.CaptureTimings();
            if (timings.CpuComposition.Observed == 0 || timings.CpuDisplayAndPresent.Observed == 0)
                throw new InvalidOperationException("Worker did not record CPU submission/presentation timings.");
            if (metrics.HasPendingFrame || metrics.Published != metrics.Taken + metrics.Replaced + metrics.Invalidated)
                throw new InvalidOperationException("GPU worker lost mailbox accounting.");
            Console.WriteLine($"{selection.Kind}: worker {adapter}; idle-start, 1001 publications, retained redraw, two device-loss recoveries, generation/resize, bounded accounting passed.");
        }
        finally
        {
            if (worker is not null)
            {
                Task stop = worker.StopAsync(); PumpUntil(() => stop.IsCompleted); stop.GetAwaiter().GetResult();
            }
            if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        VerifyRecoveryFailure(selection.Kind, repeatDeviceLoss: true);
        VerifyRecoveryFailure(selection.Kind, repeatDeviceLoss: false);
        VerifySurfaceSuspension(selection.Kind);
    }

    private static void PumpUntil(Func<bool> finished)
    {
        var timeout = System.Diagnostics.Stopwatch.StartNew();
        while (!finished())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException("GPU worker verification timed out.");
            while (PeekMessageW(out var message, 0, 0, 0, 1))
            { TranslateMessage(in message); DispatchMessageW(in message); }
            Thread.Sleep(1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowMessage
    {
        public nint Window; public uint Id; public nuint WParam; public nint LParam;
        public uint Time; public int X, Y; public uint Private;
    }
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PeekMessageW(out WindowMessage message, nint window, uint min, uint max, uint flags);
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TranslateMessage(in WindowMessage message);
    [LibraryImport("user32.dll")]
    private static partial nint DispatchMessageW(in WindowMessage message);

    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Predefined STATIC class, no WS_VISIBLE, no activation or message dialog.
        nint window = CreateWindowExW(0, "STATIC", "Hidden renderer verification", 0,
            0, 0, 640, 480, 0, 0, 0, 0);
        if (window == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            using var presenter = new D3D11SwapchainPresenter(device, window, 640, 480);
            var gate = new RenderPresentationGate(1);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(173,91,27));
            renderer.Render(packet);
            _ = presenter.TryAcquireFrameOpportunity();
            var outcome = presenter.Present(renderer, packet.Identity, gate);
            if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Current frame was rejected.");
            gate.AdvanceGeneration(2);
            if (presenter.Present(renderer, packet.Identity, gate) != D3D11PresentationResult.StaleGeneration)
                throw new InvalidOperationException("Swapchain presented stale generation.");
            bool mismatch = false;
            try { presenter.Present(renderer, new(2,2,0), gate); }
            catch (InvalidOperationException) { mismatch = true; }
            if (!mismatch) throw new InvalidOperationException("Swapchain accepted mismatched GPU frame identity.");
            foreach (var size in new[] { (319,601), (1,1), (800,600) })
            {
                presenter.Resize(size.Item1, size.Item2);
                packet = new(new(packet.Identity.Sequence + 1, 2, 0), new Rgba32(27,91,173));
                renderer.Render(packet);
                outcome = presenter.Present(renderer, packet.Identity, gate);
                if (outcome == D3D11PresentationResult.StaleGeneration) throw new InvalidOperationException("Resized frame was rejected.");
            }
            Console.WriteLine($"{device.Kind}: hidden flip swapchain creation, readiness, resize and generation checks passed; last status {outcome}.");
        }
        finally { if (!DestroyWindow(window)) throw new Win32Exception(Marshal.GetLastWin32Error()); }
    }

    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWindowExW(uint extendedStyle, string className, string windowName,
        uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(nint window);
}
