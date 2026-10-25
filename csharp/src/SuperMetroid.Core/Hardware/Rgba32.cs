namespace SuperMetroid.Core.Hardware;

/// <summary>Immutable host color with four independent eight-bit channels, rather than a packed SNES CGRAM word or palette index.</summary>
/// <param name="R">Red channel intensity, 0..255.</param>
/// <param name="G">Green channel intensity, 0..255.</param>
/// <param name="B">Blue channel intensity, 0..255.</param>
/// <param name="A">Alpha channel, with 0 transparent and 255 opaque; omitted alpha defaults to full opacity.</param>
/// <remarks>
/// The single owner of the host RGBA8888 pixel encoding: pixel buffers are reinterpreted as, and read back from, GPU
/// textures in this four-byte layout, so each channel is stored at one byte.
/// </remarks>
[EncodingOwner("RGBA8888 texture pixel")]
public readonly record struct Rgba32(byte R, byte G, byte B, byte A = 255); // cartridge-encoding: allow(HostPixelFormat) - host RGBA8888 pixel: four one-byte channels whose 4-byte layout pixel buffers are reinterpreted as and read back from GPU textures
