using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.RenderVerification.Bosses;

/// <summary>A retained frame and the pixels it should render.</summary>
/// <param name="Packet">The packet.</param>
/// <param name="Expected">The expected.</param>
internal readonly record struct RetainedFrame(DisplayPacket Packet, RgbaPixel[] Expected);
