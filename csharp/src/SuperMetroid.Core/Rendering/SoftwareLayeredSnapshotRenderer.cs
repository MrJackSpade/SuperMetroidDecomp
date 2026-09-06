using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Reference composition from immutable state, without scene or ROM access.</summary>
public static class SoftwareLayeredSnapshotRenderer
{
    public static Rgba32[] Render(LayeredRenderSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var memory = new SoftwarePpuSnapshotMemory(snapshot.Memory);
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(memory.Cgram,
            SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels);
        // Resolve OAM precedence once. Drawing independently filtered OBJ lists would
        // wrongly allow a higher-numbered record to shine through the winning record.
        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram,
            memory.Cgram, snapshot.ObjectSelection);
        foreach (RenderLayer layer in snapshot.Layers)
        {
            switch (layer)
            {
                case ObjPriorityRenderLayer obj:
                    for (int pixel = 0; pixel < output.Length; pixel++)
                        if (objects.Priorities[pixel] == obj.Priority)
                            output[pixel] = objects.Pixels[pixel];
                    break;
                case Bg4BppRenderLayer bg:
                    SnesBgTilemapRenderer.Composite4BppViewport(output, memory.Vram, memory.Cgram,
                        bg.TilemapWord, bg.CharacterWord, bg.HorizontalScroll, bg.VerticalScroll,
                        SnesPpuLayout.ScreenWidthPixels, SnesPpuLayout.ScreenHeightPixels,
                        bg.MapWidthTiles, bg.MapHeightTiles, priority: bg.Priority);
                    break;
                case Bg2BppRenderLayer bg:
                    SnesLayerCompositor.Composite(output, SnesBgTilemapRenderer.Render2Bpp(
                        memory.Vram, memory.Cgram, bg.TilemapWord, bg.CharacterWord,
                        rowCount: bg.RowCount, transparentColorZero: true, priority: bg.Priority));
                    break;
                default:
                    throw new InvalidDataException($"Unsupported render layer {layer.GetType().Name}.");
            }
        }
        MasterBrightnessFilter.Apply(output, snapshot.Brightness);
        return output;
    }
}
