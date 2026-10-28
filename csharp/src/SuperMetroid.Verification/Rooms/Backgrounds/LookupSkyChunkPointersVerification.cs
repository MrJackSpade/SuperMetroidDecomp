using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Compares the land and ocean sky chunks' pages and the sky rows they select with the native bank-$88 tables (#1414):
/// each chunk shows the page its native word names, and a chunk whose word names no page shows nothing.
/// </summary>
internal static class LookupSkyChunkPointersVerification
{
    /// <summary><c>$88:AD9C</c>, the land sky chunk pointer table the scrolling-sky room main reads.</summary>
    private const int LandTable = 0x88ad9c;

    /// <summary><c>$88:ADA6</c>, the ocean sky chunk pointer table <c>RoomMainAsm_ScrollingSkyOcean</c> passes.</summary>
    private const int OceanTable = 0x88ada6;

    /// <summary><c>$8A:B180</c>, the first sky page a chunk word can name.</summary>
    private const int FirstPageWord = 0xb180;

    /// <summary>The bytes of one sky page: the stride between the words that name consecutive pages.</summary>
    private const int PageBytes = 0x800;

    /// <summary>Checks the land sky table and the rows it selects.</summary>
    internal static void VerifyLandSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Land));

    /// <summary>Checks the ocean sky table and the rows it selects.</summary>
    internal static void VerifyOceanSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Ocean));

    /// <summary>
    /// Checks the page every reachable chunk index names, including the adjacent data and code the ocean table and
    /// index 255 read, then the row every vertical position shows.
    /// </summary>
    private static void VerifySkyChunkPointers(SuperMetroidAddressSpace rom, ScrollingSkyChunkTable table)
    {
        int tableAddress = table == ScrollingSkyChunkTable.Land ? LandTable : OceanTable;
        ushort Original(int index) => ReadVerificationWord(rom, 0x880000 | ((tableAddress + 2 * index) & 0xffff));
        int NativePage(int index)
        {
            int offset = Original(index) - FirstPageWord;
            return offset >= 0 && offset % PageBytes == 0 && offset / PageBytes < RoomSkyTilemapFormat.PageCount ? offset / PageBytes : -1;
        }
        for (int index = 0; index <= 256; index++)
        {
            if (index is < 9 or 255)
            {
                bool selects = ScrollingSkyChunks.TryGetPage(table, index, out int page);
                AssertEqual(NativePage(index), selects ? page : -1,
                    $"Sky chunk {index} shows the page its native word names, or none for adjacent data/code");
            }
            else
                AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunks.TryGetPage(table, index, out _),
                    "Sky rejects unreachable indices");
        }
        foreach (int index in new[] { int.MinValue, -1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunks.TryGetPage(table, index, out _),
                "Sky int index endpoints reject");
        foreach (int invalidTable in new[] { int.MinValue, 0, (int)ScrollingSkyChunkTable.Ocean + 1, int.MaxValue })
        foreach (int index in new[] { -1, 0, 8, 255 })
        {
            var exception = AssertThrows<ArgumentOutOfRangeException>(
                () => ScrollingSkyChunks.TryGetPage((ScrollingSkyChunkTable)invalidTable, index, out _), "Unknown sky table rejects first");
            AssertEqual("table", exception.ParamName, "Table rejection precedes index handling");
        }

        // Each 8-pixel row reads its chunk's page at the row's offset (#1347); a chunk whose native word names no page
        // (the ocean table's adjacent code) shows nothing.
        SkyTilemap rows = RoomSkyTilemapFixtures.Numbered().Tilemap(table);
        for (int row = 0; row < SkyTilemap.Rows; row++)
        {
            int page = NativePage(row / 32);
            AssertEqual(page >= 0, rows.RowShown(row), $"Sky row {row} shows exactly when its native chunk names a page");
            for (int column = 0; page >= 0 && column < SkyTilemap.Columns; column++)
                AssertEqual(RoomSkyTilemapFixtures.NumberedWord(page, row % 32 * 32 + column), rows.Tiles[row * SkyTilemap.Columns + column].PackedWord,
                    $"Sky row {row} column {column} reads its native page");
        }
    }
}
