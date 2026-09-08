using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Shared main-screen composition rules for software-rendered SNES layers.</summary>
public static class SnesLayerCompositor
{
    /// <summary>
    /// Adds a decoded subscreen to an already opaque main screen in five-bit SNES
    /// color space, without halving. Keyed sub pixels select black fixed color.
    /// Callers own CGADSUB layer eligibility and window masking before this stage.
    /// </summary>
    public static void AddSubscreen(Span<Rgba32> main, ReadOnlySpan<Rgba32> sub)
    {
        if (main.Length != sub.Length)
            throw new ArgumentException("SNES screens must have identical pixel counts.", nameof(sub));
        for (int i = 0; i < main.Length; i++)
        {
            if (sub[i].A == 0) continue;
            main[i] = new Rgba32(Add(main[i].R, sub[i].R), Add(main[i].G, sub[i].G), Add(main[i].B, sub[i].B));
        }
        static byte Add(byte first, byte second)
        {
            int fiveBit = Math.Min(31, (first >> 3) + (second >> 3));
            return (byte)((fiveBit << 3) | (fiveBit >> 2));
        }
    }
    /// <summary>
    /// Creates an opaque PPU backdrop from CGRAM color zero.
    /// </summary>
    /// <remarks>
    /// Individual BG/OBJ renderers encode palette index zero as alpha zero so higher-level
    /// code can layer them. That alpha is an internal key, not a property of the SNES video
    /// output. A completed frame must begin on the opaque backdrop or PNG/debug hosts expose
    /// the key color as transparency (and may display it using an arbitrary checker/color).
    /// </remarks>
    public static Rgba32[] CreateBackdrop(SnesCgram cgram, int pixelCount, Rgba32[]? outputBuffer = null)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pixelCount);

        if (outputBuffer is not null && outputBuffer.Length != pixelCount)
            throw new ArgumentException("Unexpected backdrop buffer dimensions.", nameof(outputBuffer));
        var pixels = outputBuffer ?? new Rgba32[pixelCount];
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
