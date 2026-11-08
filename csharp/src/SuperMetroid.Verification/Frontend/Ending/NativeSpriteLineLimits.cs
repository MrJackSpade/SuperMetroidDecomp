namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Which display lines the native PPU drops sprites on (#1569): the upstream PPU (upstream-sm/src/snes/ppu.c,
/// <c>ppu_evaluateSprites</c>) draws at most 32 sprites and 34 eight-pixel slivers on a line, and stops evaluating the
/// line's sprites at the first past either limit. The port draws every frame whole, so on those lines the native raster
/// shows less than the port's.
/// </summary>
internal static class NativeSpriteLineLimits
{
    /// <summary>The sprites the PPU draws on one line.</summary>
    private const int SpritesPerLine = 32;

    /// <summary>The eight-pixel slivers the PPU fetches on one line.</summary>
    private const int SliversPerLine = 34;

    /// <summary>Sprite records in an OAM upload.</summary>
    private const int Sprites = 128;

    /// <summary>The bytes of the upload's low table, four per sprite.</summary>
    private const int LowTableBytes = Sprites * 4;

    /// <summary>The small and large sprite sides for each OBSEL size selection, as the PPU's <c>spriteSizes</c> lists them.</summary>
    private static readonly int[][] SpriteSides = [[8, 16], [8, 32], [8, 64], [16, 32], [16, 64], [32, 64], [16, 32], [16, 32]];

    /// <summary>
    /// Whether the PPU's sprite evaluation for each of the 256 lines passes the line limit, for the OAM
    /// <paramref name="upload"/> (low table, then high table) under <paramref name="objectSelection"/>, with OAM
    /// priority rotation off and no interlace, as the ending's dump sets them.
    /// </summary>
    /// <exception cref="ArgumentException">The upload is not a whole OAM image.</exception>
    internal static bool[] Overflowing(ReadOnlySpan<byte> upload, int objectSelection)
    {
        if (upload.Length != LowTableBytes + Sprites / 4)
            throw new ArgumentException($"An OAM upload holds {LowTableBytes + Sprites / 4} bytes, not {upload.Length}.", nameof(upload));
        int[] sides = SpriteSides[objectSelection >> 5];
        var overflowing = new bool[256];
        for (int line = 0; line < overflowing.Length; line++)
        {
            int sprites = 0, slivers = 0;
            for (int sprite = 0; sprite < Sprites && !overflowing[line]; sprite++)
            {
                int high = upload[LowTableBytes + sprite / 4] >> (sprite % 4 * 2);
                int side = sides[(high >> 1) & 1];
                int row = (line - upload[sprite * 4 + 1]) & 0xff;
                if (row >= side)
                    continue;
                int x = upload[sprite * 4] | ((high & 1) << 8);
                if (x > 255)
                    x -= 512;
                if (x <= -side)
                    continue;
                if (++sprites > SpritesPerLine)
                {
                    overflowing[line] = true;
                    break;
                }
                for (int column = 0; column < side; column += 8)
                {
                    if (column + x > -8 && column + x < 256 && ++slivers > SliversPerLine)
                    {
                        overflowing[line] = true;
                        break;
                    }
                }
            }
        }
        return overflowing;
    }
}
