using System.Runtime.ExceptionServices;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Single-owner GPU consumer; simulation publishes immutable packets without waiting for GPU work.</summary>
/// <remarks>The HWND must remain alive until StopAsync completes. Await startup/shutdown from the UI;
/// never synchronously block its message pump while DXGI may be interacting with the window.</remarks>
public sealed class D3D11RenderWorker
{
    /// <summary>Latest-frame mailbox shared with publishers; pending visual packets replace older pending packets.</summary>
    private readonly LatestRenderFrameMailbox mailbox;
    /// <summary>Generation boundary that prevents a frame from being presented across load/reset transitions.</summary>
    private readonly RenderPresentationGate gate;
    /// <summary>Signal that wakes the owner thread for publication, resize, or shutdown work.</summary>
    private readonly AutoResetEvent wake = new(false);
    /// <summary>Protects lifecycle state and the pending resize request.</summary>
    private readonly object lifecycle = new();
    /// <summary>Completes when initial device resources are ready or startup fails.</summary>
    private readonly TaskCompletionSource<string> ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Completes when owner-thread resources are released, or faults on an unrecoverable failure.</summary>
    private readonly TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Captured owner-thread failure rethrown by public caller-side operations.</summary>
    private ExceptionDispatchInfo? fault;
    /// <summary>Whether shutdown has been requested or the owner thread has exited.</summary>
    private bool stopping;
    /// <summary>Most recent client-size request waiting for owner-thread processing.</summary>
    private (int Width, int Height)? resize;
    /// <summary>Cumulative successful presentations, occlusion results, and stale-generation results.</summary>
    private long presented, occluded, stale;
    /// <summary>Highest packet sequence consumed by the render owner.</summary>
    private long lastConsumedSequence;
    /// <summary>Number of render attempts that reused retained artwork instead of consuming a new packet.</summary>
    private long retainedRedraws;
    /// <summary>Last client size successfully drawn, packed as width in the high word and height in the low word.</summary>
    private long lastDrawnSize;
    /// <summary>Whether zero-sized client dimensions currently suspend presentation.</summary>
    private bool surfaceSuspended;
    /// <summary>Successful device/resource recreations after initial startup.</summary>
    private long deviceRecoveries;
    /// <summary>Diagnostic details from the most recent device-loss exception.</summary>
    private D3D11DeviceLossDiagnostic? lastDeviceLoss;
    /// <summary>Bounded timing history for CPU-side frame composition and upload submission.</summary>
    private readonly RenderTimingWindow cpuCompositionTiming;
    /// <summary>Bounded timing history for the CPU presentation call.</summary>
    private readonly RenderTimingWindow cpuPresentationTiming;
    /// <summary>Bounded GPU timing history for composition only.</summary>
    private readonly RenderTimingWindow gpuCompositionTiming;
    /// <summary>Bounded GPU timing history for the full composition/display interval.</summary>
    private readonly RenderTimingWindow gpuFrameTiming;
    /// <summary>Bounded timing history for CPU upload calls made while submitting a frame.</summary>
    private readonly RenderTimingWindow cpuUploadTiming;
    /// <summary>Cumulative upload bytes and calls recorded after render/presentation attempts.</summary>
    private long submittedUploadBytes, submittedUploadCalls;
    /// <summary>Thread-safe cumulative UpdateSubresource byte count for completed render/presentation attempts, including composition and display uploads; retained redraws count and this is not GPU memory residency.</summary>
    public long SubmittedUploadBytes => Interlocked.Read(ref submittedUploadBytes);
    /// <summary>Thread-safe cumulative UpdateSubresource call count recorded with completed render/presentation attempts, including retained redraws and continuing across device recreation.</summary>
    public long SubmittedUploadCalls => Interlocked.Read(ref submittedUploadCalls);
    /// <summary>Per-present CPU UpdateSubresource time, including composition and display constants.</summary>
    public RenderTimingDistribution CaptureUploadTimings() => cpuUploadTiming.Snapshot();
    /// <summary>Captures bounded millisecond distributions for CPU composition submission, CPU display/Present, GPU composition, and the complete GPU composition/display interval; GPU results arrive asynchronously and do not include blocking Present time.</summary>
    /// <returns>Independently sampled thread-safe timing histories without waiting for the render owner or GPU.</returns>
    public RenderWorkerTimings CaptureTimings() => new(cpuCompositionTiming.Snapshot(),
        cpuPresentationTiming.Snapshot(), gpuCompositionTiming.Snapshot(), gpuFrameTiming.Snapshot());
    /// <summary>Latest valid asynchronous GPU composition duration, or NaN before a valid sample arrives.</summary>
    private double gpuCompositionMilliseconds = double.NaN;
    /// <summary>Counts of accepted, rejected, and unavailable GPU timestamp samples.</summary>
    private long validGpuTimingSamples, invalidGpuTimingSamples, skippedGpuTimingSamples;
    /// <summary>Thread-safe count of completed timestamp samples rejected for a disjoint/zero-frequency clock or inconsistent ordering; they do not enter the timing distributions.</summary>
    public long InvalidGpuTimingSamples => Interlocked.Read(ref invalidGpuTimingSamples);
    /// <summary>Thread-safe count of render attempts without GPU timestamps because the query ring was full; telemetry is skipped without delaying composition or simulation.</summary>
    public long SkippedGpuTimingSamples => Interlocked.Read(ref skippedGpuTimingSamples);
    /// <summary>Thread-safe count of successful device/resource recreations after startup, excluding failed recovery attempts and the initial device.</summary>
    public long DeviceRecoveries => Interlocked.Read(ref deviceRecoveries);
    /// <summary>Optional verification hook invoked on the owner thread immediately before composition.</summary>
    private readonly Action? beforeRenderForVerification;

