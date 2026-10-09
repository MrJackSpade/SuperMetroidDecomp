using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    // Host identities deliberately live outside the debugger-state graph.
    /// <summary>Monotonic identity assigned to each retained display snapshot.</summary>
    private long displaySequence;
    /// <summary>Generation token used to reject display work captured before a restart or renderer transition.</summary>
    private long displayGeneration;
    /// <summary>Latest immutable display packet awaiting publication to the selected renderer.</summary>
    private RenderFrameSnapshot? pendingDisplay;
    /// <summary>Background Direct3D worker when hardware rendering is active.</summary>
    private D3D11RenderWorker? gpuWorker;
    /// <summary>Records that renderer initialization has been attempted and cannot be repeated.</summary>
    private bool rendererStarted;
    /// <summary>Signals that shutdown has begun so late startup and resize continuations stop publishing work.</summary>
    private bool rendererStopping;
    /// <summary>Form whose resize event is observed while the GPU worker owns presentation.</summary>
    private Form? rendererHost;
    /// <summary>Presented-frame count sampled by the last health-timer tick.</summary>
    private long previousPresentCount;
    /// <summary>Timestamp paired with <see cref="previousPresentCount"/> for the displayed GPU rate.</summary>
    private long previousPresentTimestamp;
    /// <summary>GPU presentation rate calculated from consecutive health-timer samples.</summary>
    private double gpuFramesPerSecond;
    /// <summary>Formatted timing distributions and upload/recovery counters shown by renderer diagnostics.</summary>
    private string rendererTimingDetail = string.Empty;

    /// <summary>Refreshes the diagnostic text from the worker's retained CPU/GPU timing distributions and counters.</summary>
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
    /// <summary>Timer that samples renderer health and presentation rate on the UI message pump.</summary>
    private readonly System.Windows.Forms.Timer rendererHealthTimer = new() { Interval = 250 };

    /// <summary>Compact GPU status suffix showing measured presentation rate and replaced mailbox packets.</summary>
    private string GpuTimingText => gpuWorker is { } worker
        ? $" | GPU {gpuFramesPerSecond:F1} fps / replaced {worker.MailboxMetrics.Replaced}"
        : string.Empty;

    /// <summary>Advances the display epoch and discards any snapshot belonging to the prior epoch.</summary>
    private void BeginDisplayGeneration()
    {
        displayGeneration = checked(displayGeneration + 1);
        gpuWorker?.AdvanceGeneration(displayGeneration);
        pendingDisplay = null;
    }

    /// <summary>Waits for the GPU worker to cross into a new display epoch before accepting new snapshots.</summary>
    /// <returns>False when the worker rejects the barrier or shutdown/disposal prevents installing the epoch.</returns>
    private async Task<bool> BeginDisplayGenerationAsync()
    {
        long next = checked(displayGeneration + 1);
        if (gpuWorker is { } worker && !await worker.AdvanceGenerationAsync(next)) return false;
        if (rendererStopping || IsDisposed) return false;
        displayGeneration = next;
        pendingDisplay = null;
        return true;
    }

    /// <summary>Stops playback, invalidates old display work, restarts game state, then restores the prior play mode.</summary>
    private async Task RestartAsync()
    {
        bool resume = playbackTimer.Enabled;
        SetPlaying(false);
        Enabled = false;
        try
        {
            if (!await BeginDisplayGenerationAsync()) return;
            if (rendererStopping || IsDisposed) return;
            RestartCore();
            SetPlaying(resume);
        }
        finally { if (!IsDisposed) Enabled = true; }
    }

    /// <summary>
    /// Starts the window-owned renderer with the UI message pump free to service DXGI.
    /// Call once after showing the form. Auto prefers hardware with logged startup fallback.
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
        rendererHost = FindForm();
        if (rendererHost is not null) rendererHost.Resize += ResizeGpu;
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

    /// <summary>Resizes GPU presentation to the canvas client area, suspending it while the owning form is minimized.</summary>
    /// <param name="sender">Event source for a canvas or host-form resize.</param>
    /// <param name="e">Resize event data.</param>
    private void ResizeGpu(object? sender, EventArgs e)
    {
        if (rendererStopping) return;
        // Minimization may retain the child canvas dimensions. The form's state,
        // not just its child SizeChanged event, owns presentation suspension.
        bool minimized = rendererHost?.WindowState == FormWindowState.Minimized;
        gpuWorker?.Resize(minimized ? 0 : canvas.ClientSize.Width, minimized ? 0 : canvas.ClientSize.Height);
    }

    /// <summary>Surfaces worker faults and updates the displayed presentation rate from successive present counts.</summary>
    /// <param name="sender">Health timer that triggered the sample.</param>
    /// <param name="e">Timer event data.</param>
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

    /// <summary>Publishes the pending captured scene to the GPU worker when hardware rendering is active.</summary>
    private void PublishGpuDisplay()
    {
        if (gpuWorker is not { } worker || rendererStopping) return;
        if (pendingDisplay is not { } packet)
            throw new NotSupportedException("Direct3D11 requires a captured scene; legacy raster fallback is not permitted.");
        worker.Publish(packet);
    }

    /// <summary>Updates the software canvas from the retained scene packet or the legacy frame pixel buffer.</summary>
    /// <param name="frame">Current frontend frame used only when no captured display packet is available.</param>
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
        if (rendererHost is not null) rendererHost.Resize -= ResizeGpu;
        rendererHost = null;
        if (gpuWorker is { } worker) await worker.StopAsync();
    }

    /// <summary>Releases the health timer after GPU shutdown has completed and rejects premature window teardown.</summary>
    private void DisposeRendererHost()
    {
        if (gpuWorker is { Completion.IsCompleted: false })
            throw new InvalidOperationException("Await StopRendererAsync before destroying the GPU window.");
        rendererHealthTimer.Dispose();
    }
}
