using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Reference byte-domain color math from a detached BG plane and register rows.</summary>
internal static class SoftwareBgColorMathRenderer
{
    internal static void Composite(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, Bg2BppColorMathRenderLayer layer)
    {
        int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        if (output.Length != width * height) throw new ArgumentException("BG math needs a native-sized frame.", nameof(output));
        int yMask = layer.MapHeightTiles * SnesPpuLayout.BackgroundTileSizePixels - 1;
        for (int y = layer.FirstScanline; y < height; y++)
        {
            BackgroundLineScroll scroll = layer.Scrolls[y];
            int sy = (scroll.Y + y) & yMask;
            for (int x = 0; x < width; x++)
            {
                int sx = (scroll.X + x) & (width - 1);
                SnesBgTilemapWord tile = vram.ReadWord((layer.TilemapWord +
                    (sy >> 3) * SnesPpuLayout.TilemapPageWidthInTiles + (sx >> 3)) & (SnesPpuLayout.VramWordCount - 1));
                int px = tile.FlipHorizontally ? 7 - (sx & 7) : sx & 7;
                int py = tile.FlipVertically ? 7 - (sy & 7) : sy & 7;
                int character = ((layer.CharacterWord + tile.CharacterIndex * 8) & (SnesPpuLayout.VramWordCount - 1)) * 2;
                int mask = 1 << (7 - px);
                int color = ((vram.ReadByte(character + py * 2) & mask) != 0 ? 1 : 0)
                    | ((vram.ReadByte(character + py * 2 + 1) & mask) != 0 ? 2 : 0);
                if (color == 0) continue;
                Rgba32 overlay = cgram.GetRgba(tile.PaletteIndex * 4 + color);
                int index = y * width + x;
                Rgba32 source = output[index];
                output[index] = layer.Operation == ExpandedColorMathOperation.Subtract
                    ? new((byte)Math.Max(0, source.R - overlay.R), (byte)Math.Max(0, source.G - overlay.G),
                        (byte)Math.Max(0, source.B - overlay.B), source.A)
                    : new((byte)Math.Min(255, source.R + overlay.R), (byte)Math.Min(255, source.G + overlay.G),
                        (byte)Math.Min(255, source.B + overlay.B), source.A);
            }
        }
    }
}
