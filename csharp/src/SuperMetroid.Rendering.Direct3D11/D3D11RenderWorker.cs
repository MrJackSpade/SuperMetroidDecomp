using System.Runtime.ExceptionServices;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Single-owner GPU consumer; simulation publishes immutable packets without waiting for GPU work.</summary>
/// <remarks>The HWND must remain alive until StopAsync completes. Await startup/shutdown from the UI;
/// never synchronously block its message pump while DXGI may be interacting with the window.</remarks>
public sealed class D3D11RenderWorker
{
    private readonly LatestRenderFrameMailbox mailbox;
    private readonly RenderPresentationGate gate;
    private readonly AutoResetEvent wake = new(false);
    private readonly object lifecycle = new();
    private readonly TaskCompletionSource<string> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ExceptionDispatchInfo? fault;
    private bool stopping;
    private (int Width, int Height)? resize;
    private long presented, occluded, stale;
    private long lastConsumedSequence;
    private long retainedRedraws;
    private long lastDrawnSize;
    private long deviceRecoveries;
    private D3D11DeviceLossDiagnostic? lastDeviceLoss;
    public D3D11DeviceLossDiagnostic? LastDeviceLoss => Volatile.Read(ref lastDeviceLoss);
    private readonly RenderTimingWindow cpuCompositionTiming;
    private readonly RenderTimingWindow cpuPresentationTiming;
    private readonly RenderTimingWindow gpuCompositionTiming;
    private readonly RenderTimingWindow gpuFrameTiming;
    private readonly RenderTimingWindow cpuUploadTiming;
    private long submittedUploadBytes, submittedUploadCalls;
    public long SubmittedUploadBytes => Interlocked.Read(ref submittedUploadBytes);
    public long SubmittedUploadCalls => Interlocked.Read(ref submittedUploadCalls);
    /// <summary>Per-present CPU UpdateSubresource time, including composition and display constants.</summary>
    public RenderTimingDistribution CaptureUploadTimings() => cpuUploadTiming.Snapshot();
    public RenderWorkerTimings CaptureTimings() => new(cpuCompositionTiming.Snapshot(),
        cpuPresentationTiming.Snapshot(), gpuCompositionTiming.Snapshot(), gpuFrameTiming.Snapshot());
    private double gpuCompositionMilliseconds = double.NaN;
    private long validGpuTimingSamples, invalidGpuTimingSamples, skippedGpuTimingSamples;
    /// <summary>Latest asynchronous GPU composition duration; excludes display scaling/Present.</summary>
    public double GpuCompositionMilliseconds => Volatile.Read(ref gpuCompositionMilliseconds);
    public long ValidGpuTimingSamples => Interlocked.Read(ref validGpuTimingSamples);
    public long InvalidGpuTimingSamples => Interlocked.Read(ref invalidGpuTimingSamples);
    public long SkippedGpuTimingSamples => Interlocked.Read(ref skippedGpuTimingSamples);
    public long DeviceRecoveries => Interlocked.Read(ref deviceRecoveries);
    private readonly Action? beforeRenderForVerification;

    public Task<string> Ready => ready.Task;
    public Task Completion => completion.Task;
    public RenderMailboxMetrics MailboxMetrics => mailbox.Metrics;
    public long PresentedFrames => Interlocked.Read(ref presented);
    public long OccludedFrames => Interlocked.Read(ref occluded);
    public long StaleFrames => Interlocked.Read(ref stale);
    public long LastConsumedSequence => Interlocked.Read(ref lastConsumedSequence);
    public long RetainedRedraws => Interlocked.Read(ref retainedRedraws);
    internal (int Width, int Height) LastDrawnSize
    {
        get { long size = Interlocked.Read(ref lastDrawnSize); return ((int)(size >> 32), (int)size); }
    }

    public D3D11RenderWorker(nint window, int width, int height, long generation, D3D11DeviceKind kind,
        int timingCapacity = RenderTelemetryLimits.DefaultHistoryCapacity)
        : this(window, width, height, generation, kind, null, timingCapacity) { }

