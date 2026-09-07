using Microsoft.Win32.SafeHandles;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

public enum D3D11PresentationResult { Presented, Occluded, StaleGeneration }

/// <summary>Render-owner flip-model swapchain; window creation and simulation remain host responsibilities.</summary>
public sealed class D3D11SwapchainPresenter : IDisposable
{
    private readonly D3D11RenderDevice owner;
    private readonly IDXGISwapChain2 swapchain;
    private readonly LatencyWaitHandle latency;
    private ID3D11RenderTargetView? target;
    private int width, height;
    private bool disposed;

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

    /// <summary>Draws and presents only if the frame still belongs to the current load/reset generation.</summary>
    public D3D11PresentationResult Present(D3D11FrameRenderer renderer, RenderFrameIdentity identity, RenderPresentationGate gate)
        => Present(renderer, identity, gate, null);

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

    private void CreateTarget()
    {
        using var buffer = swapchain.GetBuffer<ID3D11Texture2D>(0);
        target = owner.Device.CreateRenderTargetView(buffer);
    }
    private void Verify() { owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this); }
    public void Dispose()
    {
        if (disposed) return;
        owner.VerifyOwner(); target?.Dispose(); latency.Dispose(); swapchain.Dispose(); disposed = true;
    }
    private sealed class LatencyWaitHandle : WaitHandle
    {
        internal LatencyWaitHandle(nint handle)
        {
            if (handle == 0) throw new InvalidOperationException("DXGI returned no frame-latency handle.");
            SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true);
        }
    }
}
