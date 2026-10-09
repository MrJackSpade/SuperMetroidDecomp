using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>$96:FF14: four illustrated intro BG pages, copied by $8B:A4E5-A4F3 to VRAM word $5000. Named drawing fragments retain selected composition; tile strides, runs, reflection and repetition calculate.</summary>
internal static class IntroBackgroundTilemapDefinitions
{
    /// <summary>$96:FF14: shared border blank tile $B6 with palette two; scene interiors separately select $3FF.</summary>
    private const ushort BorderBlank = 0x08b6;
    /// <summary>$96:FF14: unadorned scene interior blank $3FF.</summary>
    private const ushort SceneBlank = 0x03ff;

    /// <summary>Builds the four native intro background pages from their selected tile compositions.</summary>
    /// <returns>Page tilemap words concatenated in page order, with each word stored little-endian.</returns>
    internal static byte[] Compile()
    {
        var output = new byte[IntroCinematicArtworkFormat.BackgroundPageCount * IntroCinematicArtworkFormat.BackgroundPageByteCount];
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
        {
            var canvas = new Page(output, page);
            canvas.Fill(0, 0, 32, 32, BorderBlank);
            switch (page)
            {
                case 0: MotherBrainRoom(canvas); break;
                case 1: DiscoveryCave(canvas); break;
                case 2: DeliveryPortrait(canvas); break;
                case 3: ExaminationRoom(canvas); break;
            }
        }
        return output;
    }

    /// <summary>$96:FF14 page two: selected scientist/Samus delivery portrait, palette two. The nineteen atlas rows begin at $90, with a separate shoulder strip and two foot runs.</summary>
    private static void DeliveryPortrait(Page p)
    {
        p.Fill(0, 4, 32, 20, 0x08e6);
        for (int row = 0; row < 19; row++)
        for (int column = 0; column < 16; column++)
        {
            bool occupied = row switch
            {
                0 => column is >= 9 and <= 12,
                1 => column >= 9,
                2 => column >= 7,
                >= 3 and <= 7 => column is >= 1 and <= 5 or >= 7,
                8 => column <= 5 || column >= 7,
                9 => column <= 4 || column >= 8,
                10 => true,
                _ => column < 15,
            };
            if (occupied) p.Put(8 + column, 4 + row, 0x0890 + row * 16 + column);
        }
        p.Patch(24, 6, 1, 4, 0x094f);
        p.Patch(8, 23, 7, 1, 0x0890);
        p.Patch(16, 23, 7, 1, 0x08a0);
    }

    /// <summary>$96:FF14 page three: selected examination laboratory. Atlas body $1C0-$2FF shares reflected apparatus halves; right-side $000-$089 patches are repacked into a continuous lab wall/floor.</summary>
    private static void ExaminationRoom(Page p)
    {
        p.Patch(10, 2, 3, 1, 0x089d);
        p.Put(13, 2, 0x08b0);
        p.Patch(15, 2, 3, 1, 0x089d, mirrorX: true);
        p.Put(14, 2, 0x48b0);
        p.Put(18, 2, BorderBlank | 0x4000);
        p.Patch(10, 3, 4, 1, 0x08b1);
        p.Patch(14, 3, 4, 1, 0x08b1, mirrorX: true);
        for (int row = 0; row < 20; row++)
        for (int column = 0; column < 16; column++)
        {
            bool occupied = row switch
            {
                0 or 1 => column is >= 10 and <= 13,
                >= 2 and <= 4 => column is >= 10 and <= 12,
                5 => column >= 10,
                >= 6 and <= 9 => column >= 10 - row,
                >= 10 and <= 17 => true,
                18 => column >= 1,
                _ => column >= 2,
            };
            if (occupied) p.Put(4 + column, 4 + row, 0x09c0 + row * 16 + column);
        }
        p.Patch(10, 4, 4, 2, 0x09ca, mirrorX: true);
        p.Patch(11, 6, 3, 4, 0x09ea, mirrorX: true);
        p.Patch(12, 7, 1, 3, 0x09ed);
        p.Put(12, 10, 0x0a89);
        p.Patch(13, 10, 1, 1, 0x0a2a, mirrorX: true);
        p.Patch(12, 11, 2, 5, 0x0a3a, mirrorX: true);
        p.Patch(13, 16, 1, 1, 0x0a8a, mirrorX: true);
        p.Put(13, 22, 0x0ae8);
        p.Put(13, 23, 0x0ae8);
        p.Put(18, 23, 0x0aee);
        p.Put(19, 5, 0x09df);
        p.Patch(18, 6, 2, 3, 0x09ee);
        p.Patch(20, 5, 3, 1, 0x0800);
        p.Patch(20, 6, 4, 8, 0x0810);
        p.Put(24, 13, 0x085f);
        p.Patch(20, 14, 5, 1, 0x0804);
        p.Patch(20, 15, 6, 6, 0x0814);
        for (int strip = 0; strip < 3; strip++)
            p.Patch(20 + strip * 2, 21, 2, 3, 0x080a + strip * 3 * 16);
        p.Put(24, 23, 0x087b);
        p.Put(25, 23, BorderBlank);
    }

