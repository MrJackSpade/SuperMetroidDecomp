namespace SuperMetroid.Core.Rendering;

/// <summary>Reference implementation of explicit HUD/Mode-7/Mode-1 scanline bands.</summary>
internal static class SoftwareMode7GameplayRenderer
{
    /// <summary>Composes backdrop, Mode-7 room, priority-ordered floor and OBJ bands, then writes the opaque BG3 HUD at physical scanlines.</summary>
    internal static Rgba32[] Render(SoftwarePpuSnapshotMemory memory, RenderScratch scratch,
        Mode7GameplayRenderLayer layer, Rgba32[]? outputBuffer = null)
    {
        const int width = SnesPpuLayout.ScreenWidthPixels;
        const int height = SnesPpuLayout.ScreenHeightPixels;
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(memory.Cgram, width * height, outputBuffer);
        Rgba32[] mode7 = scratch.Colors(width * height);
        SnesMode7Renderer.CompositeViewport(mode7, memory.Cgram, layer.Transform, layer.Map);
        // Resolve the winning OAM record once, before choosing its layer priority.
        ResolvedObjFrame objects = scratch.ResolveObjects(memory);
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
            var low = scratch.Colors(output.Length);
            var high = scratch.Colors(output.Length);
            var viewport = new BgViewport { Scroll = floor.Scroll.AsBgPlaneScroll(), Size = SnesPpuLayout.ScreenSize, TilemapSize = floor.MapSize };
            // The floor is the room's layer-2 plane when it streams (#1346), otherwise the BG2 tilemap.
            if (floor.Room is { } room)
            {
                SnesBgTilemapRenderer.Composite4BppViewport(low, memory.Vram, memory.Cgram, room, floor.CharacterWord, viewport, priority: false);
                SnesBgTilemapRenderer.Composite4BppViewport(high, memory.Vram, memory.Cgram, room, floor.CharacterWord, viewport, priority: true);
            }
            else
            {
                var addresses = new BgVramAddresses { TilemapBaseWord = floor.TilemapWord, CharacterBaseWord = floor.CharacterWord };
                SnesBgTilemapRenderer.Composite4BppViewport(low, memory.Vram, memory.Cgram, addresses, viewport, priority: false);
                SnesBgTilemapRenderer.Composite4BppViewport(high, memory.Vram, memory.Cgram, addresses, viewport, priority: true);
            }
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
            // BG3 writes every HUD pixel (opaque color zero), from its pictures once resolved, so it renders straight into the frame.
            int hudRows = layer.HudScanlines / SnesPpuLayout.BackgroundTileSizePixels;
            SnesBgTilemapRenderer.Render2Bpp(output.AsSpan(0, width * hudRows * SnesPpuLayout.BackgroundTileSizePixels), memory.Cgram, layer.Hud, hudRows);
        }
        return output;
    }
}
