using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="SnesGraphics"/>; never linked by player hosts.</summary>
internal static class SnesGraphicsTooling
{
    /// <summary>
    /// Converts little-endian SNES BGR555 colors into 8-bit RGBA. The console stores five
    /// bits per channel in the order RRRRR, GGGGG, BBBBB from least to most significant.
    /// </summary>
    public static IReadOnlyList<Rgba32> DecodeBgr555Palette(ReadOnlySpan<byte> bytes)
    {
        var colors = new Rgba32[bytes.Length / 2];
        for (int i = 0; i < colors.Length; i++)
        {
            // Palette bytes as uploaded to CGRAM: bit 15 is not stored.
            colors[i] = Bgr555.FromCgramPortWord((ushort)(bytes[i * 2] | (bytes[i * 2 + 1] << 8))).ToRgba32();
        }
        return colors;
    }
}
