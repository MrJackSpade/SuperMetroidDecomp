using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Explicit device selection; hardware requests never silently become WARP.</summary>
public enum D3D11DeviceKind { Hardware, Warp }

/// <summary>Render-thread-owned D3D11 device/context lifetime, isolated from portable display contracts.</summary>
public sealed class D3D11RenderDevice : IDisposable
{
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    private bool disposed;
    internal ID3D11Device Device { get; }
    internal ID3D11DeviceContext Context { get; }
    public string AdapterDescription { get; }
    public D3D11DeviceKind Kind { get; }
    /// <summary>Actual feature level returned by the created device, not an inferred adapter capability.</summary>
    public FeatureLevel ActualFeatureLevel { get; }
    public string DiagnosticDescription => $"{AdapterDescription}; backend={Kind}; feature level={ActualFeatureLevel}";

    public D3D11RenderDevice(D3D11DeviceKind kind)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Kind = kind;
        using IDXGIAdapter1? selectedAdapter = kind == D3D11DeviceKind.Hardware ? FindHardwareAdapter() : null;
        D3D11.D3D11CreateDevice(selectedAdapter, kind == D3D11DeviceKind.Warp ? DriverType.Warp : DriverType.Unknown,
            DeviceCreationFlags.BgraSupport, new[] { FeatureLevel.Level_11_0 },
            out ID3D11Device device, out ID3D11DeviceContext context).CheckError();
        Device = device; Context = context;
        try
        {
            ActualFeatureLevel = device.FeatureLevel;
            using IDXGIDevice dxgi = device.QueryInterface<IDXGIDevice>();
            using IDXGIAdapter adapter = dxgi.GetAdapter();
            AdapterDescription = adapter.Description.Description;
        }
        catch { context.Dispose(); device.Dispose(); throw; }
    }

    private static IDXGIAdapter1 FindHardwareAdapter()
    {
        using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();
        for (uint index = 0; factory.EnumAdapters1(index, out IDXGIAdapter1? adapter).Success; index++)
        {
            if ((adapter.Description1.Flags & AdapterFlags.Software) == 0) return adapter;
            adapter.Dispose();
        }
        throw new NotSupportedException("DXGI exposes no hardware adapter; explicitly select WARP for software-driver diagnostics.");
    }

    internal void VerifyOwner()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Environment.CurrentManagedThreadId != ownerThread)
            throw new InvalidOperationException("D3D11 immediate context must remain on its render-owner thread.");
    }

    public void Dispose()
    {
        if (disposed) return;
        VerifyOwner();
        Context.ClearState(); Context.Flush();
        Context.Dispose(); Device.Dispose(); disposed = true;
    }
}
