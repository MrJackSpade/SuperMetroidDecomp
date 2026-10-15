using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11.Device;

/// <summary>Render-thread-owned D3D11 device/context lifetime, isolated from portable display contracts.</summary>
public sealed class D3D11RenderDevice : IDisposable
{
    /// <summary>Managed thread allowed to use the immediate context and perform first disposal.</summary>
    private readonly int ownerThread = Environment.CurrentManagedThreadId;
    /// <summary>Prevents repeated GPU calls after context and device release.</summary>
    private bool disposed;
    /// <summary>D3D11 device retained for render-thread resource creation.</summary>
    internal ID3D11Device Device { get; }
    /// <summary>Immediate context that must remain on the creating render thread.</summary>
    internal ID3D11DeviceContext Context { get; }
    /// <summary>Description queried from the adapter actually associated with the successfully created device.</summary>
    public string AdapterDescription { get; }
    /// <summary>Explicit hardware or WARP backend selected at construction; device creation never silently changes it.</summary>
    public D3D11DeviceKind Kind { get; }
    /// <summary>Actual feature level returned by the created device, not an inferred adapter capability.</summary>
    public FeatureLevel ActualFeatureLevel { get; }
    /// <summary>Adapter, selected backend, and actual feature level formatted for worker startup/recovery reporting without further GPU calls.</summary>
    public string DiagnosticDescription => $"{AdapterDescription}; backend={Kind}; feature level={ActualFeatureLevel}";

    /// <summary>Creates a BGRA-capable feature-level-11 device and immediate context on the calling render-owner thread; resource use and first disposal must remain on that thread.</summary>
    /// <param name="kind">Explicit hardware-adapter or WARP selection; only the defined enum values are accepted.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is not a defined device backend.</exception>
    /// <exception cref="NotSupportedException">Hardware was requested but DXGI exposes no non-software adapter.</exception>
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

    /// <summary>Selects the first non-software DXGI adapter and rejects an absent hardware device.</summary>
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

    /// <summary>Rejects use after disposal or from a thread other than the immediate-context owner.</summary>
    internal void VerifyOwner()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Environment.CurrentManagedThreadId != ownerThread)
            throw new InvalidOperationException("D3D11 immediate context must remain on its render-owner thread.");
    }

    /// <summary>Clears and flushes the immediate context, then releases context/device references on their owner thread; repeated disposal is harmless and does not perform a GPU-completion wait.</summary>
    /// <exception cref="InvalidOperationException">First disposal is attempted from a different thread.</exception>
    public void Dispose()
    {
        if (disposed) return;
        VerifyOwner();
        Context.ClearState(); Context.Flush();
        Context.Dispose(); Device.Dispose(); disposed = true;
    }
}