    /// <summary>Initial startup task returning the actual device's diagnostic description once renderer/swapchain/timer resources are ready; startup failure faults the task, and later recovery does not replace its result.</summary>
    public Task<string> Ready => ready.Task;
    /// <summary>Worker shutdown/failure task; unrecoverable owner-thread errors fault it rather than escaping the thread or displaying a modal error.</summary>
    public Task Completion => completion.Task;
    /// <summary>Lock-consistent count of pending visual packets replaced before consumption; does not count skipped gameplay updates or audio.</summary>
    public RenderMailboxMetrics MailboxMetrics => mailbox.Metrics;
    /// <summary>Thread-safe count of successful DXGI presentation results, including retained redraws; excludes occlusion/stale-generation attempts and is not a simulation-frame counter.</summary>
    public long PresentedFrames => Interlocked.Read(ref presented);

    /// <summary>Starts a background render-owner thread with a latest-frame mailbox and generation gate; creates GPU resources asynchronously, so the UI must await <see cref="Ready"/> without blocking its message pump.</summary>
    /// <param name="window">Existing HWND kept alive through <see cref="StopAsync"/> completion; window validation/device creation failures are reported asynchronously.</param>
    /// <param name="width">Positive initial client width in pixels, validated by swapchain startup on the worker.</param>
    /// <param name="height">Positive initial client height in pixels.</param>
    /// <param name="generation">Positive load/reset identity shared with every initially published frame, not its sequence number.</param>
    /// <param name="kind">Explicit hardware or WARP backend; startup and recovery never silently change the selection.</param>
    /// <param name="timingCapacity">Positive retained-sample capacity for each timing history, bounded by the shared telemetry limit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Generation or timing capacity is outside its supported range.</exception>
    public D3D11RenderWorker(nint window, int width, int height, long generation, D3D11DeviceKind kind,
        int timingCapacity = RenderTelemetryLimits.DefaultHistoryCapacity)
        : this(window, width, height, generation, kind, null, timingCapacity) { }

    /// <summary>Creates a worker with an optional owner-thread hook used by rendering verification.</summary>
    /// <param name="window">Existing HWND retained until asynchronous shutdown completes.</param>
    /// <param name="width">Initial client width in pixels.</param>
    /// <param name="height">Initial client height in pixels.</param>
    /// <param name="generation">Initial load/reset generation shared with published frames.</param>
    /// <param name="kind">Explicit hardware or WARP renderer selection.</param>
    /// <param name="beforeRenderForVerification">Optional callback invoked immediately before frame composition.</param>
    /// <param name="timingCapacity">Number of recent timing samples retained in each history.</param>
    /// <exception cref="ArgumentOutOfRangeException">Timing capacity is zero or exceeds the shared history limit.</exception>
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

