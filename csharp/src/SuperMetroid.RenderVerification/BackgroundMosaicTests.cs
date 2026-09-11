using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class BackgroundMosaicTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        for (int color = 1; color <= 15; color++) cgram.SetColor(color, (ushort)(color | (15 - color) << 5));
        // A repeating asymmetric pixel pattern makes both sampling axes observable.
        byte[] character = new byte[32];
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int color = 1 + (x + 2 * y) % 15;
            for (int plane = 0; plane < 4; plane++)
                character[(plane / 2) * 16 + y * 2 + plane % 2] |= (byte)(((color >> plane) & 1) << (7 - x));
        }
        vram.LoadBytes(0, character);
        for (int row = 0; row < 8; row++) vram.LoadBytes(32 + row * 2, new byte[] { 255, 0 });
        cgram.SetColor(241, 31 << 10);
        var oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 96; oam[1] = 64; oam[2] = 1; oam[3] = 0x3e;
        var memory = new PpuMemorySnapshot(vram.Bytes, cgram.Colors, oam, 1);
        ushort[] horizontal = Enumerable.Range(0, 192).Select(y => (ushort)(y * 3 + 7)).ToArray();
        ushort[] vertical = Enumerable.Range(0, 192).Select(y => (ushort)(65530 + y)).ToArray();
        int cases = 0;
        foreach (int size in Enumerable.Range(1, 16))
        foreach (int mode in Enumerable.Range(0, 4))
        {
            var registers = new OrdinaryGameplayRegisters(0, 0, 0, 0, 64, 32,
                SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, SnesPpuLayout.GameplayHudCharacterBaseWord,
                SnesMainScreenLayers.Obj | (mode == 2 ? SnesMainScreenLayers.None : SnesMainScreenLayers.Bg2) |
                    (mode == 3 ? SnesMainScreenLayers.Bg1 : SnesMainScreenLayers.None),
                Bg2Mosaic: new(size));
            var ordinary = new OrdinaryGameplayRenderLayer(registers, horizontal, vertical);
            RenderLayer layer = mode is 0 or 3 ? ordinary : new XrayGameplayRenderLayer(ordinary,
                Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false,
                mode == 2 ? SnesColorMathControl.Backdrop : SnesColorMathControl.None,
                mode == 2, 0, 0, 0, subscreenUsesBg2: mode == 2);
            var scene = new LayeredRenderSnapshot(memory, [layer], 0, 15);
            var packet = new RenderFrameSnapshot(new(++cases, 1, 0), scene);
            var actual = SoftwareLayeredSnapshotRenderer.Render(scene);
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                int expectedColor = 0;
                if (y >= 32)
                {
                    // Independent scalar oracle: choose the screen block, then
                    // current-row scroll, wrap within the authored 8x8 pattern.
                    int sx = (x / size * size + horizontal[y - 32]) & 7;
                    int sy = (y / size * size + 1 + vertical[y - 32]) & 7;
                    if (mode == 3) { sx = x & 7; sy = (y + 1) & 7; }
                    expectedColor = 1 + (sx + 2 * sy) % 15;
                }
                if (x is >= 96 and < 104 && y is >= 64 and < 72) expectedColor = 241;
                if (actual[y * 256 + x] != cgram.GetRgba(expectedColor))
                    throw new InvalidDataException($"Mosaic size {size}, mode {mode}: wrong pixel ({x},{y}).");
            }
            var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(restored, actual, SoftwareFrameSnapshotRenderer.Render(restored), "mosaic packet round trip");
            PixelComparison.Verify(restored, actual, renderer.RenderForReadback(restored), $"{device.Kind}: mosaic size {size}, mode {mode}");
        }
        Console.WriteLine($"{device.Kind}: {cases} mosaic frames match independent pixels and packet round trips (ordinary, main BG2, additive BG2; HUD/OBJ/BG1 unaffected).");
    }
}
