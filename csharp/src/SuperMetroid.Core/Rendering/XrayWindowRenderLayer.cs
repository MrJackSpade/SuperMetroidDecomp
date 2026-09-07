using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive native window endpoints; inverted endpoints hide the reveal on that line.</summary>
public readonly record struct XrayWindowLine(byte Left, byte Right);

/// <summary>Owned revealed BG2/OBJ scene and the resolved X-ray color-window endpoints.</summary>
public sealed record XrayWindowRenderLayer : RenderLayer
{
    private readonly XrayWindowLine[] lines;
    public ReadOnlySpan<XrayWindowLine> Lines => lines;
    public LayeredRenderSnapshot Reveal { get; }
    public XrayWindowRenderLayer(LayeredRenderSnapshot reveal, ReadOnlySpan<XrayWindowLine> lines)
    {
        ArgumentNullException.ThrowIfNull(reveal);
        if (lines.Length != SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("X-ray windows require every physical scanline.", nameof(lines));
        foreach (RenderLayer layer in reveal.Layers)
            if (layer is XrayWindowRenderLayer or WindowedSceneRenderLayer)
                throw new ArgumentException("Nested scene insertions are unsupported.", nameof(reveal));
        Reveal = reveal;
        this.lines = lines.ToArray();
    }
}
