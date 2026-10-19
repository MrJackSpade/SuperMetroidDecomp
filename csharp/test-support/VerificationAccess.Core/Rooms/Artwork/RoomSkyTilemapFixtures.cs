namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Constructed scrolling-sky pages for sky tests (#1347).</summary>
internal static class RoomSkyTilemapFixtures
{
    /// <summary>The word every <see cref="Numbered"/> page holds at a cell: its page and the cell's index in it.</summary>
    internal static ushort NumberedWord(int page, int cell) => (ushort)(page * RoomBackgroundTilemapFormat.CellsPerPage + cell);

    /// <summary>Seven pages whose every tile word names its own page and cell, so a sky row shows where it was read.</summary>
    internal static RoomSkyTilemapCatalog Numbered()
    {
        var pages = new RoomBackgroundTilemapAtlas[RoomSkyTilemapFormat.PageCount];
        for (int page = 0; page < pages.Length; page++)
        {
            var bytes = new byte[RoomSkyTilemapFormat.PageByteCount];
            for (int cell = 0; cell < bytes.Length / sizeof(ushort); cell++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(cell * 2), NumberedWord(page, cell));
            pages[page] = new RoomBackgroundTilemapAtlas(bytes);
        }
        return new RoomSkyTilemapCatalog(pages);
    }
}
