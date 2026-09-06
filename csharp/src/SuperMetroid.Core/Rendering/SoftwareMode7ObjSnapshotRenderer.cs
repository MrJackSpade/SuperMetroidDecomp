using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Observational reference consumer; all scratch state belongs to this call.</summary>
public static class SoftwareMode7ObjSnapshotRenderer
{
    public static Rgba32[] Render(Mode7ObjRenderSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        // Existing raster kernels consume hardware-model readers. Rehydrate private
        // scratch models during boundary extraction; never return them to simulation.
        // A later direct-span consumer can eliminate these copies without changing the
        // packet or requiring GPU consumers to construct hardware objects.
        var memory = new SoftwarePpuSnapshotMemory(snapshot.Memory);
        SnesVram vram = memory.Vram;
        SnesCgram cgram = memory.Cgram;
        OamBuffer oam = memory.Oam;

        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram,
            SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels);
        if (snapshot.Background is { } bg)
        {
            SnesLayerCompositor.Composite(pixels, SnesMode7Renderer.RenderViewport(vram, cgram,
                bg.MatrixA, bg.MatrixB, bg.MatrixC, bg.MatrixD, bg.CenterX, bg.CenterY,
                bg.HorizontalOffset, bg.VerticalOffset,
                fillOutsideWithCharacterZero: bg.FillOutsideWithCharacterZero));
        }
        SnesLayerCompositor.Composite(pixels,
            SnesObjRenderer.Render(oam, vram, cgram, snapshot.ObjectSelection));
        MasterBrightnessFilter.Apply(pixels, snapshot.Brightness);
        return pixels;
    }
}
