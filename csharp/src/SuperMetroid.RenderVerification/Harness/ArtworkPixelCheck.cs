using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.RenderVerification.Samus;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.Harness;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Exact indexed-pixel oracle plus backend comparison for disk-authored replacements.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry")]
internal sealed class ArtworkPixelCheck(D3D11RenderDevice device, D3D11FrameRenderer renderer)
{
    /// <summary>Gives each stock or edited render packet a fresh frame identity.</summary>
    private long sequence;
    /// <summary>Counts backend readbacks compared with the WARP reference's pixels.</summary>
    internal int Comparisons { get; private set; }
    /// <summary>Counts visible pixels changed by the indexed artwork substitution.</summary>
    internal long ChangedPixels { get; private set; }
    /// <summary>Distinct test colors let the oracle infer palette indices from rendered pixels.</summary>
    private readonly SnesCgram colors = MakeColors();

    /// <summary>Checks edited VRAM against exact index substitution and verifies stock and edited backend output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal void Pair(SnesVram stock, SnesVram edited, OamBuffer oam, string context)
    {
        DisplayPacket a = Packet(stock), b = Packet(edited);
        RgbaPixel[] first = DisplayRendering.Render(a);
        RgbaPixel[] second = DisplayRendering.Render(b);
        var substitutions = Enumerable.Range(0, 16).ToDictionary(
            index => colors.GetRgba(index), index => colors.GetRgba(InstalledSamusArtworkFixture.Remap((byte)index)));
        for (int pixel = 0; pixel < first.Length; pixel++)
        {
            // The fixture changes every nontransparent index to its successor.
            // This independent oracle catches wrong tile planes, palette indices,
            // crop/flip mistakes and edits which never reach visible output.
            if (!substitutions.TryGetValue(first[pixel], out RgbaPixel expected) || second[pixel] != expected)
                throw new InvalidOperationException($"{context}: PNG index substitution differs at " +
                    $"({pixel % a.Size.X},{pixel / a.Size.X}): {first[pixel]} -> {second[pixel]}.");
            if (first[pixel] != second[pixel]) ChangedPixels++;
        }
        Compare(a, first, context + " stock");
        Compare(b, second, context + " replacement");
        // Reuse the immutable stock packet after the replacement upload. This also
        // detects stale renderer caches keyed only by reused VRAM destinations.
        Compare(a, first, context + " retained stock after replacement");

        DisplayPacket Packet(SnesVram vram) => new(new(++sequence, 1, 0),
            new LayeredScene(FramePaintConstants.CaptureFrontend(vram, colors, oam, 3),
                [new SpriteRenderLayer()], 15));
    }

    /// <summary>Reads a GPU frame and compares its pixels with the WARP reference's.</summary>
    private void Compare(DisplayPacket frame, RgbaPixel[] expected, string context)
    {
        PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: {context}");
        Comparisons++;
    }

    /// <summary>Builds distinct index colors across all CGRAM rows for exact artwork comparison.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native PPU geometry")]
    private static SnesCgram MakeColors()
    {
        var result = new SnesCgram();
        // All palette rows deliberately share sixteen distinct colors. Sprite
        // palette selection remains native, while the pixel-index oracle is exact.
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            result.SetColor(index, new RgbColor(index % 16 + 1, 7, 13));
        return result;
    }
}
