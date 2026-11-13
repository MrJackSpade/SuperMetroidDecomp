using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Compares the land and ocean sky chunks' pages and the sky rows they select with the native bank-$88 tables (#1414):
/// each chunk shows the page its own table's entry names, the sky repeating its table's entries from the first, and
/// never a value beside the table (#1611).
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

    /// <summary>The land table's own entries: its words before the ocean table, which follows it directly.</summary>
    private const int LandEntries = (OceanTable - LandTable) / 2;

    /// <summary>The ocean table's own entries: its words from the start that name sky pages; the next word is code.</summary>
    private const int OceanEntries = 6;

    /// <summary>Checks the land sky table and the rows it selects.</summary>
    internal static void VerifyLandSkyChunkPointers(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Land));

    /// <summary>Checks the ocean sky table and the rows it selects.</summary>
    internal static void VerifyOceanSkyChunkPointers(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Ocean));

    /// <summary>
    /// Checks that every chunk down the sky's span shows the page its own table's entry names, repeating the table's
    /// entries from the first, then the row every vertical position shows.
    /// </summary>
    private static void VerifySkyChunkPointers(CartridgeImportAddressSpace rom, ScrollingSkyChunkTable table)
    {
        int tableAddress = table == ScrollingSkyChunkTable.Land ? LandTable : OceanTable;
        int entries = table == ScrollingSkyChunkTable.Land ? LandEntries : OceanEntries;
        ushort Original(int index) => ReadVerificationWord(rom, 0x880000 | ((tableAddress + 2 * index) & 0xffff));
        int NativePage(int index)
        {
            int offset = Original(index) - FirstPageWord;
            return offset >= 0 && offset % PageBytes == 0 && offset / PageBytes < RoomSkyTilemapFormat.PageCount ? offset / PageBytes : -1;
        }
        // The table's own entries each name a page; the ocean table's next word, the code beside it, names none.
        for (int entry = 0; entry < entries; entry++)
            AssertTrue(NativePage(entry) >= 0, $"{table} sky table entry {entry} names a sky page");
        if (table == ScrollingSkyChunkTable.Ocean)
            AssertEqual(-1, NativePage(entries), "The ocean sky table ends where the code beside it begins");

        // Every chunk shows its own table's entry, the entries repeating from the first; none reads beside the table.
        for (int chunk = 0; chunk < ScrollingSkyChunks.ChunkCount; chunk++)
            AssertEqual(NativePage(chunk % entries), ScrollingSkyChunks.PageOf(table, chunk),
                $"{table} sky chunk {chunk} shows its own table's entry {chunk % entries}");
        foreach (int chunk in new[] { int.MinValue, -1, ScrollingSkyChunks.ChunkCount, 255, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunks.PageOf(table, chunk),
                "Sky rejects chunks outside its span");
        foreach (int invalidTable in new[] { int.MinValue, 0, (int)ScrollingSkyChunkTable.Ocean + 1, int.MaxValue })
        foreach (int chunk in new[] { -1, 0, ScrollingSkyChunks.ChunkCount - 1, ScrollingSkyChunks.ChunkCount })
        {
            var exception = AssertThrows<ArgumentOutOfRangeException>(
                () => ScrollingSkyChunks.PageOf((ScrollingSkyChunkTable)invalidTable, chunk), "Unknown sky table rejects first");
            AssertEqual("table", exception.ParamName, "Table rejection precedes chunk handling");
        }

        // Each 8-pixel row reads its chunk's page at the row's offset (#1347), every row from its own table's entry.
        SkyTilemap rows = RoomSkyTilemapFixtures.Numbered().Tilemap(table);
        for (int row = 0; row < SkyTilemap.Rows; row++)
        {
            int page = NativePage(row / 32 % entries);
            AssertEqual(page, rows.RowPage(row), $"{table} sky row {row} shows its own table's page");
            for (int column = 0; column < SkyTilemap.Columns; column++)
                AssertEqual(RoomSkyTilemapFixtures.NumberedWord(page, row % 32 * 32 + column), rows.Tiles[row * SkyTilemap.Columns + column].PackedWord,
                    $"{table} sky row {row} column {column} reads its own table's page");
        }
    }
}