    /// <summary>$96:FF14 page zero: selected broken tank, paired pipe loops, suspended apparatus and small wall marks. Repeated atlas motifs and geometric reflections calculate; individual damaged-glass fragments remain drawing choices.</summary>
    private static void MotherBrainRoom(Page p)
    {
        p.Fill(0, 4, 32, 22, SceneBlank);
        p.Fill(28, 25, 4, 1, BorderBlank);
        p.Fill(12, 26, 2, 1, BorderBlank | 0x4000);
        for (int x = 0; x < 32; x += 2) p.Patch(x, 2, 2, 2, 0x1dc8);
        for (int side = 0; side < 2; side++)
        {
            p.Patch(2 + side * 8, 4, 2, 2, 0x1dc8);
            p.Fill(2 + side * 8, 6, 2, 2, 0x1de9);
        }
        p.Fill(4, 4, 6, 4, 0x1de8);
        for (int y = 4; y < 8; y++) p.Patch(6, y, 2, 1, 0x9e08);
        p.Fill(4, 8, 6, 1, 0x1e78);
        for (int x = 4; x < 10; x += 2) p.Patch(x, 9, 2, 1, 0x1e68);
        p.Patch(18, 4, 2, 2, 0x3dc8);
        p.Fill(18, 6, 2, 2, 0x3de9);
        for (int x = 16; x <= 20; x += 4) p.Patch(x, 6, 2, 2, 0x3e28);
        p.Put(15, 7, 0x7e17);
        p.Put(22, 7, 0x3e17);
        p.Patch(15, 8, 2, 1, 0x3e18, mirrorX: true);
        p.Patch(21, 8, 2, 1, 0x3e18);
        p.Patch(18, 8, 2, 1, 0x3df8);
        p.Patch(18, 9, 2, 1, 0x3e68);
        p.Patch(18, 10, 2, 1, 0x3e06);

        // Three roof fragments form one half; their opposite half reflects exactly.
        p.Patch(2, 10, 2, 2, 0x1c7c);
        p.Patch(4, 10, 2, 2, 0x1c5c);
        p.Patch(6, 10, 1, 2, 0x1cc0);
        p.Patch(7, 10, 1, 2, 0x1cc0, mirrorX: true);
        p.Patch(8, 10, 2, 2, 0x1c5c, mirrorX: true);
        p.Patch(10, 10, 2, 2, 0x1c7c, mirrorX: true);
        p.Patch(2, 12, 2, 3, 0x1c0c);
        p.Patch(2, 15, 2, 3, 0x1c0c, mirrorY: true);
        p.Patch(10, 12, 2, 5, 0x1c0e);
        p.Put(10, 17, 0x1c5e);
        p.Patch(10, 18, 2, 1, 0x1c6e);
        for (int row = 0; row < 4; row++) p.Put(9, 14 + row, 0x1c3c + row % 2 * 16 + row / 2);
        p.Put(2, 18, 0x9c8c);
        p.Put(3, 18, 0x1ce0);
        p.Patch(2, 19, 2, 1, 0x9c7c);
        p.Patch(10, 19, 2, 1, 0x9c7c, mirrorX: true);
        p.Patch(5, 18, 5, 1, 0x1e01);
        p.Patch(4, 19, 6, 1, 0x1e10);

        p.Patch(0, 20, 2, 2, 0x1de6);
        p.Patch(12, 20, 2, 2, 0x3de6, mirrorX: true);
        for (int side = 0; side < 2; side++)
        {
            p.Fill(2 + side * 8, 20, 2, 2, 0x1de9);
            p.Fill(2 + side * 8, 24, 2, 2, 0x1de9);
            p.Patch(0 + side * 8, 24, 2, 2, 0x1de6, mirrorY: true);
        }
        for (int x = 4; x < 10; x += 2)
        {
            p.Patch(x, 20, 2, 1, 0x9e68);
            for (int y = 21; y < 24; y++) p.Patch(x, y, 2, 1, 0x1e08);
        }
        for (int y = 22; y < 24; y++)
        {
            p.Patch(0, y, 2, 1, 0x1df8);
            p.Patch(12, y, 2, 1, 0x3df8, mirrorX: true);
        }
        p.Patch(2, 22, 2, 2, 0x1de6);
        p.Patch(10, 22, 2, 2, 0x1de6, mirrorX: true);
        p.Patch(4, 24, 2, 2, 0x1de6, mirrorX: true, mirrorY: true);
        p.Patch(12, 24, 2, 2, 0x3de6, mirrorX: true, mirrorY: true);
        p.Patch(6, 24, 2, 2, 0x3e48);
        p.Patch(18, 17, 2, 1, 0xbe06);
        p.Patch(18, 18, 2, 1, 0xbe68);
        for (int y = 19; y < 22; y++) p.Patch(18, y, 2, 1, 0x3e08);
        p.Patch(18, 22, 2, 2, 0x3e48);
        p.Fill(18, 24, 4, 2, 0x1de9);

        p.Fill(13, 6, 1, 4, 0x0403);
        p.Fill(13, 17, 1, 3, 0x0403);
        p.Fill(21, 13, 9, 2, 0x0403);
        for (int row = 0; row < 2; row++)
        {
            p.Fill(0, 9 + row * 10, 2, 1, 0x048a);
            for (int x = 21; x < 30; x += 2) p.Put(x, 9 + row * 10, 0x048a);
            p.Fill(15, 4 + row * 19, 2, 1, 0x048b);
        }
        p.Put(24, 18, 0x3dce);
        p.Fill(25, 18, 4, 1, 0x3dcf);
        p.Put(29, 18, 0x3dde);
    }
    /// <summary>$96:FF14 page one: selected SR388 ceiling/floor rock motifs and egg ledge. Repeated four-cell rocks and reflected edge pairs share atlas patches; these are illustration tiles, not room collision geometry.</summary>
    private static void DiscoveryCave(Page p)
    {
        p.Fill(0, 4, 32, 20, SceneBlank);
        for (int group = 0; group < 2; group++)
        {
            p.Patch(group * 4, 4, 4, 2, 0x1dc0);
            p.Patch(24 + group * 4, 4, 4, 2, 0x1dc0);
            p.Patch(group * 4, 6, 4, 2, 0x1dc0, mirrorY: true);
        }
        p.Patch(8, 4, 2, 2, 0x1dc2);
        CeilingPoint(p, 10, 4, false);
        CeilingPoint(p, 12, 4, true);
        p.Patch(14, 4, 2, 2, 0x1dc6, mirrorX: true);
        p.Patch(16, 4, 4, 2, 0x1dc4);
        CeilingPoint(p, 20, 4, false);
        p.Patch(22, 4, 2, 2, 0x1dc6, mirrorX: true);
        p.Patch(8, 6, 2, 2, 0x1dc6);
        p.Patch(24, 6, 2, 2, 0x1dc6, mirrorX: true);
        p.Patch(26, 6, 4, 2, 0x1dc0, mirrorY: true);
        p.Patch(30, 6, 2, 2, 0x1dc0, mirrorY: true);
        p.Patch(0, 8, 4, 2, 0x1dc4);
        CeilingPoint(p, 4, 8, false);
        CeilingPoint(p, 6, 8, true);
        CeilingPoint(p, 26, 8, false);
        CeilingPoint(p, 28, 8, true);
        CeilingPoint(p, 30, 8, false);

        p.Patch(0, 20, 4, 2, 0x1dc4, mirrorY: true);
        p.Patch(7, 20, 2, 2, 0x1c97);
        p.Patch(9, 20, 3, 1, 0x1de3);
        p.Patch(17, 20, 3, 1, 0x1de3, mirrorX: true);
        p.Put(4, 21, 0x9dd4);
        p.Put(5, 21, 0x9dd7);
        p.Put(6, 21, 0x1ee0);
        p.Put(12, 21, 0x1ee0);
        p.Patch(9, 21, 3, 1, 0x1e20);
        p.Patch(15, 21, 2, 1, 0x1ef0);
        p.Patch(17, 21, 3, 1, 0x1e30);
        p.Patch(20, 21, 2, 1, 0x3e40);
        p.Put(22, 21, 0x3e50);
        p.Put(23, 21, 0x3ee0);
        for (int group = 0; group < 6; group++)
            p.Patch(group * 4, 22, 4, 2, 0x1dc0 | (group == 5 ? 0x2000 : 0));
        p.Patch(24, 22, 4, 2, 0x3dc4, mirrorY: true);
        p.Patch(28, 22, 2, 2, 0x3dc6, mirrorX: true, mirrorY: true);
        p.Patch(30, 22, 2, 2, 0x3dc4, mirrorY: true);
        p.Fill(20, 24, 12, 4, BorderBlank | 0x2000);
    }

