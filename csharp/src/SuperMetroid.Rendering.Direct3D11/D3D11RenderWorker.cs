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

    public Task<string> Ready => ready.Task;
    public Task Completion => completion.Task;
    public RenderMailboxMetrics MailboxMetrics => mailbox.Metrics;
    public long PresentedFrames => Interlocked.Read(ref presented);
    public long OccludedFrames => Interlocked.Read(ref occluded);
    public long StaleFrames => Interlocked.Read(ref stale);
    public long LastConsumedSequence => Interlocked.Read(ref lastConsumedSequence);

    public D3D11RenderWorker(nint window, int width, int height, long generation, D3D11DeviceKind kind)
    {
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
            using var device = new D3D11RenderDevice(kind);
            using var renderer = new D3D11FrameRenderer(device);
            using var presenter = new D3D11SwapchainPresenter(device, window, width, height);
            ready.SetResult(device.AdapterDescription);
            bool suspended = false;
            bool opportunity = false, wasOccluded = false;
            while (true)
            {
                (int Width, int Height)? nextSize;
                lock (lifecycle)
                {
                    if (stopping) break;
                    nextSize = resize; resize = null;
                }
                if (nextSize is { } size)
                {
                    suspended = size.Width == 0 || size.Height == 0;
                    if (!suspended) presenter.Resize(size.Width, size.Height);
                }
                opportunity |= !suspended && (wasOccluded || presenter.TryAcquireFrameOpportunity());
                if (!suspended && opportunity)
                {
                    var packet = mailbox.TakeLatest();
                    if (packet is not null)
                    {
                        renderer.Render(packet);
                        switch (presenter.Present(renderer, packet.Identity, gate))
                        {
                            case D3D11PresentationResult.Presented:
                                opportunity = false; wasOccluded = false; Interlocked.Increment(ref presented); break;
                            case D3D11PresentationResult.Occluded:
                                opportunity = false; wasOccluded = true; Interlocked.Increment(ref occluded); break;
                            case D3D11PresentationResult.StaleGeneration: Interlocked.Increment(ref stale); break;
                        }
                        Interlocked.Exchange(ref lastConsumedSequence, packet.Identity.Sequence);
                    }
                }
                // A wake expedites publication/resize/shutdown. This bounded wait is
                // exclusively on the render owner, never on simulation or audio.
                wake.WaitOne(wasOccluded ? 100 : 8);
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
}