    /// <summary>Publishes one complete immutable display packet and wakes the GPU owner without waiting for rendering; a newer pending packet replaces the old visual packet instead of creating a backlog.</summary>
    /// <param name="frame">Current-generation frame with a strictly increasing sequence, including across load/reset generations; simulation data remains outside the mailbox.</param>
    /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The worker is stopping, the generation differs, or the sequence does not increase; stored worker failures are also rethrown.</exception>
    public void Publish(RenderFrameSnapshot frame)
    {
        lock (lifecycle)
        {
            VerifyRunning(); mailbox.Publish(frame); wake.Set();
        }
    }

    /// <summary>Queues the latest client-size request and wakes the owner without calling DXGI on the caller; either zero dimension suspends presentation, while a positive resize redraws retained current-generation artwork.</summary>
    /// <param name="width">Nonnegative client width in pixels, zero when the surface is unavailable/minimized.</param>
    /// <param name="height">Nonnegative client height in pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is negative.</exception>
    /// <exception cref="InvalidOperationException">The worker is stopping; stored worker failures are also rethrown.</exception>
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

    /// <summary>Async load/reset boundary; false means orderly shutdown canceled the handoff.</summary>
    /// <remarks>Callers suspend stepping until completion and must not restore state after false.
    /// Genuine worker failures still propagate, including failures racing shutdown.</remarks>
    public async Task<bool> AdvanceGenerationAsync(long generation)
    {
        ThrowIfFaulted();
        await gate.AdvanceGenerationAsync(generation).ConfigureAwait(false);
        lock (lifecycle)
        {
            ThrowIfFaulted();
            if (stopping) return false;
            mailbox.AdvanceGeneration(generation);
            wake.Set();
            return true;
        }
    }

    /// <summary>Rethrows any captured unrecoverable worker failure on the caller with its original stack; returns immediately when no failure has been recorded and does not wait for startup or GPU work.</summary>
    public void ThrowIfFaulted() => Volatile.Read(ref fault)?.Throw();

    /// <summary>Idempotently requests owner-thread shutdown and wakes it, returning the shared completion task; await before destroying the HWND and do not block the UI message pump while Present may be in flight.</summary>
    /// <returns>Task completed after render-device resources have been released, or faulted with an unrecoverable worker error; shutdown need not present the pending packet.</returns>
    public Task StopAsync()
    {
        lock (lifecycle)
        {
            if (!stopping) { stopping = true; wake.Set(); }
        }
        return completion.Task;
    }

    /// <summary>Rejects caller operations after shutdown starts and rethrows any stored worker failure.</summary>
    private void VerifyRunning()
    {
        ThrowIfFaulted();
        if (stopping) throw new InvalidOperationException("GPU worker is stopping.");
    }

    /// <summary>Owns the render thread's recovery loop and completes startup/shutdown tasks.</summary>
    /// <param name="window">HWND used to create the swapchain.</param>
    /// <param name="width">Initial client width, updated by resize requests.</param>
    /// <param name="height">Initial client height, updated by resize requests.</param>
    /// <param name="kind">Renderer backend to retain across recovery attempts.</param>
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

    /// <summary>Creates device-scoped resources and consumes frames until shutdown or device loss.</summary>
    /// <param name="window">HWND that owns the swapchain.</param>
    /// <param name="width">Current client width, updated after successful resize.</param>
    /// <param name="height">Current client height, updated after successful resize.</param>
    /// <param name="kind">Explicit backend used for device creation and diagnostics.</param>
    /// <param name="retained">Latest CPU frame kept for redraw after resize or occlusion.</param>
    /// <param name="suspended">Whether zero client dimensions currently prevent presentation.</param>
    /// <param name="consecutiveLosses">Recovery attempts carried across recreated devices.</param>
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
                    Volatile.Write(ref surfaceSuspended, suspended);
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
