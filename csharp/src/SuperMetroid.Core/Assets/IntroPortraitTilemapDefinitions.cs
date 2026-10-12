using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>$97:88CC Samus-head BG2 composition, copied by $8B:A4A5-A4B3 to VRAM word $4800. Atlas runs and repeated cells calculate; selected drawing layout remains explicit.</summary>
internal static class IntroPortraitTilemapDefinitions
{
    /// <summary>$97:88CC: selected portrait atlas block begins at $300; its sixteen-cell row stride comes from the source atlas.</summary>
    private const int AtlasOrigin = 0x300;
    /// <summary>$97:88CC: the main sixteen-column portrait block starts at page column eight, centered in the 32-column BG page.</summary>
    private const int AtlasSide = 16;
    /// <summary>$97:88CC: chosen portrait top row five.</summary>
    private const int Top = 5;
    /// <summary>$97:88CC: chosen blank tile $3FE and palette three, no priority or reflections; RGB paint and tile pixels are separate.</summary>
    private const ushort Blank = 0x0ffe;
    private const ushort Style = 3 << 10;

    internal static byte[] Compile()
    {
        const int columns = IntroCinematicArtworkFormat.TileColumns;
        int left = (columns - AtlasSide) / 2;
        var output = new byte[IntroCinematicArtworkFormat.BackgroundPageByteCount];
        for (int cell = 0; cell < output.Length / 2; cell++)
            BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(cell * 2), Blank);
        void Put(int x, int y, int tile) => BinaryPrimitives.WriteUInt16LittleEndian(
            output.AsSpan((y * columns + x) * 2), (ushort)(Style | tile));
        void Run(int x, int y, int tile, int count)
        {
            for (int column = 0; column < count; column++) Put(x + column, y, tile + column);
        }
        for (int row = 0; row < AtlasSide; row++)
        for (int column = 0; column < AtlasSide; column++)
            if ((Silhouette(row) & (1 << column)) != 0)
                Put(left + column, Top + row, AtlasOrigin + row * AtlasSide + column);

        // Left shoulder fragments reuse free atlas cells around the main head drawing.
        Run(4, 12, AtlasOrigin + 11, 2);
        Put(4, 13, AtlasOrigin + 13);
        for (int row = 0; row < 2; row++) Run(5, 13 + row, AtlasOrigin + 26 + row * AtlasSide, 3);
        Run(5, 15, AtlasOrigin, 3);
        Run(6, 16, AtlasOrigin + 17, 2);
        Put(7, 17, AtlasOrigin + 49);
        Put(7, 18, AtlasOrigin + 3);

        // The near-right fin follows one atlas column, while the far-right tip
        // stitches two vertically packed two-cell strips side by side.
        for (int row = 0; row < 3; row++) Put(24, 11 + row, AtlasOrigin + 141 + row * AtlasSide);
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
            Put(26 + column, 13 + row, AtlasOrigin + 191 + (column * 2 + row) * AtlasSide);

        // Seven chin cells occupy two source rows because the atlas keeps other
        // head fragments in the adjacent cells; displayed runs remain contiguous.
        Run(13, 21, AtlasOrigin + 89, 4);
        Run(17, 21, AtlasOrigin + 105, 3);
        return output;
    }

    /// <summary>$97:88CC: selected one-bit occupancy of the main sixteen-by-sixteen atlas block. Repeated adjacent rows share their silhouette; every occupied tile identity follows the same atlas position formula. This mask describes drawing placement, not collision or a sampled numeric curve.</summary>
    private static ushort Silhouette(int row) => row switch
    {
        0 => 0x07f0,
        1 => 0x03f8,
        2 or 3 => 0x03fc,
        4 => 0x03fe,
        5 => 0xc1fe,
        6 => 0xf0ff,
        7 => 0xfcff,
        8 => 0x9fff,
        9 or 10 => 0xdfff,
        11 => 0x6fff,
        12 => 0x67ff,
        13 => 0x73ff,
        14 => 0x3bff,
        15 => 0x1ff8,
        _ => throw new ArgumentOutOfRangeException(nameof(row)),
    };
}
