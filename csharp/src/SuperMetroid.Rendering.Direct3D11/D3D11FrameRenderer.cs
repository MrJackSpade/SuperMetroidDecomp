using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Integer compute composition from owned display inputs.</summary>
/// <remarks>Unsupported operations fail explicitly. Readback is diagnostic, not the presentation path.</remarks>
public sealed partial class D3D11FrameRenderer : IDisposable
{
    private readonly D3D11RenderDevice owner;
    private readonly ID3D11ComputeShader shader;
    private readonly ID3D11Texture2D output;
    private ID3D11Texture2D? staging;
    private RenderFrameIdentity? renderedIdentity;
    internal RenderFrameIdentity? SubmittedIdentity => renderedIdentity;
    internal D3D11RenderDevice DeviceOwner => owner;
    private readonly ID3D11UnorderedAccessView view;
    private readonly ID3D11ShaderResourceView displaySource;
    private readonly ID3D11VertexShader displayVertexShader;
    private readonly ID3D11PixelShader displayPixelShader;
    private readonly ID3D11Buffer constants;
    private readonly ID3D11ComputeShader tileShader;
    private readonly ID3D11Buffer memoryBuffer;
    private readonly ID3D11ShaderResourceView memoryView;
    private readonly ID3D11Texture2D resolvedObjects;
    private readonly ID3D11UnorderedAccessView objectView;
    private readonly List<IDisposable> resources = [];
    private bool disposed;

    public D3D11FrameRenderer(D3D11RenderDevice owner)
    {
        this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        owner.VerifyOwner();
        try
        {
            shader = Own(LoadShader(D3D11ShaderLayout.SolidResourceName));
            tileShader = Own(LoadShader(D3D11ShaderLayout.TileResourceName));
            output = Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
                SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.UnorderedAccess | BindFlags.ShaderResource)));
            view = Own(owner.Device.CreateUnorderedAccessView(output));
            displaySource = Own(owner.Device.CreateShaderResourceView(output));
            displayVertexShader = Own(owner.Device.CreateVertexShader(LoadShaderBytes(D3D11ShaderLayout.DisplayVertexResourceName)));
            displayPixelShader = Own(owner.Device.CreatePixelShader(LoadShaderBytes(D3D11ShaderLayout.DisplayPixelResourceName)));
            constants = Own(owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.SolidConstantWords * sizeof(uint), BindFlags.ConstantBuffer)));
            memoryBuffer = Own(owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.PpuMemoryWords * sizeof(uint),
                BindFlags.ShaderResource, ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint))));
            memoryView = Own(owner.Device.CreateShaderResourceView(memoryBuffer));
            resolvedObjects = Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt,
                SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.UnorderedAccess)));
            objectView = Own(owner.Device.CreateUnorderedAccessView(resolvedObjects));
        }
        catch { DisposeResources(); throw; }
    }

    /// <summary>Submits GPU composition without staging allocation, readback or a CPU raster.</summary>
    public void Render(RenderFrameSnapshot packet)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(packet);
        renderedIdentity = null;
        RenderCore(packet);
        renderedIdentity = packet.Identity;
    }

    /// <summary>Diagnostic convenience; production presentation must use Render without readback.</summary>
    public Rgba32[] RenderForReadback(RenderFrameSnapshot packet)
    {
        Render(packet);
        return Readback();
    }

    private unsafe void RenderCore(RenderFrameSnapshot packet)
    {
        if (packet.Layers is { } layers) { DrawLayers(packet, layers); return; }
        if (packet.Mode7 is { } mode7)
        {
            var operations = new List<RenderLayer>();
            if (mode7.Background is { } background) operations.Add(new Mode7RenderLayer(background));
            operations.Add(new ObjRenderLayer());
            DrawLayers(packet, new LayeredRenderSnapshot(mode7.Memory,
                operations.ToArray(), mode7.ObjectSelection, mode7.Brightness));
            return;
        }
        Rgba32 color = packet.SolidColor ?? throw new NotSupportedException("This compute path does not yet support the requested composition.");
        if (packet.BrightnessPasses.Length > D3D11ShaderLayout.MaximumBrightnessPasses) throw new ArgumentOutOfRangeException(nameof(packet));
        uint[] data = new uint[D3D11ShaderLayout.SolidConstantWords];
        data[0] = (uint)(color.R | color.G << 8 | color.B << 16 | color.A << 24);
        data[1] = (uint)packet.BrightnessPasses.Length;
        data[2] = (uint)packet.Width; data[3] = (uint)packet.Height;
        for (int i = 0; i < packet.BrightnessPasses.Length; i++) data[D3D11ShaderLayout.SolidHeaderWords + i] = packet.BrightnessPasses[i];
        fixed (uint* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
        owner.Context.CSSetShader(shader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.Dispatch((uint)((packet.Width + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge),
            (uint)((packet.Height + D3D11ShaderLayout.DispatchTileEdge - 1) / D3D11ShaderLayout.DispatchTileEdge), 1);
    }

    /// <summary>Reads the last completed submission for diagnostics; never advances simulation.</summary>
    public unsafe Rgba32[] Readback()
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        if (renderedIdentity is null) throw new InvalidOperationException("No successful frame submission is available for readback.");
        const int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        staging ??= Own(owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
            width, height, 1, 1, BindFlags.None, ResourceUsage.Staging, CpuAccessFlags.Read)));
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.CopyResource(staging, output);
        MappedSubresource mapped = owner.Context.Map(staging, 0, MapMode.Read);
        try
        {
            var pixels = new Rgba32[width * height];
            for (int y = 0; y < height; y++)
            {
                uint* row = (uint*)((byte*)mapped.DataPointer + y * mapped.RowPitch);
                for (int x = 0; x < width; x++)
                {
                    uint packed = row[x];
                    pixels[y * width + x] = new((byte)packed, (byte)(packed >> 8), (byte)(packed >> 16), (byte)(packed >> 24));
                }
            }
            return pixels;
        }
        finally { owner.Context.Unmap(staging, 0); }
    }

    public void Dispose()
    {
        if (disposed) return;
        owner.VerifyOwner();
        owner.Context.CSSetShader(null);
        owner.Context.VSSetShader(null!);
        owner.Context.PSSetShader(null!);
        owner.Context.PSSetShaderResource(0, null!);
        owner.Context.PSSetConstantBuffer(0, null!);
        owner.Context.CSSetConstantBuffer(0, null);
        owner.Context.CSSetShaderResource(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        DisposeResources();
        disposed = true;
    }

    private T Own<T>(T value) where T : IDisposable { resources.Add(value); return value; }
    private void DisposeResources()
    {
        for (int i = resources.Count - 1; i >= 0; i--) resources[i].Dispose();
        resources.Clear();
    }
    private ID3D11ComputeShader LoadShader(string name)
        => owner.Device.CreateComputeShader(LoadShaderBytes(name));

    private static byte[] LoadShaderBytes(string name)
    {
        using Stream resource = typeof(D3D11FrameRenderer).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidDataException($"Build-generated shader {name} is missing.");
        using var bytes = new MemoryStream(); resource.CopyTo(bytes);
        return bytes.ToArray();
    }
}
