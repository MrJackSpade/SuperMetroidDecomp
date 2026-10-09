using Microsoft.Win32.SafeHandles;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Mutually exclusive result of the render owner's final generation-checked swapchain presentation attempt.</summary>
public enum D3D11PresentationResult
{
    /// <summary>DXGI Present returned success; the worker counts the submitted frame as presented, independently of simulation update count.</summary>
    Presented,
    /// <summary>DXGI reported occlusion; the worker retains the immutable frame and retries presentation after a bounded render-thread wait.</summary>
    Occluded,
    /// <summary>The presentation gate rejected the frame's old load/reset generation; DXGI Present was not called even though render/display commands were already submitted.</summary>
    StaleGeneration
}

/// <summary>Render-owner flip-model swapchain; window creation and simulation remain host responsibilities.</summary>
public sealed class D3D11SwapchainPresenter : IDisposable
{
    /// <summary>The render device whose owner thread must perform swapchain operations.</summary>
    private readonly D3D11RenderDevice owner;

    /// <summary>The flip-model DXGI swapchain that presents rendered frames to the host window.</summary>
    private readonly IDXGISwapChain2 swapchain;

    /// <summary>Owns the swapchain's frame-latency handle used to wait for presentation capacity.</summary>
    private readonly LatencyWaitHandle latency;

    /// <summary>Render-target view for the current first swapchain buffer, recreated after resize.</summary>
    private ID3D11RenderTargetView? target;

    /// <summary>Client dimensions used to draw the current frame into the swapchain target.</summary>
    private int width, height;

    /// <summary>Whether disposal has released this presenter's render target, wait handle, and swapchain.</summary>
    private bool disposed;

    /// <summary>Creates a two-buffer BGRA flip-discard swapchain for an existing HWND on its device-owner thread, with a frame-latency waitable object and maximum latency one; owns the swapchain, not the window or device.</summary>
    /// <param name="owner">Live render-owner device shared by the renderer; must outlive the presenter.</param>
    /// <param name="window">Nonzero native HWND kept alive by the host until presentation shutdown completes.</param>
    /// <param name="width">Positive initial client width in pixels; minimized surfaces must be suspended by the host rather than created at zero.</param>
    /// <param name="height">Positive initial client height in pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">A client dimension is zero or negative.</exception>
    /// <exception cref="ArgumentException"><paramref name="window"/> is zero.</exception>
    /// <exception cref="InvalidOperationException">Construction is attempted off the device's owner thread.</exception>
    public D3D11SwapchainPresenter(D3D11RenderDevice owner, nint window, int width, int height)
    {
        this.owner = owner;
        owner.VerifyOwner();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (window == 0) throw new ArgumentException("A real HWND is required.", nameof(window));
        using var factory = DXGI.CreateDXGIFactory1<IDXGIFactory2>();
        using var created = factory.CreateSwapChainForHwnd(owner.Device, window, new SwapChainDescription1
        {
            Width = (uint)width, Height = (uint)height, Format = Format.B8G8R8A8_UNorm,
            SampleDescription = new(1, 0), BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 2, Scaling = Scaling.Stretch, SwapEffect = SwapEffect.FlipDiscard,
            Flags = SwapChainFlags.FrameLatencyWaitableObject
        });
        swapchain = created.QueryInterface<IDXGISwapChain2>();
        try
        {
            swapchain.MaximumFrameLatency = 1;
            latency = new LatencyWaitHandle(swapchain.FrameLatencyWaitableObject);
            this.width = width; this.height = height;
            CreateTarget();
        }
        catch { target?.Dispose(); latency?.Dispose(); swapchain.Dispose(); throw; }
    }

    /// <summary>Nonblocking readiness query. The host waits outside simulation and the generation gate.</summary>
    public bool TryAcquireFrameOpportunity() { Verify(); return latency.WaitOne(0); }

