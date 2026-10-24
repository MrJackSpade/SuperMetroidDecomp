using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Decoded source indexes and palette, before any SNES tile encoding or palette assignment.</summary>
/// <param name="Size">Image width and height in pixels.</param>
/// <param name="Pixels">Row-major unpacked palette indices, one byte per pixel; the array is exposed directly rather than copied by the record.</param>
/// <param name="Colours">Ordered RGB/alpha entries addressed by the pixel indices; the array is exposed directly and is not itself a SNES CGRAM palette.</param>
public sealed record IndexedPngImage(Extent16<ImagePixels> Size, byte[] Pixels, Rgba32[] Colours);
