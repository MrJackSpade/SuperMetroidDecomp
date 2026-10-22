using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Mode 7 maps for render tests that set the map up in VRAM (#1349).</summary>
internal static class Mode7MapFixtures
{
    /// <summary>The map in the low bytes of a test VRAM image's first $4000 words.</summary>
    [PpuLayoutBoundary("test fixture: reads the Mode 7 map a render test wrote into VRAM")]
    internal static Mode7MapSnapshot FromVram(ReadOnlySpan<byte> vram)
    {
        var cells = new byte[Mode7Map.Cells];
        for (int cell = 0; cell < cells.Length; cell++)
            cells[cell] = vram[cell * 2];
        return new Mode7MapSnapshot(cells, default);
    }

    /// <summary><paramref name="map"/> drawn from seeded tile pictures (#1392).</summary>
    internal static Mode7MapSnapshot WithSeededTiles(Mode7MapSnapshot map, Random random, Bgr555[] scene) =>
        new(Ensure.NotNull(map).Cells, map.Tileset, RoomBackgroundsFixtures.SeededMode7Tiles(random, scene));
}
