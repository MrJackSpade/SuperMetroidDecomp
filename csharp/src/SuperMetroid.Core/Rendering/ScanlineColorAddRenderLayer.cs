using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive horizontal endpoints and expanded-byte RGB addition for one physical scanline.</summary>
public readonly record struct ColorAddWindow(byte Left, byte Right, byte Red, byte Green, byte Blue)
{
    /// <summary>Inverted endpoints represent an empty window, including hidden HUD lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}

/// <summary>
/// Owned 224-line color windows. Add expanded eight-bit components, clamp at 255,
/// and preserve alpha. These reference semantics intentionally differ from the
/// intro operation which first reduces the source pixel to five-bit components.
/// </summary>
public sealed record ScanlineColorAddRenderLayer : RenderLayer
{
    private readonly ColorAddWindow[] windows;
    public ReadOnlySpan<ColorAddWindow> Windows => windows;

    public ScanlineColorAddRenderLayer(ReadOnlySpan<ColorAddWindow> windows)
    {
        if (windows.Length != SnesPpuLayout.ScreenHeightPixels)
            throw new ArgumentException("Color windows must cover exactly 224 physical scanlines.", nameof(windows));
        this.windows = windows.ToArray();
    }
}
