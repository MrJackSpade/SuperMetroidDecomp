using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Frames drawn for verification by the game's only renderer, Direct3D 11, on the WARP device (#1409): the same software
/// device on every machine, so a frame's pixels do not depend on the graphics card. Each thread draws with its own
/// renderer, made on first use and kept for the thread's life, as the device belongs to the thread that made it.
/// </summary>
internal static class DisplayRendering
{
    /// <summary>This thread's renderer, or null before its first frame.</summary>
    [ThreadStatic] private static D3D11FrameRenderer? renderer;
    /// <summary>The publication sequence this thread's next frame takes.</summary>
    [ThreadStatic] private static long sequence;

    /// <summary>The pixels <paramref name="packet"/> shows, row by row from the top.</summary>
    internal static RgbaPixel[] Render(DisplayPacket packet) => Renderer().RenderForReadback(packet);

    /// <summary>The pixels a packet of <paramref name="scene"/> alone shows.</summary>
    internal static RgbaPixel[] Render(LayeredScene scene) => Render(new DisplayPacket(NextFrame(), scene));

    /// <summary>The identity of this thread's next frame.</summary>
    internal static RenderFrameIdentity NextFrame() => new(++sequence, 1, 0);

    /// <summary>This thread's renderer, made with its WARP device on first use.</summary>
    private static D3D11FrameRenderer Renderer() => renderer ??= new D3D11FrameRenderer(new D3D11RenderDevice(D3D11DeviceKind.Warp));
}
