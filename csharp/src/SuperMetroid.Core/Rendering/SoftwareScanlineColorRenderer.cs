using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Observational expanded-byte color addition over immutable scanline windows.</summary>
internal static class SoftwareScanlineColorRenderer
{
    internal static void Composite(Span<Rgba32> pixels, ScanlineColorAddRenderLayer layer)
    {
        if (pixels.Length != SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Color windows require a native-sized frame.", nameof(pixels));
        for (int y = 0; y < layer.Windows.Length; y++)
        {
            ColorAddWindow window = layer.Windows[y];
            for (int x = window.Left; x <= window.Right; x++)
            {
                int index = y * SnesPpuLayout.ScreenWidthPixels + x;
                Rgba32 source = pixels[index];
                pixels[index] = new((byte)Math.Min(255, source.R + window.Red),
                    (byte)Math.Min(255, source.G + window.Green),
                    (byte)Math.Min(255, source.B + window.Blue), source.A);
            }
        }
    }
}
