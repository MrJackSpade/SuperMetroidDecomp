using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Shared main-screen composition rules for software-rendered SNES layers.</summary>
public static class SnesLayerCompositor
{
    /// <summary>
    /// Creates an opaque PPU backdrop from CGRAM color zero.
    /// </summary>
    /// <remarks>
    /// Individual BG/OBJ renderers encode palette index zero as alpha zero so higher-level
    /// code can layer them. That alpha is an internal key, not a property of the SNES video
    /// output. A completed frame must begin on the opaque backdrop or PNG/debug hosts expose
    /// the key color as transparency (and may display it using an arbitrary checker/color).
    /// </remarks>
    public static Rgba32[] CreateBackdrop(SnesCgram cgram, int pixelCount)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (pixelCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(pixelCount));

        var pixels = new Rgba32[pixelCount];
        Array.Fill(pixels, cgram.GetRgba(0));
        return pixels;
    }

    /// <summary>Copies only non-keyed pixels from one decoded layer onto another.</summary>
    public static void Composite(Span<Rgba32> destination, ReadOnlySpan<Rgba32> source)
    {
        if (destination.Length != source.Length)
            throw new ArgumentException("SNES layers must have identical pixel counts.", nameof(source));

        for (int pixel = 0; pixel < destination.Length; pixel++)
        {
            if (source[pixel].A != 0)
                destination[pixel] = source[pixel];
        }
    }
}