    internal D3D11RenderWorker(nint window, int width, int height, long generation, D3D11DeviceKind kind,
        Action? beforeRenderForVerification, int timingCapacity = RenderTelemetryLimits.DefaultHistoryCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timingCapacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timingCapacity, RenderTelemetryLimits.MaximumHistoryCapacity);
        cpuCompositionTiming = new(timingCapacity);
        cpuPresentationTiming = new(timingCapacity);
        gpuCompositionTiming = new(timingCapacity);
        gpuFrameTiming = new(timingCapacity);
        cpuUploadTiming = new(timingCapacity);
        this.beforeRenderForVerification = beforeRenderForVerification;
        mailbox = new(generation); gate = new(generation);
        var thread = new Thread(() => Run(window, width, height, kind))
        { IsBackground = true, Name = "Super Metroid GPU owner" };
        thread.Start();
    }

    public void Publish(RenderFrameSnapshot frame)
    {
        lock (lifecycle)
        {
            VerifyRunning(); mailbox.Publish(frame); wake.Set();
        }
    }

    public void Resize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        lock (lifecycle) { VerifyRunning(); resize = (width, height); wake.Set(); }
    }

    /// <summary>Called only at the simulation load/reset boundary, before new-generation publication.</summary>
    public void AdvanceGeneration(long generation)
    {
        // Do not hold the publication lock while an in-progress Present completes.
        ThrowIfFaulted(); gate.AdvanceGeneration(generation);
        lock (lifecycle) { VerifyRunning(); mailbox.AdvanceGeneration(generation); wake.Set(); }
    }

    /// <summary>Async load/reset boundary; callers must suspend stepping until it completes.</summary>
    public async Task AdvanceGenerationAsync(long generation)
    {
        ThrowIfFaulted();
        await gate.AdvanceGenerationAsync(generation).ConfigureAwait(false);
        lock (lifecycle) { VerifyRunning(); mailbox.AdvanceGeneration(generation); wake.Set(); }
    }

    public void ThrowIfFaulted() => Volatile.Read(ref fault)?.Throw();

    public Task StopAsync()
    {
        lock (lifecycle)
        {
            if (!stopping) { stopping = true; wake.Set(); }
        }
        return completion.Task;
    }

    private void VerifyRunning()
    {
        ThrowIfFaulted();
        if (stopping) throw new InvalidOperationException("GPU worker is stopping.");
    }

    private void Run(nint window, int width, int height, D3D11DeviceKind kind)
    {
        try
        {
            RenderFrameSnapshot? retained = null;
            bool suspended = false;
            int consecutiveLosses = 0;
            while (true)
            {
                // Complete startup even if Stop raced thread entry, so a UI awaiting
                // Ready is never stranded while simultaneously awaiting shutdown.
                lock (lifecycle) { if (stopping && ready.Task.IsCompleted) break; }
                try
                {
                    RunDevice(window, ref width, ref height, kind, ref retained, ref suspended, ref consecutiveLosses);
                    break;
                }
                catch (Exception error) when (ready.Task.IsCompletedSuccessfully &&
                    D3D11RecoveryPolicy.IsDeviceLoss(error.HResult) &&
                    consecutiveLosses++ < D3D11RecoveryPolicy.MaximumConsecutiveRecreations)
                {
                    // RunDevice's using scopes release every old-device reference before
                    // recreating the HWND swapchain. Keep only immutable CPU display data.
                    Console.Error.WriteLine($"GPU device lost; recreating resources (attempt {consecutiveLosses}): {error}");
                }
            }
        }
        catch (Exception exception)
        {
            Volatile.Write(ref fault, ExceptionDispatchInfo.Capture(exception));
            ready.TrySetException(exception);
            completion.TrySetException(exception);
        }
        finally
        {
            lock (lifecycle) { stopping = true; wake.Dispose(); }
            completion.TrySetResult();
        }
    }

    private void RunDevice(nint window, ref int width, ref int height, D3D11DeviceKind kind,
        ref RenderFrameSnapshot? retained, ref bool suspended, ref int consecutiveLosses)
    {
            using var device = new D3D11RenderDevice(kind);
            try
            {
            using var renderer = new D3D11FrameRenderer(device);
            using var presenter = new D3D11SwapchainPresenter(device, window, width, height);
            using var gpuTimer = new D3D11GpuTimer(device);
            if (!ready.TrySetResult(device.DiagnosticDescription))
            {
                Interlocked.Increment(ref deviceRecoveries);
                Console.WriteLine($"Renderer recovered: {device.DiagnosticDescription}");
            }
            bool opportunity = false, wasOccluded = false;
            bool redraw = retained is not null;
            while (true)
            {
                while (gpuTimer.TryRead(out GpuTimingSample measurement))
                {
                    if (measurement.Valid)
                    {
                        Volatile.Write(ref gpuCompositionMilliseconds, measurement.CompositionMilliseconds);
                        gpuCompositionTiming.Record(measurement.CompositionMilliseconds);
                        gpuFrameTiming.Record(measurement.Milliseconds);
                        Interlocked.Increment(ref validGpuTimingSamples);
                    }
                    else Interlocked.Increment(ref invalidGpuTimingSamples);
                }
                (int Width, int Height)? nextSize;
                lock (lifecycle)
                {
                    if (stopping) break;
                    nextSize = resize; resize = null;
                }
                if (nextSize is { } size)
                {
                    suspended = size.Width == 0 || size.Height == 0;
                    if (!suspended)
                    {
                        width = size.Width; height = size.Height;
                        presenter.Resize(width, height);
                        redraw = true;
                    }
                }
                opportunity |= !suspended && (wasOccluded || presenter.TryAcquireFrameOpportunity());
                if (!suspended && opportunity)
                {
                    var packet = mailbox.TakeLatest();
                    bool newlyTaken = packet is not null;
                    if (retained is not null && !mailbox.IsCurrent(retained)) retained = null;
                    packet ??= redraw || wasOccluded ? retained : null;
                    if (packet is not null)
                    {
                        // Preserve a just-taken packet even if submission loses the device.
                        // A newer mailbox packet or generation always supersedes it on retry.
                        retained = packet;
                        beforeRenderForVerification?.Invoke();
                        bool timed = gpuTimer.TryBegin(packet.Identity);
                        if (!timed) Interlocked.Increment(ref skippedGpuTimingSamples);
                        long submissionStarted = System.Diagnostics.Stopwatch.GetTimestamp();
                        var uploadBefore = renderer.UploadStatistics;
                        renderer.Render(packet);
                        cpuCompositionTiming.Record(System.Diagnostics.Stopwatch.GetElapsedTime(submissionStarted).TotalMilliseconds);
                        if (timed) gpuTimer.MarkCompositionFinished();
                        long presentationStarted = System.Diagnostics.Stopwatch.GetTimestamp();
                        var presentationResult = presenter.Present(renderer, packet.Identity, gate, timed ? gpuTimer : null);
                        cpuPresentationTiming.Record(System.Diagnostics.Stopwatch.GetElapsedTime(presentationStarted).TotalMilliseconds);
                        var uploadAfter = renderer.UploadStatistics;
                        cpuUploadTiming.Record(uploadAfter.CpuMilliseconds - uploadBefore.CpuMilliseconds);
                        Interlocked.Add(ref submittedUploadBytes, uploadAfter.Bytes - uploadBefore.Bytes);
                        Interlocked.Add(ref submittedUploadCalls, uploadAfter.Calls - uploadBefore.Calls);
                        switch (presentationResult)
                        {
                            case D3D11PresentationResult.Presented:
                                opportunity = false; wasOccluded = false; Interlocked.Increment(ref presented); break;
                            case D3D11PresentationResult.Occluded:
                                opportunity = false; wasOccluded = true; Interlocked.Increment(ref occluded); break;
                            case D3D11PresentationResult.StaleGeneration: Interlocked.Increment(ref stale); break;
                        }
                        retained = packet;
                        consecutiveLosses = 0;
                        Interlocked.Exchange(ref lastDrawnSize, ((long)width << 32) | (uint)height);
                        redraw = false;
                        if (newlyTaken || packet.Identity.Sequence > Interlocked.Read(ref lastConsumedSequence))
                            Interlocked.Exchange(ref lastConsumedSequence, packet.Identity.Sequence);
                        else Interlocked.Increment(ref retainedRedraws);
                    }
                }
                // A wake expedites publication/resize/shutdown. This bounded wait is
                // exclusively on the render owner, never on simulation or audio.
                wake.WaitOne(wasOccluded ? 100 : 8);
            }
            }
            catch (Exception error) when (D3D11RecoveryPolicy.IsDeviceLoss(error.HResult))
            {
                // Query the still-live device, not its replacement. A synthetic test
                // HRESULT correctly reports S_OK here; do not invent a removal cause.
                var diagnostic = new D3D11DeviceLossDiagnostic(error.HResult,
                    device.Device.DeviceRemovedReason.Code, device.AdapterDescription,
                    kind, width, height, retained?.Identity);
                Volatile.Write(ref lastDeviceLoss, diagnostic);
                Console.Error.WriteLine(diagnostic);
                throw; // Preserve the HRESULT used by bounded recovery and strict failure.
            }
    }
}
