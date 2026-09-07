using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Reference implementation of explicit HUD/Mode-7/Mode-1 scanline bands.</summary>
internal static class SoftwareMode7GameplayRenderer
{
    internal static Rgba32[] Render(SoftwarePpuSnapshotMemory memory, Mode7GameplayRenderLayer layer, byte objectSelection)
    {
        const int width = SnesPpuLayout.ScreenWidthPixels;
        const int height = SnesPpuLayout.ScreenHeightPixels;
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(memory.Cgram, width * height);
        Mode7RenderRegisters m = layer.Registers;
        Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(memory.Vram, memory.Cgram,
            m.MatrixA, m.MatrixB, m.MatrixC, m.MatrixD, m.CenterX, m.CenterY,
            m.HorizontalOffset, m.VerticalOffset, fillOutsideWithCharacterZero: m.FillOutsideWithCharacterZero);
        // Resolve the winning OAM record once, before choosing its layer priority.
        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, objectSelection);
        int floorStart = layer.Floor?.FirstScanline ?? height;
        for (int i = layer.HudScanlines * width; i < floorStart * width; i++)
        {
            if (mode7[i].A != 0) output[i] = mode7[i];
            if (objects.Priorities[i] != SnesObjRenderer.TransparentPriority) output[i] = objects.Pixels[i];
        }
        if (layer.Floor is { } floor)
        {
            // Build each priority plane at full physical coordinates, then copy only
            // its band. Transparent floor pixels expose backdrop, never the Mode-7 map.
            var low = new Rgba32[output.Length];
            var high = new Rgba32[output.Length];
            SnesBgTilemapRenderer.Composite4BppViewport(low, memory.Vram, memory.Cgram,
                floor.TilemapWord, floor.CharacterWord, floor.HorizontalScroll, floor.VerticalScroll,
                width, height, floor.MapWidthTiles, floor.MapHeightTiles, priority: false);
            SnesBgTilemapRenderer.Composite4BppViewport(high, memory.Vram, memory.Cgram,
                floor.TilemapWord, floor.CharacterWord, floor.HorizontalScroll, floor.VerticalScroll,
                width, height, floor.MapWidthTiles, floor.MapHeightTiles, priority: true);
            for (int i = floorStart * width; i < output.Length; i++)
            {
                byte priority = objects.Priorities[i];
                if (priority is 0 or 1) output[i] = objects.Pixels[i];
                if (low[i].A != 0) output[i] = low[i];
                if (priority == 2) output[i] = objects.Pixels[i];
                if (high[i].A != 0) output[i] = high[i];
                if (priority == 3) output[i] = objects.Pixels[i];
            }
        }
        if (layer.HudScanlines != 0)
        {
            Rgba32[] hud = SnesBgTilemapRenderer.Render2Bpp(memory.Vram, memory.Cgram,
                layer.HudTilemapWord, layer.HudCharacterWord,
                rowCount: layer.HudScanlines / SnesPpuLayout.BackgroundTileSizePixels, transparentColorZero: false);
            hud.CopyTo(output, 0);
        }
        return output;
    }
}
