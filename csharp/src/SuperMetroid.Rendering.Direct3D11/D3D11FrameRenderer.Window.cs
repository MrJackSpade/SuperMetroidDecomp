using SuperMetroid.Core.Rendering;
using Vortice.Mathematics;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private D3D11FrameRenderer? childRenderer;

    private void DrawWindow(RenderFrameSnapshot packet, WindowedSceneRenderLayer window)
    {
        if (window.Left == window.Right || window.Top == window.Bottom) return;
        // A single reusable child owns distinct memory, resolved OAM and output.
        // The contract prohibits nested windows, bounding scratch resource usage.
        childRenderer ??= Own(new D3D11FrameRenderer(owner));
        var childPacket = new RenderFrameSnapshot(packet.Identity, window.Scene);
        childRenderer.DrawLayers(childPacket, window.Scene);
        owner.Context.CSSetUnorderedAccessView(0, null);
        owner.Context.CSSetUnorderedAccessView(1, null);
        owner.Context.CopySubresourceRegion(output, 0, (uint)window.Left, (uint)window.Top, 0,
            childRenderer.output, 0, new Box(window.Left, window.Top, 0, window.Right, window.Bottom, 1));
        // The immediate context is shared but every scene's resources are separate.
        // Restore the parent before later overlays, OBJ insertion, or outer fades.
        owner.Context.CSSetShader(tileShader);
        owner.Context.CSSetConstantBuffer(0, constants);
        owner.Context.CSSetShaderResource(0, memoryView);
        owner.Context.CSSetUnorderedAccessView(0, view);
        owner.Context.CSSetUnorderedAccessView(1, objectView);
    }
}
