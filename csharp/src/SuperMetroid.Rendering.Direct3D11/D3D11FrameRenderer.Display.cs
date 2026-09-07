using Vortice.Direct3D;
using Vortice.Direct3D11;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    /// <summary>Draws the submitted integer frame into an owner-device target, without readback.</summary>
    internal unsafe void DrawDisplay(ID3D11RenderTargetView target, int width, int height)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (renderedIdentity is null) throw new InvalidOperationException("No frame has been submitted for display.");
        var viewport = DisplayViewport.ForClient(width, height);
        var data = System.Runtime.InteropServices.MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = viewport.Left; data[1] = viewport.Top;
        data[2] = viewport.Width; data[3] = viewport.Height;
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.OMSetRenderTargets(target);
        owner.Context.RSSetViewport(0, 0, width, height);
        owner.Context.IASetInputLayout(null);
        owner.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        owner.Context.VSSetShader(displayVertexShader);
        owner.Context.PSSetShader(displayPixelShader);
        owner.Context.PSSetConstantBuffer(0, constants);
        owner.Context.PSSetShaderResource(0, displaySource);
        owner.Context.Draw(3, 0);
        // Avoid SRV/UAV hazards on the next emulation frame and release target binding
        // before a later swapchain resize. Raster state belongs to this render owner.
        owner.Context.PSSetShaderResource(0, null!);
        owner.Context.OMSetRenderTargets(Array.Empty<ID3D11RenderTargetView>());
    }
}
