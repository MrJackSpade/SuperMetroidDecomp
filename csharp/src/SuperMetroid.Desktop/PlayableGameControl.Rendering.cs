using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    // Host identities deliberately live outside the debugger-state graph.
    private long displaySequence;
    private long displayGeneration;
    private RenderFrameSnapshot? pendingDisplay;
    private D3D11RenderWorker? gpuWorker;
    private bool rendererStarted;
    private bool rendererStopping;
    private long previousPresentCount;
    private long previousPresentTimestamp;
    private double gpuFramesPerSecond;
    private string rendererTimingDetail = string.Empty;

    private void RefreshRendererTimingDetail()
    {
        if (gpuWorker is not { } worker) { rendererTimingDetail = string.Empty; return; }
        var timings = worker.CaptureTimings();
        static string Format(string name, RenderTimingDistribution value) =>
            $"{name}: p50/p95/p99 {value.P50Milliseconds:F3}/{value.P95Milliseconds:F3}/{value.P99Milliseconds:F3} ms; " +
            $"window {value.Retained}, warmup excluded {value.WarmupExcluded}, observed {value.Observed}";
        rendererTimingDetail = Environment.NewLine + string.Join(Environment.NewLine,
            Format("CPU composition submission", timings.CpuComposition),
            Format("CPU upload submission (within composition/display)", worker.CaptureUploadTimings()),
            $"Upload bytes {worker.SubmittedUploadBytes}, calls {worker.SubmittedUploadCalls}",
            Format("CPU display + Present (includes wait)", timings.CpuDisplayAndPresent),
            Format("GPU composition (excludes display/Present)", timings.GpuComposition),
            Format("GPU composition + display (excludes CPU Present wait)", timings.GpuCompositionAndDisplay),
            $"GPU samples skipped {worker.SkippedGpuTimingSamples}, invalid {worker.InvalidGpuTimingSamples}; recoveries {worker.DeviceRecoveries}");
    }
    private readonly System.Windows.Forms.Timer rendererHealthTimer = new() { Interval = 250 };

    private string GpuTimingText => gpuWorker is { } worker
        ? $" | GPU {gpuFramesPerSecond:F1} fps / replaced {worker.MailboxMetrics.Replaced}"
        : string.Empty;

    private void BeginDisplayGeneration()
    {
        displayGeneration = checked(displayGeneration + 1);
        gpuWorker?.AdvanceGeneration(displayGeneration);
        pendingDisplay = null;
    }

    private async Task BeginDisplayGenerationAsync()
    {
        long next = checked(displayGeneration + 1);
        if (gpuWorker is { } worker) await worker.AdvanceGenerationAsync(next);
        displayGeneration = next;
        pendingDisplay = null;
    }

    private async Task RestartAsync()
    {
        bool resume = playbackTimer.Enabled;
        SetPlaying(false);
        Enabled = false;
        try
        {
            await BeginDisplayGenerationAsync();
            if (rendererStopping || IsDisposed) return;
            RestartCore();
            SetPlaying(resume);
        }
        finally { if (!IsDisposed) Enabled = true; }
    }

    /// <summary>
    /// Starts the window-owned renderer with the UI message pump free to service DXGI.
    /// Call once after showing the form. Software remains the migration default.
    /// </summary>
    public Task InitializeRendererAsync() => InitializeRendererAsync(static (window, width, height, generation) =>
        new D3D11RenderWorker(window, width, height, generation, D3D11DeviceKind.Hardware));

    /// <summary>Verification seam for a worker that genuinely fails asynchronous device/window startup.</summary>
    internal async Task InitializeRendererAsync(Func<nint, int, int, long, D3D11RenderWorker> createWorker)
    {
        ArgumentNullException.ThrowIfNull(createWorker);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (rendererStopping) throw new InvalidOperationException("Renderer has already been stopped.");
        if (rendererStarted) throw new InvalidOperationException("Renderer already initialized.");
        rendererStarted = true;
        if (gameOptions.Renderer == RendererSelection.Software) return;
        var worker = createWorker(canvas.Handle,
            Math.Max(1, canvas.ClientSize.Width), Math.Max(1, canvas.ClientSize.Height),
            displayGeneration);
        gpuWorker = worker;
        string adapter;
        try
        {
            adapter = await worker.Ready;
        }
        catch (Exception error)
        {
            // Drain failed creation before letting GDI reclaim this same window.
            try { await worker.StopAsync(); }
            catch (Exception stopError) { Console.Error.WriteLine(stopError); }
            gpuWorker = null;
            canvas.SetGpuOwned(false);
            if (gameOptions.Renderer != RendererSelection.Auto || rendererStopping) throw;
            Console.Error.WriteLine($"Direct3D11 startup failed; Auto selected software rendering.\n{error}");
            RefreshFrame(game.CurrentFrame);
            return;
        }
        if (rendererStopping) return;
        canvas.SetGpuOwned(true);
        canvas.SizeChanged += ResizeGpu;
        ResizeGpu(this, EventArgs.Empty);
        // A publication may already have occurred during asynchronous startup.
        // Fresh identity makes this safe without advancing a paused game. Coverage or
        // runtime failures are outside the Auto startup fallback boundary intentionally.
        pendingDisplay = game.GetRetainedDisplay(++displaySequence, displayGeneration);
        PublishGpuDisplay();
        rendererHealthTimer.Tick += CheckRendererHealth;
        previousPresentTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        rendererHealthTimer.Start();
        Console.WriteLine($"Renderer: Direct3D11 on {adapter}");
    }

    private void ResizeGpu(object? sender, EventArgs e)
    {
        if (!rendererStopping) gpuWorker?.Resize(canvas.ClientSize.Width, canvas.ClientSize.Height);
    }

    private void CheckRendererHealth(object? sender, EventArgs e)
    {
        if (gpuWorker?.Completion.IsFaulted == true)
        {
            rendererHealthTimer.Stop();
            SetPlaying(false);
            gpuWorker.ThrowIfFaulted();
        }
        if (gpuWorker is { } worker)
        {
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            long count = worker.PresentedFrames;
            gpuFramesPerSecond = (count - previousPresentCount) * (double)System.Diagnostics.Stopwatch.Frequency
                / Math.Max(1, now - previousPresentTimestamp);
            previousPresentCount = count;
            previousPresentTimestamp = now;
        }
    }

    private void PublishGpuDisplay()
    {
        if (gpuWorker is not { } worker || rendererStopping) return;
        if (pendingDisplay is not { } packet)
            throw new NotSupportedException("Direct3D11 requires a captured scene; legacy raster fallback is not permitted.");
        worker.Publish(packet);
    }

    private void RefreshDisplay(FrontendFrame frame)
    {
        if (gpuWorker is not null) return;
        var pixels = pendingDisplay is { } packet
            ? SoftwareFrameSnapshotRenderer.Render(packet)
            : frame.Pixels;
        canvas.ReplaceFrame(FrontendFrame.Width, FrontendFrame.Height, pixels);
    }

    /// <summary>Await before destroying the form/child HWND; never synchronously wait on the UI thread.</summary>
    public async Task StopRendererAsync()
    {
        rendererStopping = true;
        SetPlaying(false);
        rendererHealthTimer.Stop();
        canvas.SizeChanged -= ResizeGpu;
        if (gpuWorker is { } worker) await worker.StopAsync();
    }

    private void DisposeRendererHost()
    {
        if (gpuWorker is { Completion.IsCompleted: false })
            throw new InvalidOperationException("Await StopRendererAsync before destroying the GPU window.");
        rendererHealthTimer.Dispose();
    }
}
