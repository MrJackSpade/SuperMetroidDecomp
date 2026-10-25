using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.ReferenceRendering;

/// <summary>A fully composed, top-left-origin room image in row-major RGBA order.</summary>
public sealed record RenderedRoom(Extent16<ImagePixels> Size, RgbaPixel[] Pixels);
