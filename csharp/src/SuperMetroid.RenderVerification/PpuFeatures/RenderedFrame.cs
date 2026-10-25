using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.RenderVerification.PpuFeatures;

/// <summary>A rendered frame and its pixels.</summary>
/// <param name="Packet">The packet.</param>
/// <param name="Pixels">The pixels.</param>
internal readonly record struct RenderedFrame(DisplayPacket Packet, RgbaPixel[] Pixels);
