using System.Buffers.Binary;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>$96:FF14: four illustrated intro BG pages, copied by $8B:A4E5-A4F3 to VRAM word $5000. Named drawing fragments retain selected composition; tile strides, runs, reflection and repetition calculate.</summary>
internal static class IntroBackgroundTilemapDefinitions
{
    /// <summary>$96:FF14: shared border blank tile $B6 with palette two; scene interiors separately select $3FF.</summary>
    private const ushort BorderBlank = 0x08b6;
    /// <summary>$96:FF14: unadorned scene interior blank $3FF.</summary>
    private const ushort SceneBlank = 0x03ff;

    /// <summary>Composes the four 32×32 intro pages in native page order, filling border tiles before drawing each scene.</summary>
    internal static byte[] Compile()
    {
        var output = new byte[IntroCinematicArtworkFormat.BackgroundPageCount * IntroCinematicArtworkFormat.BackgroundPageByteCount];
        for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
        {
            var canvas = new Page(output, page);
            canvas.Fill(new(0, 0), new(32, 32), BorderBlank);
            switch (page)
            {
                case 0: MotherBrainRoom(canvas); break;
                case 1: DiscoveryCave(canvas); break;
                case 2: DeliveryPortrait(canvas); break;
                case 3: ExaminationRoom(canvas); break;
                default:
                    throw new InvalidOperationException($"Intro background page {page} has no tilemap.");
            }
        }
        return output;
    }

    /// <summary>$96:FF14 page two: selected scientist/Samus delivery portrait, palette two. The nineteen atlas rows begin at $90, with a separate shoulder strip and two foot runs.</summary>
    private static void DeliveryPortrait(Page p)
    {
        p.Fill(new(0, 4), new(32, 20), 0x08e6);
        for (int row = 0; row < 19; row++)
        for (int column = 0; column < 16; column++)
        {
            bool occupied = row switch
            {
                0 => column is >= 9 and <= 12,
                1 => column >= 9,
                2 => column >= 7,
                >= 3 and <= 7 => column is >= 1 and <= 5 or >= 7,
                8 => column is <= 5 or >= 7,
                9 => column is <= 4 or >= 8,
                10 => true,
                _ => column < 15,
            };
            if (occupied) p.Put(new(checked((ushort)(8 + column)), checked((ushort)(4 + row))), 0x0890 + row * 16 + column);
        }
        p.Patch(new(24, 6), new(1, 4), 0x094f);
        p.Patch(new(8, 23), new(7, 1), 0x0890);
        p.Patch(new(16, 23), new(7, 1), 0x08a0);
    }

