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

    /// <summary>
    /// Starts the window-owned renderer with the UI message pump free to service DXGI.
    /// Call once after showing the form. Software remains the migration default.
    /// </summary>
    public async Task InitializeRendererAsync()
    {
        if (rendererStarted) throw new InvalidOperationException("Renderer already initialized.");
        rendererStarted = true;
        if (gameOptions.Renderer == RendererSelection.Software) return;
        var worker = new D3D11RenderWorker(canvas.Handle,
            Math.Max(1, canvas.ClientSize.Width), Math.Max(1, canvas.ClientSize.Height),
            displayGeneration, D3D11DeviceKind.Hardware);
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
