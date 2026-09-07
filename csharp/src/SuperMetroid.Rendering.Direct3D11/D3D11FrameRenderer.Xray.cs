using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Mathematics;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private unsafe void DrawXrayWindow(RenderFrameSnapshot packet, XrayWindowRenderLayer layer)
    {
        var data = ClearUploadConstants();
        data[0] = (uint)D3D11TileOperation.XrayHalfColor;
        data[25] = SnesPpuLayout.GameplayHudHeightPixels;
        data[26] = SnesPpuLayout.ScreenHeightPixels;
        fixed (uint* source = data) UploadBuffer(constants, (nint)source, data.Length * sizeof(uint));
        owner.Context.Dispatch(32, 28, 1);
        childRenderer ??= Own(new D3D11FrameRenderer(owner));
        childRenderer.DrawLayers(new RenderFrameSnapshot(packet.Identity, layer.Reveal), layer.Reveal);
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        for (int y = SnesPpuLayout.GameplayHudHeightPixels; y < layer.Lines.Length; y++)
        {
            XrayWindowLine line = layer.Lines[y];
            if (line.Left > line.Right) continue;
            owner.Context.CopySubresourceRegion(output, 0, line.Left, (uint)y, 0,
                childRenderer.output, 0, new Box(line.Left, y, 0, line.Right + 1, y + 1, 1));
        }
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
    }
}