    /// <summary>Draws the display overlay and makes one generation-checked DXGI presentation attempt.</summary>
    /// <param name="renderer">Renderer that submitted the frame and shares this presenter's device owner.</param>
    /// <param name="identity">Identity that must match the submitted frame and pass the presentation gate.</param>
    /// <param name="gate">Final generation gate that controls whether DXGI Present is called.</param>
    /// <param name="timer">Optional GPU timer ended after display commands are submitted and before presentation waits.</param>
    /// <returns>Whether the frame was presented, occluded, or rejected as stale by the generation gate.</returns>
    internal D3D11PresentationResult Present(D3D11FrameRenderer renderer, RenderFrameIdentity identity,
        RenderPresentationGate gate, D3D11GpuTimer? timer)
    {
        Verify();
        if (!ReferenceEquals(renderer.DeviceOwner, owner)) throw new InvalidOperationException("Renderer and swapchain must share a device owner.");
        if (target is null) throw new InvalidOperationException("Swapchain target is unavailable after failed resize.");
        if (renderer.SubmittedIdentity != identity) throw new InvalidOperationException("Presentation identity does not match the rendered frame.");
        renderer.DrawDisplay(target!, width, height);
        // Finish GPU timing after display commands, before the CPU's blocking Present.
        // This remains one timestamp-disjoint interval for the whole rendered frame.
        timer?.End();
        D3D11PresentationResult outcome = D3D11PresentationResult.StaleGeneration;
        gate.TryPresent(identity, () =>
        {
            var result = swapchain.Present(1, PresentFlags.None);
            result.CheckError();
            if (result.Code == DxgiPresentationStatus.Occluded) outcome = D3D11PresentationResult.Occluded;
            else if (result.Code == 0) outcome = D3D11PresentationResult.Presented;
            else throw new InvalidOperationException($"Unexpected DXGI presentation status {result}.");
        });
        return outcome;
    }

    /// <summary>Resizes positive client dimensions; minimized windows must suspend presentation in the host.</summary>
    public void Resize(int nextWidth, int nextHeight)
    {
        Verify();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nextWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nextHeight);
        target?.Dispose(); target = null;
        swapchain.ResizeBuffers(2, (uint)nextWidth, (uint)nextHeight, Format.B8G8R8A8_UNorm,
            SwapChainFlags.FrameLatencyWaitableObject).CheckError();
        width = nextWidth; height = nextHeight;
        CreateTarget();
    }

    /// <summary>Creates a render-target view for buffer zero of the current swapchain.</summary>
    private void CreateTarget()
    {
        using var buffer = swapchain.GetBuffer<ID3D11Texture2D>(0);
        target = owner.Device.CreateRenderTargetView(buffer);
    }
    /// <summary>Requires the device-owner thread and rejects use after disposal.</summary>
    private void Verify() { owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this); }
    /// <summary>Releases the render target, owned latency wait handle, and swapchain on the device-owner thread; leaves the HWND/device to their owners and ignores repeated disposal.</summary>
    /// <exception cref="InvalidOperationException">First disposal is attempted from a different thread.</exception>
    public void Dispose()
    {
        if (disposed) return;
        owner.VerifyOwner(); target?.Dispose(); latency.Dispose(); swapchain.Dispose(); disposed = true;
    }
    /// <summary>Wraps and owns DXGI's frame-latency waitable handle as a managed wait handle.</summary>
    private sealed class LatencyWaitHandle : WaitHandle
    {
        /// <summary>Adopts the native handle so disposal closes the DXGI waitable object.</summary>
        /// <param name="handle">The frame-latency handle returned by the swapchain.</param>
        /// <exception cref="InvalidOperationException">DXGI supplied a null handle.</exception>
        internal LatencyWaitHandle(nint handle)
        {
            if (handle == 0) throw new InvalidOperationException("DXGI returned no frame-latency handle.");
            SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true);
        }
    }
}
