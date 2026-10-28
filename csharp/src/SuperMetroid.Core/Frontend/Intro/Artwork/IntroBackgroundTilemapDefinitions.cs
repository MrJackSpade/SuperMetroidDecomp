using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>$96:FF14: four illustrated intro BG pages, copied by $8B:A4E5-A4F3 to VRAM word $5000. Named drawing fragments retain selected composition; tile strides, runs, reflection and repetition calculate.</summary>
internal static class IntroBackgroundTilemapDefinitions
{
    /// <summary>$96:FF14: shared border blank tile $B6 in colour variant two; scene interiors separately select $3FF.</summary>
    private static BackgroundTile BorderBlank => Tiled(0x0b6, 2);
    /// <summary>$96:FF14: unadorned scene interior blank $3FF.</summary>
    private static BackgroundTile SceneBlank => Tiled(0x3ff, 0);

    /// <summary>Tile <paramref name="tile"/> of the intro atlas in <paramref name="colourVariant"/>, prioritised and mirrored as drawn.</summary>
    private static BackgroundTile Tiled(int tile, int colourVariant, bool priority = false, TileFlip mirroring = TileFlip.None) =>
        new(tile, colourVariant, priority, mirroring);

    /// <summary>Composes the four 32×32 intro pages in native page order, filling border tiles before drawing each scene.</summary>
    internal static BackgroundTile[] Compile()
    {
        var output = new BackgroundTile[IntroCinematicArtworkFormat.BackgroundPageCount * Page.Cells];
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
        p.Fill(new(0, 4), new(32, 20), Tiled(0x0e6, 2));
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
            if (occupied) p.Put(new(checked((ushort)(8 + column)), checked((ushort)(4 + row))), Tiled(0x090 + row * 16 + column, 2));
        }
        p.Patch(new(24, 6), new(1, 4), Tiled(0x14f, 2));
        p.Patch(new(8, 23), new(7, 1), Tiled(0x090, 2));
        p.Patch(new(16, 23), new(7, 1), Tiled(0x0a0, 2));
    }

    /// <summary>$96:FF14 page three: selected examination laboratory. Atlas body $1C0-$2FF shares reflected apparatus halves; right-side $000-$089 patches are repacked into a continuous lab wall/floor.</summary>
    private static void ExaminationRoom(Page p)
    {
        p.Patch(new(10, 2), new(3, 1), Tiled(0x09d, 2));
        p.Put(new(13, 2), Tiled(0x0b0, 2));
        p.Patch(new(15, 2), new(3, 1), Tiled(0x09d, 2), TileFlip.Horizontal);
        p.Put(new(14, 2), Tiled(0x0b0, 2, mirroring: TileFlip.Horizontal));
        p.Put(new(18, 2), BorderBlank with { FlipHorizontally = true });
        p.Patch(new(10, 3), new(4, 1), Tiled(0x0b1, 2));
        p.Patch(new(14, 3), new(4, 1), Tiled(0x0b1, 2), TileFlip.Horizontal);
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
            if (occupied) p.Put(new(checked((ushort)(4 + column)), checked((ushort)(4 + row))), Tiled(0x1c0 + row * 16 + column, 2));
        }
        p.Patch(new(10, 4), new(4, 2), Tiled(0x1ca, 2), TileFlip.Horizontal);
        p.Patch(new(11, 6), new(3, 4), Tiled(0x1ea, 2), TileFlip.Horizontal);
        p.Patch(new(12, 7), new(1, 3), Tiled(0x1ed, 2));
        p.Put(new(12, 10), Tiled(0x289, 2));
        p.Patch(new(13, 10), new(1, 1), Tiled(0x22a, 2), TileFlip.Horizontal);
        p.Patch(new(12, 11), new(2, 5), Tiled(0x23a, 2), TileFlip.Horizontal);
        p.Patch(new(13, 16), new(1, 1), Tiled(0x28a, 2), TileFlip.Horizontal);
        p.Put(new(13, 22), Tiled(0x2e8, 2));
        p.Put(new(13, 23), Tiled(0x2e8, 2));
        p.Put(new(18, 23), Tiled(0x2ee, 2));
        p.Put(new(19, 5), Tiled(0x1df, 2));
        p.Patch(new(18, 6), new(2, 3), Tiled(0x1ee, 2));
        p.Patch(new(20, 5), new(3, 1), Tiled(0x000, 2));
        p.Patch(new(20, 6), new(4, 8), Tiled(0x010, 2));
        p.Put(new(24, 13), Tiled(0x05f, 2));
        p.Patch(new(20, 14), new(5, 1), Tiled(0x004, 2));
        p.Patch(new(20, 15), new(6, 6), Tiled(0x014, 2));
        for (int strip = 0; strip < 3; strip++)
            p.Patch(new(checked((ushort)(20 + strip * 2)), 21), new(2, 3), Tiled(0x00a + strip * 3 * 16, 2));
        p.Put(new(24, 23), Tiled(0x07b, 2));
        p.Put(new(25, 23), BorderBlank);
    }

    /// <summary>$96:FF14 page zero: selected broken tank, paired pipe loops, suspended apparatus and small wall marks. Repeated atlas motifs and geometric reflections calculate; individual damaged-glass fragments remain drawing choices.</summary>
    private static void MotherBrainRoom(Page p)
    {
        p.Fill(new(0, 4), new(32, 22), SceneBlank);
        p.Fill(new(28, 25), new(4, 1), BorderBlank);
        p.Fill(new(12, 26), new(2, 1), BorderBlank with { FlipHorizontally = true });
        for (int x = 0; x < 32; x += 2) p.Patch(new(checked((ushort)x), 2), new(2, 2), Tiled(0x1c8, 7));
        for (int side = 0; side < 2; side++)
        {
            p.Patch(new(checked((ushort)(2 + side * 8)), 4), new(2, 2), Tiled(0x1c8, 7));
            p.Fill(new(checked((ushort)(2 + side * 8)), 6), new(2, 2), Tiled(0x1e9, 7));
        }
        p.Fill(new(4, 4), new(6, 4), Tiled(0x1e8, 7));
        for (int y = 4; y < 8; y++) p.Patch(new(6, checked((ushort)y)), new(2, 1), Tiled(0x208, 7, mirroring: TileFlip.Vertical));
        p.Fill(new(4, 8), new(6, 1), Tiled(0x278, 7));
        for (int x = 4; x < 10; x += 2) p.Patch(new(checked((ushort)x), 9), new(2, 1), Tiled(0x268, 7));
        p.Patch(new(18, 4), new(2, 2), Tiled(0x1c8, 7, priority: true));
        p.Fill(new(18, 6), new(2, 2), Tiled(0x1e9, 7, priority: true));
        for (int x = 16; x <= 20; x += 4) p.Patch(new(checked((ushort)x), 6), new(2, 2), Tiled(0x228, 7, priority: true));
        p.Put(new(15, 7), Tiled(0x217, 7, priority: true, mirroring: TileFlip.Horizontal));
        p.Put(new(22, 7), Tiled(0x217, 7, priority: true));
        p.Patch(new(15, 8), new(2, 1), Tiled(0x218, 7, priority: true), TileFlip.Horizontal);
        p.Patch(new(21, 8), new(2, 1), Tiled(0x218, 7, priority: true));
        p.Patch(new(18, 8), new(2, 1), Tiled(0x1f8, 7, priority: true));
        p.Patch(new(18, 9), new(2, 1), Tiled(0x268, 7, priority: true));
        p.Patch(new(18, 10), new(2, 1), Tiled(0x206, 7, priority: true));

        // Three roof fragments form one half; their opposite half reflects exactly.
        p.Patch(new(2, 10), new(2, 2), Tiled(0x07c, 7));
        p.Patch(new(4, 10), new(2, 2), Tiled(0x05c, 7));
        p.Patch(new(6, 10), new(1, 2), Tiled(0x0c0, 7));
        p.Patch(new(7, 10), new(1, 2), Tiled(0x0c0, 7), TileFlip.Horizontal);
        p.Patch(new(8, 10), new(2, 2), Tiled(0x05c, 7), TileFlip.Horizontal);
        p.Patch(new(10, 10), new(2, 2), Tiled(0x07c, 7), TileFlip.Horizontal);
        p.Patch(new(2, 12), new(2, 3), Tiled(0x00c, 7));
        p.Patch(new(2, 15), new(2, 3), Tiled(0x00c, 7), TileFlip.Vertical);
        p.Patch(new(10, 12), new(2, 5), Tiled(0x00e, 7));
        p.Put(new(10, 17), Tiled(0x05e, 7));
        p.Patch(new(10, 18), new(2, 1), Tiled(0x06e, 7));
        for (int row = 0; row < 4; row++) p.Put(new(9, checked((ushort)(14 + row))), Tiled(0x03c + row % 2 * 16 + row / 2, 7));
        p.Put(new(2, 18), Tiled(0x08c, 7, mirroring: TileFlip.Vertical));
        p.Put(new(3, 18), Tiled(0x0e0, 7));
        p.Patch(new(2, 19), new(2, 1), Tiled(0x07c, 7, mirroring: TileFlip.Vertical));
        p.Patch(new(10, 19), new(2, 1), Tiled(0x07c, 7, mirroring: TileFlip.Vertical), TileFlip.Horizontal);
        p.Patch(new(5, 18), new(5, 1), Tiled(0x201, 7));
        p.Patch(new(4, 19), new(6, 1), Tiled(0x210, 7));

        p.Patch(new(0, 20), new(2, 2), Tiled(0x1e6, 7));
        p.Patch(new(12, 20), new(2, 2), Tiled(0x1e6, 7, priority: true), TileFlip.Horizontal);
        for (int side = 0; side < 2; side++)
        {
            p.Fill(new(checked((ushort)(2 + side * 8)), 20), new(2, 2), Tiled(0x1e9, 7));
            p.Fill(new(checked((ushort)(2 + side * 8)), 24), new(2, 2), Tiled(0x1e9, 7));
            p.Patch(new(checked((ushort)(0 + side * 8)), 24), new(2, 2), Tiled(0x1e6, 7), TileFlip.Vertical);
        }
        for (int x = 4; x < 10; x += 2)
        {
            p.Patch(new(checked((ushort)x), 20), new(2, 1), Tiled(0x268, 7, mirroring: TileFlip.Vertical));
            for (int y = 21; y < 24; y++) p.Patch(new(checked((ushort)x), checked((ushort)y)), new(2, 1), Tiled(0x208, 7));
        }
        for (int y = 22; y < 24; y++)
        {
            p.Patch(new(0, checked((ushort)y)), new(2, 1), Tiled(0x1f8, 7));
            p.Patch(new(12, checked((ushort)y)), new(2, 1), Tiled(0x1f8, 7, priority: true), TileFlip.Horizontal);
        }
        p.Patch(new(2, 22), new(2, 2), Tiled(0x1e6, 7));
        p.Patch(new(10, 22), new(2, 2), Tiled(0x1e6, 7), TileFlip.Horizontal);
        p.Patch(new(4, 24), new(2, 2), Tiled(0x1e6, 7), TileFlip.Horizontal | TileFlip.Vertical);
        p.Patch(new(12, 24), new(2, 2), Tiled(0x1e6, 7, priority: true), TileFlip.Horizontal | TileFlip.Vertical);
        p.Patch(new(6, 24), new(2, 2), Tiled(0x248, 7, priority: true));
        p.Patch(new(18, 17), new(2, 1), Tiled(0x206, 7, priority: true, mirroring: TileFlip.Vertical));
        p.Patch(new(18, 18), new(2, 1), Tiled(0x268, 7, priority: true, mirroring: TileFlip.Vertical));
        for (int y = 19; y < 22; y++) p.Patch(new(18, checked((ushort)y)), new(2, 1), Tiled(0x208, 7, priority: true));
        p.Patch(new(18, 22), new(2, 2), Tiled(0x248, 7, priority: true));
        p.Fill(new(18, 24), new(4, 2), Tiled(0x1e9, 7));

        p.Fill(new(13, 6), new(1, 4), Tiled(0x003, 1));
        p.Fill(new(13, 17), new(1, 3), Tiled(0x003, 1));
        p.Fill(new(21, 13), new(9, 2), Tiled(0x003, 1));
        for (int row = 0; row < 2; row++)
        {
            p.Fill(new(0, checked((ushort)(9 + row * 10))), new(2, 1), Tiled(0x08a, 1));
            for (int x = 21; x < 30; x += 2) p.Put(new(checked((ushort)x), checked((ushort)(9 + row * 10))), Tiled(0x08a, 1));
            p.Fill(new(15, checked((ushort)(4 + row * 19))), new(2, 1), Tiled(0x08b, 1));
        }
        p.Put(new(24, 18), Tiled(0x1ce, 7, priority: true));
        p.Fill(new(25, 18), new(4, 1), Tiled(0x1cf, 7, priority: true));
        p.Put(new(29, 18), Tiled(0x1de, 7, priority: true));
    }
    /// <summary>$96:FF14 page one: selected SR388 ceiling/floor rock motifs and egg ledge. Repeated four-cell rocks and reflected edge pairs share atlas patches; these are illustration tiles, not room collision geometry.</summary>
    private static void DiscoveryCave(Page p)
    {
        p.Fill(new(0, 4), new(32, 20), SceneBlank);
        for (int group = 0; group < 2; group++)
        {
            p.Patch(new(checked((ushort)(group * 4)), 4), new(4, 2), Tiled(0x1c0, 7));
            p.Patch(new(checked((ushort)(24 + group * 4)), 4), new(4, 2), Tiled(0x1c0, 7));
            p.Patch(new(checked((ushort)(group * 4)), 6), new(4, 2), Tiled(0x1c0, 7), TileFlip.Vertical);
        }
        p.Patch(new(8, 4), new(2, 2), Tiled(0x1c2, 7));
        CeilingPoint(p, new(10, 4), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(12, 4), IntroCeilingPointWidth.Narrow);
        p.Patch(new(14, 4), new(2, 2), Tiled(0x1c6, 7), TileFlip.Horizontal);
        p.Patch(new(16, 4), new(4, 2), Tiled(0x1c4, 7));
        CeilingPoint(p, new(20, 4), IntroCeilingPointWidth.Wide);
        p.Patch(new(22, 4), new(2, 2), Tiled(0x1c6, 7), TileFlip.Horizontal);
        p.Patch(new(8, 6), new(2, 2), Tiled(0x1c6, 7));
        p.Patch(new(24, 6), new(2, 2), Tiled(0x1c6, 7), TileFlip.Horizontal);
        p.Patch(new(26, 6), new(4, 2), Tiled(0x1c0, 7), TileFlip.Vertical);
        p.Patch(new(30, 6), new(2, 2), Tiled(0x1c0, 7), TileFlip.Vertical);
        p.Patch(new(0, 8), new(4, 2), Tiled(0x1c4, 7));
        CeilingPoint(p, new(4, 8), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(6, 8), IntroCeilingPointWidth.Narrow);
        CeilingPoint(p, new(26, 8), IntroCeilingPointWidth.Wide);
        CeilingPoint(p, new(28, 8), IntroCeilingPointWidth.Narrow);
        CeilingPoint(p, new(30, 8), IntroCeilingPointWidth.Wide);

        p.Patch(new(0, 20), new(4, 2), Tiled(0x1c4, 7), TileFlip.Vertical);
        p.Patch(new(7, 20), new(2, 2), Tiled(0x097, 7));
        p.Patch(new(9, 20), new(3, 1), Tiled(0x1e3, 7));
        p.Patch(new(17, 20), new(3, 1), Tiled(0x1e3, 7), TileFlip.Horizontal);
        p.Put(new(4, 21), Tiled(0x1d4, 7, mirroring: TileFlip.Vertical));
        p.Put(new(5, 21), Tiled(0x1d7, 7, mirroring: TileFlip.Vertical));
        p.Put(new(6, 21), Tiled(0x2e0, 7));
        p.Put(new(12, 21), Tiled(0x2e0, 7));
        p.Patch(new(9, 21), new(3, 1), Tiled(0x220, 7));
        p.Patch(new(15, 21), new(2, 1), Tiled(0x2f0, 7));
        p.Patch(new(17, 21), new(3, 1), Tiled(0x230, 7));
        p.Patch(new(20, 21), new(2, 1), Tiled(0x240, 7, priority: true));
        p.Put(new(22, 21), Tiled(0x250, 7, priority: true));
        p.Put(new(23, 21), Tiled(0x2e0, 7, priority: true));
        for (int group = 0; group < 6; group++)
            p.Patch(new(checked((ushort)(group * 4)), 22), new(4, 2), Tiled(0x1c0, 7, priority: group == 5));
        p.Patch(new(24, 22), new(4, 2), Tiled(0x1c4, 7, priority: true), TileFlip.Vertical);
        p.Patch(new(28, 22), new(2, 2), Tiled(0x1c6, 7, priority: true), TileFlip.Horizontal | TileFlip.Vertical);
        p.Patch(new(30, 22), new(2, 2), Tiled(0x1c4, 7, priority: true), TileFlip.Vertical);
        p.Fill(new(20, 24), new(12, 4), BorderBlank with { HighPriority = true });
    }

    /// <summary>$96:FF14 cave: two selected two-cell stalactite drawings. Their lower rows are repacked at $1E2/$200 and $1F2/blank instead of an atlas-row stride.</summary>
    private static void CeilingPoint(Page p, Position16<TilemapCells> cell, IntroCeilingPointWidth width)
    {
        BackgroundTile top = Tiled(width == IntroCeilingPointWidth.Narrow ? 0x1f0 : 0x1e0, 7);
        p.Patch(cell, new(2, 1), top);
        p.Put(cell + new Offset16<TilemapCells>(0, 1), top with { Tile = top.Tile + 2 });
        p.Put(cell + new Offset16<TilemapCells>(1, 1), width == IntroCeilingPointWidth.Narrow ? SceneBlank : Tiled(0x200, 7));
    }

    /// <summary>Writes one selected intro background page into its slice of the combined pages.</summary>
    private sealed class Page(BackgroundTile[] output, int page)
    {
        /// <summary>Each illustrated page has 32 rows and columns of cells.</summary>
        private static readonly Extent16<TilemapCells> PageSize = new(32, 32);

        /// <summary>Cells in one page.</summary>
        internal const int Cells = 32 * 32;

        /// <summary>Places a tile at the page-local cell.</summary>
        internal void Put(Position16<TilemapCells> cell, BackgroundTile tile) => output[page * Cells + PageSize.RowMajorIndex(cell)] = tile;
        /// <summary>Paints a rectangular run with one tile, used for backgrounds and blank regions.</summary>
        internal void Fill(Position16<TilemapCells> origin, Extent16<TilemapCells> size, BackgroundTile tile)
        {
            for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++) Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)), tile);
        }
        /// <summary>Copies a row-major atlas patch with a 16-tile source stride, mirroring each tile as well when reflected.</summary>
        internal void Patch(Position16<TilemapCells> origin, Extent16<TilemapCells> size, BackgroundTile first, TileFlip flips = TileFlip.None)
        {
            for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++)
            {
                int sx = (flips & TileFlip.Horizontal) != TileFlip.None ? size.X - 1 - column : column;
                int sy = (flips & TileFlip.Vertical) != TileFlip.None ? size.Y - 1 - row : row;
                Put(origin + new Offset16<TilemapCells>(checked((short)column), checked((short)row)),
                    (first with { Tile = first.Tile + sy * 16 + sx }).WithFlipsToggled(flips));
            }
        }
    }
}