    /// <summary>$96:FF14 page three: selected examination laboratory. Atlas body $1C0-$2FF shares reflected apparatus halves; right-side $000-$089 patches are repacked into a continuous lab wall/floor.</summary>
    private static void ExaminationRoom(Page p)
    {
        p.Patch(new(10, 2), new(3, 1), 0x089d);
        p.Put(new(13, 2), 0x08b0);
        p.Patch(new(15, 2), new(3, 1), 0x089d, SnesTileFlipFlags.Horizontal);
        p.Put(new(14, 2), 0x48b0);
        p.Put(new(18, 2), BorderBlank | 0x4000);
        p.Patch(new(10, 3), new(4, 1), 0x08b1);
        p.Patch(new(14, 3), new(4, 1), 0x08b1, SnesTileFlipFlags.Horizontal);
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
            if (occupied) p.Put(new(checked((ushort)(4 + column)), checked((ushort)(4 + row))), 0x09c0 + row * 16 + column);
        }
        p.Patch(new(10, 4), new(4, 2), 0x09ca, SnesTileFlipFlags.Horizontal);
        p.Patch(new(11, 6), new(3, 4), 0x09ea, SnesTileFlipFlags.Horizontal);
        p.Patch(new(12, 7), new(1, 3), 0x09ed);
        p.Put(new(12, 10), 0x0a89);
        p.Patch(new(13, 10), new(1, 1), 0x0a2a, SnesTileFlipFlags.Horizontal);
        p.Patch(new(12, 11), new(2, 5), 0x0a3a, SnesTileFlipFlags.Horizontal);
        p.Patch(new(13, 16), new(1, 1), 0x0a8a, SnesTileFlipFlags.Horizontal);
        p.Put(new(13, 22), 0x0ae8);
        p.Put(new(13, 23), 0x0ae8);
        p.Put(new(18, 23), 0x0aee);
        p.Put(new(19, 5), 0x09df);
        p.Patch(new(18, 6), new(2, 3), 0x09ee);
        p.Patch(new(20, 5), new(3, 1), 0x0800);
        p.Patch(new(20, 6), new(4, 8), 0x0810);
        p.Put(new(24, 13), 0x085f);
        p.Patch(new(20, 14), new(5, 1), 0x0804);
        p.Patch(new(20, 15), new(6, 6), 0x0814);
        for (int strip = 0; strip < 3; strip++)
            p.Patch(new(checked((ushort)(20 + strip * 2)), 21), new(2, 3), 0x080a + strip * 3 * 16);
        p.Put(new(24, 23), 0x087b);
        p.Put(new(25, 23), BorderBlank);
    }

    /// <summary>$96:FF14 page zero: selected broken tank, paired pipe loops, suspended apparatus and small wall marks. Repeated atlas motifs and geometric reflections calculate; individual damaged-glass fragments remain drawing choices.</summary>
    private static void MotherBrainRoom(Page p)
    {
        p.Fill(new(0, 4), new(32, 22), SceneBlank);
        p.Fill(new(28, 25), new(4, 1), BorderBlank);
        p.Fill(new(12, 26), new(2, 1), BorderBlank | 0x4000);
        for (int x = 0; x < 32; x += 2) p.Patch(new(checked((ushort)x), 2), new(2, 2), 0x1dc8);
        for (int side = 0; side < 2; side++)
        {
            p.Patch(new(checked((ushort)(2 + side * 8)), 4), new(2, 2), 0x1dc8);
            p.Fill(new(checked((ushort)(2 + side * 8)), 6), new(2, 2), 0x1de9);
        }
        p.Fill(new(4, 4), new(6, 4), 0x1de8);
        for (int y = 4; y < 8; y++) p.Patch(new(6, checked((ushort)y)), new(2, 1), 0x9e08);
        p.Fill(new(4, 8), new(6, 1), 0x1e78);
        for (int x = 4; x < 10; x += 2) p.Patch(new(checked((ushort)x), 9), new(2, 1), 0x1e68);
        p.Patch(new(18, 4), new(2, 2), 0x3dc8);
        p.Fill(new(18, 6), new(2, 2), 0x3de9);
        for (int x = 16; x <= 20; x += 4) p.Patch(new(checked((ushort)x), 6), new(2, 2), 0x3e28);
        p.Put(new(15, 7), 0x7e17);
        p.Put(new(22, 7), 0x3e17);
        p.Patch(new(15, 8), new(2, 1), 0x3e18, SnesTileFlipFlags.Horizontal);
        p.Patch(new(21, 8), new(2, 1), 0x3e18);
        p.Patch(new(18, 8), new(2, 1), 0x3df8);
        p.Patch(new(18, 9), new(2, 1), 0x3e68);
        p.Patch(new(18, 10), new(2, 1), 0x3e06);

        // Three roof fragments form one half; their opposite half reflects exactly.
        p.Patch(new(2, 10), new(2, 2), 0x1c7c);
        p.Patch(new(4, 10), new(2, 2), 0x1c5c);
        p.Patch(new(6, 10), new(1, 2), 0x1cc0);
        p.Patch(new(7, 10), new(1, 2), 0x1cc0, SnesTileFlipFlags.Horizontal);
        p.Patch(new(8, 10), new(2, 2), 0x1c5c, SnesTileFlipFlags.Horizontal);
        p.Patch(new(10, 10), new(2, 2), 0x1c7c, SnesTileFlipFlags.Horizontal);
        p.Patch(new(2, 12), new(2, 3), 0x1c0c);
        p.Patch(new(2, 15), new(2, 3), 0x1c0c, SnesTileFlipFlags.Vertical);
        p.Patch(new(10, 12), new(2, 5), 0x1c0e);
        p.Put(new(10, 17), 0x1c5e);
        p.Patch(new(10, 18), new(2, 1), 0x1c6e);
        for (int row = 0; row < 4; row++) p.Put(new(9, checked((ushort)(14 + row))), 0x1c3c + row % 2 * 16 + row / 2);
        p.Put(new(2, 18), 0x9c8c);
        p.Put(new(3, 18), 0x1ce0);
        p.Patch(new(2, 19), new(2, 1), 0x9c7c);
        p.Patch(new(10, 19), new(2, 1), 0x9c7c, SnesTileFlipFlags.Horizontal);
        p.Patch(new(5, 18), new(5, 1), 0x1e01);
        p.Patch(new(4, 19), new(6, 1), 0x1e10);

        p.Patch(new(0, 20), new(2, 2), 0x1de6);
        p.Patch(new(12, 20), new(2, 2), 0x3de6, SnesTileFlipFlags.Horizontal);
        for (int side = 0; side < 2; side++)
        {
            p.Fill(new(checked((ushort)(2 + side * 8)), 20), new(2, 2), 0x1de9);
            p.Fill(new(checked((ushort)(2 + side * 8)), 24), new(2, 2), 0x1de9);
            p.Patch(new(checked((ushort)(0 + side * 8)), 24), new(2, 2), 0x1de6, SnesTileFlipFlags.Vertical);
        }
        for (int x = 4; x < 10; x += 2)
        {
            p.Patch(new(checked((ushort)x), 20), new(2, 1), 0x9e68);
            for (int y = 21; y < 24; y++) p.Patch(new(checked((ushort)x), checked((ushort)y)), new(2, 1), 0x1e08);
        }
        for (int y = 22; y < 24; y++)
        {
            p.Patch(new(0, checked((ushort)y)), new(2, 1), 0x1df8);
            p.Patch(new(12, checked((ushort)y)), new(2, 1), 0x3df8, SnesTileFlipFlags.Horizontal);
        }
        p.Patch(new(2, 22), new(2, 2), 0x1de6);
        p.Patch(new(10, 22), new(2, 2), 0x1de6, SnesTileFlipFlags.Horizontal);
        p.Patch(new(4, 24), new(2, 2), 0x1de6, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical);
        p.Patch(new(12, 24), new(2, 2), 0x3de6, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical);
        p.Patch(new(6, 24), new(2, 2), 0x3e48);
        p.Patch(new(18, 17), new(2, 1), 0xbe06);
        p.Patch(new(18, 18), new(2, 1), 0xbe68);
        for (int y = 19; y < 22; y++) p.Patch(new(18, checked((ushort)y)), new(2, 1), 0x3e08);
        p.Patch(new(18, 22), new(2, 2), 0x3e48);
        p.Fill(new(18, 24), new(4, 2), 0x1de9);

        p.Fill(new(13, 6), new(1, 4), 0x0403);
        p.Fill(new(13, 17), new(1, 3), 0x0403);
        p.Fill(new(21, 13), new(9, 2), 0x0403);
        for (int row = 0; row < 2; row++)
        {
            p.Fill(new(0, checked((ushort)(9 + row * 10))), new(2, 1), 0x048a);
            for (int x = 21; x < 30; x += 2) p.Put(new(checked((ushort)x), checked((ushort)(9 + row * 10))), 0x048a);
            p.Fill(new(15, checked((ushort)(4 + row * 19))), new(2, 1), 0x048b);
        }
        p.Put(new(24, 18), 0x3dce);
        p.Fill(new(25, 18), new(4, 1), 0x3dcf);
        p.Put(new(29, 18), 0x3dde);
    }
    /// <summary>$96:FF14 page one: selected SR388 ceiling/floor rock motifs and egg ledge. Repeated four-cell rocks and reflected edge pairs share atlas patches; these are illustration tiles, not room collision geometry.</summary>
    private static void DiscoveryCave(Page p)
    {
        p.Fill(new(0, 4), new(32, 20), SceneBlank);
        for (int group = 0; group < 2; group++)
        {
            p.Patch(new(checked((ushort)(group * 4)), 4), new(4, 2), 0x1dc0);
            p.Patch(new(checked((ushort)(24 + group * 4)), 4), new(4, 2), 0x1dc0);
            p.Patch(new(checked((ushort)(group * 4)), 6), new(4, 2), 0x1dc0, SnesTileFlipFlags.Vertical);
        }
        p.Patch(new(8, 4), new(2, 2), 0x1dc2);
        CeilingPoint(p, new(10, 4), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(12, 4), IntroCeilingPointWidth.Narrow);
        p.Patch(new(14, 4), new(2, 2), 0x1dc6, SnesTileFlipFlags.Horizontal);
        p.Patch(new(16, 4), new(4, 2), 0x1dc4);
        CeilingPoint(p, new(20, 4), IntroCeilingPointWidth.Wide);
        p.Patch(new(22, 4), new(2, 2), 0x1dc6, SnesTileFlipFlags.Horizontal);
        p.Patch(new(8, 6), new(2, 2), 0x1dc6);
        p.Patch(new(24, 6), new(2, 2), 0x1dc6, SnesTileFlipFlags.Horizontal);
        p.Patch(new(26, 6), new(4, 2), 0x1dc0, SnesTileFlipFlags.Vertical);
        p.Patch(new(30, 6), new(2, 2), 0x1dc0, SnesTileFlipFlags.Vertical);
        p.Patch(new(0, 8), new(4, 2), 0x1dc4);
        CeilingPoint(p, new(4, 8), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(6, 8), IntroCeilingPointWidth.Narrow);
        CeilingPoint(p, new(26, 8), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(28, 8), IntroCeilingPointWidth.Narrow);
        CeilingPoint(p, new(30, 8), IntroCeilingPointWidth.Wide);

        p.Patch(new(0, 20), new(4, 2), 0x1dc4, SnesTileFlipFlags.Vertical);
        p.Patch(new(7, 20), new(2, 2), 0x1c97);
        p.Patch(new(9, 20), new(3, 1), 0x1de3);
        p.Patch(new(17, 20), new(3, 1), 0x1de3, SnesTileFlipFlags.Horizontal);
        p.Put(new(4, 21), 0x9dd4);
        p.Put(new(5, 21), 0x9dd7);
        p.Put(new(6, 21), 0x1ee0);
        p.Put(new(12, 21), 0x1ee0);
        p.Patch(new(9, 21), new(3, 1), 0x1e20);
        p.Patch(new(15, 21), new(2, 1), 0x1ef0);
        p.Patch(new(17, 21), new(3, 1), 0x1e30);
        p.Patch(new(20, 21), new(2, 1), 0x3e40);
        p.Put(new(22, 21), 0x3e50);
        p.Put(new(23, 21), 0x3ee0);
        for (int group = 0; group < 6; group++)
            p.Patch(new(checked((ushort)(group * 4)), 22), new(4, 2), 0x1dc0 | (group == 5 ? 0x2000 : 0));
        p.Patch(new(24, 22), new(4, 2), 0x3dc4, SnesTileFlipFlags.Vertical);
        p.Patch(new(28, 22), new(2, 2), 0x3dc6, SnesTileFlipFlags.Horizontal | SnesTileFlipFlags.Vertical);
        p.Patch(new(30, 22), new(2, 2), 0x3dc4, SnesTileFlipFlags.Vertical);
        p.Fill(new(20, 24), new(12, 4), BorderBlank | 0x2000);
    }

    /// <summary>$96:FF14 cave: two selected two-cell stalactite drawings. Their lower rows are repacked at $1E2/$200 and $1F2/blank instead of an atlas-row stride.</summary>
    private static void CeilingPoint(Page p, Position16<TilemapCells> cell, IntroCeilingPointWidth width)
    {
        int top = width == IntroCeilingPointWidth.Narrow ? 0x1df0 : 0x1de0;
        p.Patch(cell, new(2, 1), top);
        p.Put(cell + new Offset16<TilemapCells>(0, 1), top + 2);
        p.Put(cell + new Offset16<TilemapCells>(1, 1), width == IntroCeilingPointWidth.Narrow ? SceneBlank : 0x1e00);
    }

    /// <summary>Writes one selected intro background page into its slice of the combined VRAM tilemap.</summary>
    private sealed class Page(byte[] output, int page)
    {
        /// <summary>Each illustrated page has 32 rows and columns of tilemap words.</summary>
        private static readonly Extent16<TilemapCells> PageSize = new(32, 32);

        /// <summary>Writes a little-endian tilemap word at the page-local cell.</summary>
        internal void Put(Position16<TilemapCells> cell, int word) => BinaryPrimitives.WriteUInt16LittleEndian(
            output.AsSpan(page * IntroCinematicArtworkFormat.BackgroundPageByteCount + PageSize.RowMajorIndex(cell) * 2), (ushort)word);
        /// <summary>Paints a rectangular run with one tilemap word, used for backgrounds and blank regions.</summary>
        internal void Fill(Position16<TilemapCells> origin, Extent16<TilemapCells> size, int word)
        {
            for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++) Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)), word);
        }
        /// <summary>Copies a row-major atlas patch with a 16-word source stride, applying SNES flip bits when reflected.</summary>
        internal void Patch(Position16<TilemapCells> origin, Extent16<TilemapCells> size, int word, SnesTileFlipFlags flips = SnesTileFlipFlags.None)
        {
            for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++)
            {
                int sx = (flips & SnesTileFlipFlags.Horizontal) != 0 ? size.X - 1 - column : column;
                int sy = (flips & SnesTileFlipFlags.Vertical) != 0 ? size.Y - 1 - row : row;
                Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)),
                    (word + sy * 16 + sx) ^ (int)flips);
            }
        }
    }
}
