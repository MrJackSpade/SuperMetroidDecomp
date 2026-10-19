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
}
