using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>Compares compiled land and ocean sky chunk pointers and the sky rows they select with native bank-$88 tables.</summary>
internal static class LookupSkyChunkPointersVerification
{
    /// <summary>Checks the land sky table and the rows it selects.</summary>
    internal static void VerifyLandSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Land));

    /// <summary>Checks the ocean sky table and the rows it selects.</summary>
    internal static void VerifyOceanSkyChunkPointers(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySkyChunkPointers), () => VerifySkyChunkPointers(rom, ScrollingSkyChunkTable.Ocean));

    /// <summary>Checks reachable and adjacent pointer indices, then the row every vertical position shows.</summary>
    private static void VerifySkyChunkPointers(SuperMetroidAddressSpace rom, ScrollingSkyChunkTable table)
    {
        ushort Original(int index) => ReadVerificationWord(rom, 0x880000 | (((int)table + 2 * index) & 0xffff));
        for (int index = 0; index <= 256; index++)
        {
            if (index is < 9 or 255)
                AssertEqual(Original(index), ScrollingSkyChunkDefinitions.Get(table, index),
                    "Sky original pointer including adjacent data/code");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunkDefinitions.Get(table, index),
                    "Sky rejects unreachable indices");
        }
        foreach (int index in new[] { int.MinValue, -1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyChunkDefinitions.Get(table, index),
                "Sky int index endpoints reject");
        foreach (int invalidTable in new[] { int.MinValue, 0, (int)table - 1, (int)table + 1, int.MaxValue })
        foreach (int index in new[] { -1, 0, 8, 255 })
        {
            var exception = AssertThrows<ArgumentOutOfRangeException>(
                () => ScrollingSkyChunkDefinitions.Get((ScrollingSkyChunkTable)invalidTable, index), "Unknown sky table rejects first");
            AssertEqual("pointerTable", exception.ParamName, "Table rejection precedes index handling");
        }

        // Each 8-pixel row reads its chunk's page at the row's offset (#1347); a chunk whose native word names no page
        // (the ocean table's adjacent code) shows nothing.
        SkyTilemap rows = RoomSkyTilemapFixtures.Numbered().Tilemap(table);
        for (int row = 0; row < SkyTilemap.Rows; row++)
        {
            int offset = Original(row / 32) - 0xb180;
            bool page = offset >= 0 && offset % 0x800 == 0 && offset / 0x800 < 7;
            AssertEqual(page, rows.RowShown(row), $"Sky row {row} shows exactly when its native chunk names a page");
            for (int column = 0; page && column < SkyTilemap.Columns; column++)
                AssertEqual(RoomSkyTilemapFixtures.NumberedWord(offset / 0x800, row % 32 * 32 + column), rows.Words[row * SkyTilemap.Columns + column],
                    $"Sky row {row} column {column} reads its native page");
        }
    }
}
