using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>A rendered frame and its pixels.</summary>
/// <param name="Packet">The packet.</param>
/// <param name="Pixels">The pixels.</param>
internal readonly record struct RenderedFrame(DisplayPacket Packet, RgbaPixel[] Pixels);
