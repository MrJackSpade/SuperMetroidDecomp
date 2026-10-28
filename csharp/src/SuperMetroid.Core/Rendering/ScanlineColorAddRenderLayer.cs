namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Owned 224-line color windows. Add expanded eight-bit components, clamp at 255,
/// and preserve alpha. These reference semantics intentionally differ from the
/// intro operation which first reduces the source pixel to five-bit components.
/// </summary>
public sealed record ScanlineColorAddRenderLayer : RenderLayer
{
    /// <summary>Private copy that keeps each physical scanline's color window stable after construction.</summary>
    private readonly ColorAddWindow[] windows;
    /// <summary>Exactly 224 immutable per-scanline windows in top-to-bottom physical display order.</summary>
    public ReadOnlySpan<ColorAddWindow> Windows => windows;

    /// <summary>Copies a complete physical-screen color-add program for deferred software or GPU compositing.</summary>
    /// <param name="windows">One inclusive window and expanded RGB addition for each of the 224 visible scanlines.</param>
    /// <exception cref="ArgumentException">The input does not contain exactly 224 entries.</exception>
    public ScanlineColorAddRenderLayer(ReadOnlySpan<ColorAddWindow> windows)
    {
        Ensure.LengthEqual(windows, ScreenGeometry.ScreenHeightPixels);
        this.windows = windows.ToArray();
    }
}
