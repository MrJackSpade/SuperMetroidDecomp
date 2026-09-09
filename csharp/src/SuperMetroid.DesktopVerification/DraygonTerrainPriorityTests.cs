using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>
    /// #385: isolate real encounter BG1/BG2 pixels and check their overlap against
    /// the native Mode-1 ladder (upstream-sm/src/snes/ppu.c layersPerMode).
    /// This tests composition, not whether the captured tilemap matches a CPU replay.
    /// </summary>
    private static (long Terrain, long Body) VerifyDraygonTerrainPriority(LayeredRenderSnapshot capture)
    {
        var layer = (OrdinaryGameplayRenderLayer)capture.Layers[0];
        var r = layer.Registers;
        if (!layer.HorizontalScrolls.IsEmpty || !layer.VerticalScrolls.IsEmpty ||
            r.MainScreenWindowMask != SnesMainScreenLayers.None)
            throw new InvalidOperationException("Draygon priority audit requires the ordinary BG scroll path.");
        var isolated = new OrdinaryGameplayRenderLayer(r with
            { MainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 });
        var actual = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
            capture.Memory, new RenderLayer[] { isolated }, capture.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness));
        long terrain = 0, body = 0;
        for (int y = Math.Max(32, r.Bg2FirstScanline); y < Math.Min(224, r.Bg2EndScanline); y++)
            for (int x = 0; x < 256; x++)
            {
                var a = ReadDraygonAuditPixel(capture.Memory, SnesPpuLayout.GameplayBg1TilemapWord,
                    r.Bg1CharacterWord, 64, 32, x + r.Bg1X, y + r.Bg1Y);
                var b = ReadDraygonAuditPixel(capture.Memory, r.Bg2TilemapWord,
                    r.Bg2CharacterWord, r.Bg2WidthTiles, r.Bg2HeightTiles, x + r.Bg2X, y + r.Bg2Y);
                // With OBJ/BG3 excluded, front-to-back is BG1 high, BG2 high,
                // BG1 low, BG2 low. Color zero is transparent on either plane.
                bool terrainWins = a.Opaque && (!b.Opaque || a.High || !b.High);
                int palette = terrainWins ? a.Palette : b.Opaque ? b.Palette : 0;
                Rgba32 expected = SnesGraphics.DecodeBgr555Color(capture.Memory.Cgram[palette]);
                if (actual[y * 256 + x] != expected)
                    throw new InvalidOperationException($"Draygon BG priority mismatch at ({x},{y}): terrain high={a.High}, body high={b.High}.");
                if (a.Opaque && b.Opaque)
                {
                    if (terrainWins) terrain++; else body++;
                }
            }
        return (terrain, body);
    }

    // Deliberately decode bitplanes here instead of calling the production sampler:
    // the assertion must detect a fused-renderer priority/addressing regression.
    private static (bool Opaque, bool High, int Palette) ReadDraygonAuditPixel(
        PpuMemorySnapshot memory, int map, int characters, int width, int height, int x, int y)
    {
        x &= width * 8 - 1;
        y &= height * 8 - 1;
        int tx = x / 8, ty = y / 8;
        int word = map + ((ty / 32) * (width / 32) + tx / 32) * 1024 + ty % 32 * 32 + tx % 32;
        int address = (word & 32767) * 2;
        int entry = memory.Vram[address] | memory.Vram[address + 1] << 8;
        int px = (entry & 0x4000) != 0 ? 7 - x % 8 : x % 8;
        int py = (entry & 0x8000) != 0 ? 7 - y % 8 : y % 8;
        int start = (characters + (entry & 1023) * 16) * 2 + py * 2;
        int color = 0;
        for (int plane = 0; plane < 4; plane++)
            color |= ((memory.Vram[(start + plane / 2 * 16 + plane % 2) & 65535] >> (7 - px)) & 1) << plane;
        return (color != 0, (entry & 0x2000) != 0, (entry >> 10 & 7) * 16 + color);
    }
}
