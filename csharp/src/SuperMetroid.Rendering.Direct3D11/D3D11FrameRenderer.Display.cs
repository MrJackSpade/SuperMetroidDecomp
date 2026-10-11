using SuperMetroid.Core.Spatial;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    /// <summary>Draws the submitted integer frame into an owner-device target, without readback.</summary>
    internal unsafe void DrawDisplay(ID3D11RenderTargetView target, Extent16<DisplayPixels> client)
    {
        owner.VerifyOwner(); ObjectDisposedException.ThrowIf(disposed, this);
        if (client.X == 0 || client.Y == 0)
            throw new ArgumentOutOfRangeException(nameof(client), client, "A display target must have positive dimensions.");
        if (renderedIdentity is null) throw new InvalidOperationException("No frame has been submitted for display.");
        var viewport = DisplayViewport.ForClient(client);
        var data = System.Runtime.InteropServices.MemoryMarshal.Cast<uint, int>(ClearUploadConstants().AsSpan());
        data[0] = viewport.Offset.X; data[1] = viewport.Offset.Y;
        data[2] = viewport.Size.X; data[3] = viewport.Size.Y;
        fixed (int* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.OMSetRenderTargets(target);
        owner.Context.RSSetViewport(0, 0, client.X, client.Y);
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
