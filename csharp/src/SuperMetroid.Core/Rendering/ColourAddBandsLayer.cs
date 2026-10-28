namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A colour added to the screen within a band of columns on each of the 224 screen lines: each eight-bit channel
/// is clamped at 255 and the screen's opacity kept. Unlike the intro's colour addition, the screen's colour is not
/// first reduced to five-bit channels.
/// </summary>
public sealed record ColourAddBandsLayer : DisplayLayer
{
    /// <summary>The layer's own copy of each line's band, unchanged by later edits to the caller's.</summary>
    private readonly ColorAddWindow[] windows;
    /// <summary>Each screen line's band and colour, top to bottom: exactly 224.</summary>
    public ReadOnlySpan<ColorAddWindow> Windows => windows;

    /// <summary>Copies each screen line's band and the colour it adds.</summary>
    /// <param name="windows">One band of columns, both edges included, and the colour it adds, for each of the 224 screen lines.</param>
    /// <exception cref="ArgumentException">The input does not contain exactly 224 entries.</exception>
    public ColourAddBandsLayer(ReadOnlySpan<ColorAddWindow> windows)
    {
        Ensure.LengthEqual(windows, ScreenGeometry.ScreenHeightPixels);
        this.windows = windows.ToArray();
    }
}
