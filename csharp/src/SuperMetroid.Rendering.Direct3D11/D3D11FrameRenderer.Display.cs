using Vortice.Direct3D;
using Vortice.Direct3D11;
using SuperMetroid.Core.Hardware;

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
        int displayWidth = width, displayHeight = (int)((long)width * SnesPpuLayout.ScreenHeightPixels / SnesPpuLayout.ScreenWidthPixels);
        if (displayHeight > height)
        {
            displayHeight = height;
            displayWidth = (int)((long)height * SnesPpuLayout.ScreenWidthPixels / SnesPpuLayout.ScreenHeightPixels);
        }
        displayWidth = Math.Max(1, displayWidth); displayHeight = Math.Max(1, displayHeight);
        var data = new uint[D3D11ShaderLayout.SolidConstantWords];
        data[0] = (uint)((width - displayWidth) / 2); data[1] = (uint)((height - displayHeight) / 2);
        data[2] = (uint)displayWidth; data[3] = (uint)displayHeight;
        fixed (uint* source = data) owner.Context.UpdateSubresource(constants, 0, null, (nint)source, 0, 0);
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
