using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Observational reference consumer; all scratch state belongs to this call.</summary>
public static class SoftwareMode7ObjSnapshotRenderer
{
    public static Rgba32[] Render(Mode7ObjRenderSnapshot snapshot, Rgba32[]? outputBuffer = null)
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
            SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels, outputBuffer);
        if (snapshot.Background is { } bg)
        {
            // The kernel preserves destination pixels for transparent samples, so it
            // can project directly over the backdrop without allocating a BG plane.
            SnesMode7Renderer.CompositeViewport(pixels, vram, cgram,
                bg.MatrixA, bg.MatrixB, bg.MatrixC, bg.MatrixD, bg.CenterX, bg.CenterY,
                bg.HorizontalOffset, bg.VerticalOffset,
                fillOutsideWithCharacterZero: bg.FillOutsideWithCharacterZero, wrapOutsideMap: bg.WrapOutsideMap);
        }
        if (snapshot.Gradient.IsEmpty)
            SnesObjRenderer.CompositeUnfiltered(oam, vram, cgram, snapshot.ObjectSelection, pixels);
        else
        {
            // Only winning palette identity is needed by title color math. Rent this
            // small plane per call; the compositor initializes every entry before use.
            byte[] rented = System.Buffers.ArrayPool<byte>.Shared.Rent(pixels.Length);
            try
            {
                Span<byte> palettes = rented.AsSpan(0, pixels.Length);
                SnesObjRenderer.CompositeUnfiltered(oam, vram, cgram, snapshot.ObjectSelection, pixels, palettes);
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = TitleGradientColorMath.Apply(pixels[i], snapshot.Gradient[i / 256],
                        palettes[i] == byte.MaxValue ? null : palettes[i]);
            }
            finally { System.Buffers.ArrayPool<byte>.Shared.Return(rented); }
        }
        MasterBrightnessFilter.Apply(pixels, snapshot.Brightness);
        return pixels;
    }
}