    /// <summary>$96:FF14 cave: two selected two-cell stalactite drawings. Their lower rows are repacked at $1E2/$200 and $1F2/blank instead of an atlas-row stride.</summary>
    private static void CeilingPoint(Page p, int x, int y, bool narrow)
    {
        int top = narrow ? 0x1df0 : 0x1de0;
        p.Patch(x, y, 2, 1, top);
        p.Put(x, y + 1, top + 2);
        p.Put(x + 1, y + 1, narrow ? SceneBlank : 0x1e00);
    }

    /// <summary>Writes tilemap words into one page of the compiled intro background buffer.</summary>
    /// <param name="output">Combined output buffer receiving all page words.</param>
    /// <param name="page">Zero-based page index selecting the 32-by-32-word region in <paramref name="output"/>.</param>
    private sealed class Page(byte[] output, int page)
    {
        /// <summary>Stores one tilemap word at a page-local tile coordinate.</summary>
        /// <param name="x">Horizontal tile coordinate within the page.</param>
        /// <param name="y">Vertical tile coordinate within the page.</param>
        /// <param name="word">Tilemap word to write in little-endian order.</param>
        internal void Put(int x, int y, int word) => BinaryPrimitives.WriteUInt16LittleEndian(
            output.AsSpan(page * IntroCinematicArtworkFormat.BackgroundPageByteCount + (y * 32 + x) * 2), (ushort)word);

