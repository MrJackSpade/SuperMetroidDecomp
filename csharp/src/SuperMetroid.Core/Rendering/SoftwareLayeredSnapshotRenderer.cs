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
                case FixedColorAddRenderLayer color:
                    for (int pixel = 0; pixel < output.Length; pixel++)
                    {
                        Rgba32 before = output[pixel];
                        output[pixel] = new(AddFixed(before.R, color.Red), AddFixed(before.G, color.Green),
                            AddFixed(before.B, color.Blue), 255);
                    }
                    break;
                case Mode7RenderLayer mode7:
                    Mode7RenderRegisters m = mode7.Registers;
                    SnesLayerCompositor.Composite(output, SnesMode7Renderer.RenderViewport(
                        memory.Vram, memory.Cgram, m.MatrixA, m.MatrixB, m.MatrixC, m.MatrixD,
                        m.CenterX, m.CenterY, m.HorizontalOffset, m.VerticalOffset,
                        fillOutsideWithCharacterZero: m.FillOutsideWithCharacterZero));
                    break;
                case ObjRenderLayer:
                    SnesLayerCompositor.Composite(output, objects.Pixels);
                    break;
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

    private static byte AddFixed(byte component, byte addend)
    {
        int reduced = (component * 31 + 127) / 255;
        int sum = Math.Min(31, reduced + addend);
        return (byte)((sum << 3) | (sum >> 2));
    }
}
