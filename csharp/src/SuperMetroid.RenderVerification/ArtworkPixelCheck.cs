using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Exact indexed-pixel oracle plus backend comparison for disk-authored replacements.</summary>
internal sealed class ArtworkPixelCheck(D3D11RenderDevice device, D3D11FrameRenderer renderer)
{
    private long sequence;
    internal int Comparisons { get; private set; }
    internal long ChangedPixels { get; private set; }
    private readonly SnesCgram colors = MakeColors();

    internal void Pair(SnesVram stock, SnesVram edited, OamBuffer oam, string context)
    {
        RenderFrameSnapshot a = Packet(stock), b = Packet(edited);
        Rgba32[] first = SoftwareFrameSnapshotRenderer.Render(a);
        Rgba32[] second = SoftwareFrameSnapshotRenderer.Render(b);
        var substitutions = Enumerable.Range(0, 16).ToDictionary(
            index => colors.GetRgba(index), index => colors.GetRgba(InstalledSamusArtworkFixture.Remap((byte)index)));
        for (int pixel = 0; pixel < first.Length; pixel++)
        {
            // The fixture changes every nontransparent index to its successor.
            // This independent oracle catches wrong tile planes, palette indices,
            // crop/flip mistakes and edits which never reach visible output.
            if (!substitutions.TryGetValue(first[pixel], out Rgba32 expected) || second[pixel] != expected)
                throw new InvalidOperationException($"{context}: PNG index substitution differs at " +
                    $"({pixel % a.Width},{pixel / a.Width}): {first[pixel]} -> {second[pixel]}.");
            if (first[pixel] != second[pixel]) ChangedPixels++;
        }
        Compare(a, first, context + " stock");
        Compare(b, second, context + " replacement");
        // Reuse the immutable stock packet after the replacement upload. This also
        // detects stale renderer caches keyed only by reused VRAM destinations.
        Compare(a, first, context + " retained stock after replacement");

        RenderFrameSnapshot Packet(SnesVram vram) => new(new(++sequence, 1, 0),
            new LayeredRenderSnapshot(PpuMemorySnapshot.Capture(vram, colors, oam),
                [new ObjRenderLayer()], 3, 15));
    }

    private void Compare(RenderFrameSnapshot frame, Rgba32[] expected, string context)
    {
        PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: {context}");
        Comparisons++;
    }

    private static SnesCgram MakeColors()
    {
        var result = new SnesCgram();
        // All palette rows deliberately share sixteen distinct colors. Sprite
        // palette selection remains native, while the pixel-index oracle is exact.
        for (int index = 0; index < SnesPpuLayout.CgramColorCount; index++)
            result.SetColor(index, (ushort)((index % 16 + 1) | 7 << 5 | 13 << 10));
        return result;
    }
}
