using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Initial integer compute path for solid display packets and ordered brightness passes.</summary>
/// <remarks>Readback is diagnostic only. This does not implement scene layers or presentation yet.</remarks>
public sealed class D3D11SolidRenderer : IDisposable
{
    private readonly D3D11RenderDevice owner;
    private readonly ID3D11ComputeShader shader;
    private readonly ID3D11Texture2D output;
    private readonly ID3D11Texture2D staging;
    private readonly ID3D11UnorderedAccessView view;
    private readonly ID3D11Buffer constants;
    private bool disposed;

    public D3D11SolidRenderer(D3D11RenderDevice owner)
    {
        this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
        owner.VerifyOwner();
        using Stream resource = typeof(D3D11SolidRenderer).Assembly.GetManifestResourceStream(D3D11ShaderLayout.SolidResourceName)
            ?? throw new InvalidDataException("The build-generated solid shader is missing.");
        using var bytes = new MemoryStream(); resource.CopyTo(bytes);
        shader = owner.Device.CreateComputeShader(bytes.ToArray());
        try
        {
            output = owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
                SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.UnorderedAccess));
            try
            {
                staging = owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
                    SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels, 1, 1, BindFlags.None,
                    ResourceUsage.Staging, CpuAccessFlags.Read));
                try
                {
                    view = owner.Device.CreateUnorderedAccessView(output);
                    try { constants = owner.Device.CreateBuffer(new BufferDescription(D3D11ShaderLayout.SolidConstantWords * sizeof(uint), BindFlags.ConstantBuffer)); }
                    catch { view.Dispose(); throw; }
                }
                catch { staging.Dispose(); throw; }
            }
            catch { output.Dispose(); throw; }
        }
        catch { shader.Dispose(); throw; }
    }

    public unsafe Rgba32[] RenderForReadback(RenderFrameSnapshot packet)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentNullException.ThrowIfNull(packet);
        Rgba32 color = packet.SolidColor ?? throw new NotSupportedException("This initial compute path only accepts solid packets.");
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
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CopyResource(staging, output);
        MappedSubresource mapped = owner.Context.Map(staging, 0, MapMode.Read);
        try
        {
            var pixels = new Rgba32[packet.Width * packet.Height];
            for (int y = 0; y < packet.Height; y++)
            {
                uint* row = (uint*)((byte*)mapped.DataPointer + y * mapped.RowPitch);
                for (int x = 0; x < packet.Width; x++)
                {
                    uint packed = row[x];
                    pixels[y * packet.Width + x] = new((byte)packed, (byte)(packed >> 8), (byte)(packed >> 16), (byte)(packed >> 24));
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
        owner.Context.CSSetConstantBuffer(0, null);
        constants.Dispose(); view.Dispose(); staging.Dispose(); output.Dispose(); shader.Dispose();
        disposed = true;
    }
}
