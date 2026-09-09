using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Check the ROM-authored words, not just the compositor's interpretation of them.</summary>
    private static int VerifyDraygonBodyTilemap(SuperMetroidRuntime runtime)
    {
        int total = 0;
        foreach (var part in runtime.Enemies.Slots)
            total += VerifyDraygonPartTilemap(runtime, part);
        return total;
    }

    private static int VerifyDraygonPartTilemap(SuperMetroidRuntime runtime, RoomEnemySlot body)
    {
        if (!body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)) return 0;
        int bank = body.Definition.Bank << 16;
        int Word(int pointer) => runtime.AddressSpace.ReadByte(bank | (pointer & 65535)) |
            runtime.AddressSpace.ReadByte(bank | ((pointer + 1) & 65535)) << 8;
        int components = Word(body.SpritemapPointer);
        if (components > 32) throw new InvalidOperationException("Unexpected Draygon extended map size.");
        int checkedWords = 0;
        for (int component = 0; component < components; component++)
        {
            int stream = Word(body.SpritemapPointer + 2 + component * 8 + 4);
            if (Word(stream) != DraygonTilemapAuditDefinitions.ExtendedTilemapMarker) continue;
            int cursor = stream + 2;
            for (int command = 0; ; command++)
            {
                int destination = Word(cursor);
                if (destination == DraygonTilemapAuditDefinitions.StreamEnd) break;
                if (command >= 128) throw new InvalidOperationException("Unterminated native Draygon map.");
                int count = Word(cursor + 2);
                int first = (destination - DraygonTilemapAuditDefinitions.WorkingRamBase) / 2;
                if (count <= 0 || first < 0 || first + count > 2048)
                    throw new InvalidOperationException("Native Draygon map exceeds workspace.");
                for (int i = 0; i < count; i++)
                {
                    int expected = Word(cursor + 4 + i * 2);
                    int actual = runtime.Vram.ReadWord(SnesPpuLayout.GameplayBg2TilemapWord + first + i);
                    if (actual != expected)
                        throw new InvalidOperationException($"Draygon map ${stream:X4}, word {i}: ROM ${expected:X4}, VRAM ${actual:X4}.");
                    checkedWords++;
                }
                cursor += 4 + count * 2;
            }
        }
        return checkedWords;
    }

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

internal static class DraygonTilemapAuditDefinitions
{
    /// <summary>$A0:96CA ProcessExtendedTilemap writes into tilemap_stuff at $7E:2000.</summary>
    internal const int WorkingRamBase = 0x2000;
    /// <summary>Native extended component header selecting a BG tilemap command stream.</summary>
    internal const int ExtendedTilemapMarker = 0xfffe;
    /// <summary>Native destination sentinel terminating ProcessExtendedTilemap.</summary>
    internal const int StreamEnd = 0xffff;
}
