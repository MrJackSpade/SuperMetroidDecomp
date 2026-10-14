using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>A fully composed, top-left-origin room image in row-major RGBA order.</summary>
public sealed record RenderedRoom(Extent16<ImagePixels> Size, Rgba32[] Pixels);