        /// <summary>Fills a rectangular page region with one repeated tilemap word.</summary>
        /// <param name="x">Left tile coordinate of the region.</param>
        /// <param name="y">Top tile coordinate of the region.</param>
        /// <param name="width">Region width in tiles.</param>
        /// <param name="height">Region height in tiles.</param>
        /// <param name="word">Tilemap word written to every tile in the region.</param>
        internal void Fill(int x, int y, int width, int height, int word)
        {
            for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++) Put(x + column, y + row, word);
        }

        /// <summary>Copies a rectangular atlas patch into the page, optionally reflecting it and setting the corresponding tile flip bits.</summary>
        /// <param name="x">Left tile coordinate where the patch is placed.</param>
        /// <param name="y">Top tile coordinate where the patch is placed.</param>
        /// <param name="width">Patch width in tiles.</param>
        /// <param name="height">Patch height in tiles.</param>
        /// <param name="word">Atlas word at the patch's unreflected origin.</param>
        /// <param name="mirrorX"><see langword="true"/> to reverse columns and set horizontal flip on each output word.</param>
        /// <param name="mirrorY"><see langword="true"/> to reverse rows and set vertical flip on each output word.</param>
        internal void Patch(int x, int y, int width, int height, int word, bool mirrorX = false, bool mirrorY = false)
        {
            for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
            {
                int sx = mirrorX ? width - 1 - column : column;
                int sy = mirrorY ? height - 1 - row : row;
                Put(x + column, y + row, (word + sy * 16 + sx) ^ (mirrorX ? 0x4000 : 0) ^ (mirrorY ? 0x8000 : 0));
            }
        }
    }
}
