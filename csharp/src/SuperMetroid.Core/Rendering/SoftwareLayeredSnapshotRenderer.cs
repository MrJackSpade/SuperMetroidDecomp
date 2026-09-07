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
        bool usesObjInsertion = false;
        foreach (RenderLayer layer in snapshot.Layers)
            usesObjInsertion |= layer is ObjRenderLayer or ObjPriorityRenderLayer;
        ResolvedObjFrame objects = usesObjInsertion
            ? SnesObjRenderer.RenderResolved(memory.Oam, memory.Vram, memory.Cgram, snapshot.ObjectSelection)
            : default;
        foreach (RenderLayer layer in snapshot.Layers)
        {
            switch (layer)
            {
                case OrdinaryGameplayRenderLayer gameplay:
                    OrdinaryGameplayRegisters r = gameplay.Registers;
                    output = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
                        memory.Vram, memory.Cgram, memory.Oam, r.Bg1X, r.Bg1Y, r.Bg2X, r.Bg2Y,
                        gameplay.HorizontalScrolls.IsEmpty ? null : gameplay.HorizontalScrolls.ToArray(),
                        gameplay.VerticalScrolls.IsEmpty ? null : gameplay.VerticalScrolls.ToArray(),
                        r.Bg2WidthTiles, r.Bg2HeightTiles, r.Bg2TilemapWord,
                        r.Bg1CharacterWord, r.Bg2CharacterWord, r.HudCharacterWord,
                        snapshot.ObjectSelection, r.MainScreenLayers);
                    break;
                case Bg2BppViewportRenderLayer bg:
                    Rgba32[] plane = SnesBgTilemapRenderer.Render2Bpp(memory.Vram, memory.Cgram,
                        bg.TilemapWord, bg.CharacterWord, rowCount: 32,
                        transparentColorZero: bg.TransparentColorZero, priority: bg.Priority);
                    for (int y = 0; y < SnesPpuLayout.ScreenHeightPixels; y++)
                        SnesLayerCompositor.Composite(output.AsSpan(y * SnesPpuLayout.ScreenWidthPixels,
                            SnesPpuLayout.ScreenWidthPixels), plane.AsSpan(
                            ((y + bg.VerticalScroll) & 255) * SnesPpuLayout.ScreenWidthPixels,
                            SnesPpuLayout.ScreenWidthPixels));
                    break;
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
