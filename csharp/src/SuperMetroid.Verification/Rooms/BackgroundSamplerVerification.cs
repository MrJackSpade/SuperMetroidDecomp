using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Compares cached SNES background pixel sampling with an independent
/// scalar tile, palette, and priority calculation.</summary>
internal static class BackgroundSamplerVerification
{
    /// <summary>Checks two- and four-bit tiles through map pages, flips, color
    /// zero, priority, and VRAM wrapping with seeded source memory.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyBackgroundSampler()
    {
        var random = new Random(355);
        var bytes = new byte[SnesVram.ByteCount];
        random.NextBytes(bytes);
        var vram = new SnesVram();
        vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < 256; i++) cgram.SetColor(i, Bgr555.FromWord(checked((ushort)random.Next(32768))));
        Rgba32[] colors = Enumerable.Range(0, 256).Select(cgram.GetRgba).ToArray();
        int checks = 0;
        foreach (bool fourBit in new[] { false, true })
        foreach (ushort width in new ushort[] { 32, 64 })
        foreach (ushort height in new ushort[] { 32, 64 })
        {
            // Exercise VRAM wrapping as well as page boundaries and random flipped tiles.
            int map = 32760, characters = 32760;
            var sampler = new SnesBackgroundPixelSampler(vram, colors,
                new VramBgTilemap(vram, map, new Extent16<TilemapCells>(width, height)), characters, fourBit ? BgBitDepth.FourBpp : BgBitDepth.TwoBpp);
            for (int run = 0; run < 256; run++)
            {
                int originX = random.Next(-1024, 1024), y = random.Next(-1024, 1024);
                for (int offset = 0; offset < 24; offset++)
                foreach (bool opaqueZero in new[] { false, true })
                {
                    int x = originX + offset;
                    // Independent scalar formula retained from the pre-cache renderer.
                    int sx = x & (width * 8 - 1), sy = y & (height * 8 - 1);
                    int tx = sx >> 3, ty = sy >> 3;
                    int page = ((ty >> 5) * (width >> 5) + (tx >> 5)) * 1024;
                    SnesBgTilemapWord tile = vram.ReadWord((map + page + (ty & 31) * 32 + (tx & 31)) & 32767);
                    int px = tile.FlipHorizontally ? 7 - (sx & 7) : sx & 7;
                    int py = tile.FlipVertically ? 7 - (sy & 7) : sy & 7;
                    int row = unchecked((ushort)((characters + tile.CharacterIndex * (fourBit ? 16 : 8)) * 2 + py * 2));
                    int shift = 7 - px;
                    int color = (bytes[row] >> shift & 1) | (bytes[unchecked((ushort)(row + 1))] >> shift & 1) << 1;
                    if (fourBit) color |= (bytes[unchecked((ushort)(row + 16))] >> shift & 1) << 2 | (bytes[unchecked((ushort)(row + 17))] >> shift & 1) << 3;
                    var actual = sampler.Sample(new Position16<BgPlanePixels>(unchecked((ushort)x), unchecked((ushort)y)), opaqueZero ? ColorZero.Opaque : ColorZero.Transparent);
                    Rgba32 expected = color == 0 && !opaqueZero ? default : colors[tile.PaletteIndex * (fourBit ? 16 : 4) + color];
                    AssertEqual(expected, actual.Color, "cached background color matches scalar sampling");
                    AssertEqual(tile.HasPriority, actual.High, "cached background priority matches scalar sampling");
                    checks++;
                }
            }
        }
        Console.WriteLine($"PASS cached background sampler: {checks} scalar comparisons, flips, pages, wrap, discontinuities and color zero.");
    }
}
