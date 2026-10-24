using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>The Android host's display rectangle: the native picture at the largest whole-number scale that fits.</summary>
/// <param name="Offset">The picture's top-left corner relative to the client's.</param>
/// <param name="Size">The drawn picture size; empty when no whole picture fits.</param>
internal readonly record struct AndroidDisplayViewport(Offset16<DisplayPixels> Offset, Extent16<DisplayPixels> Size)
{
    /// <summary>
    /// The view's client size. View APIs report it as two signed integers; a negative or oversized dimension is
    /// rejected rather than wrapped.
    /// </summary>
    public static Extent16<DisplayPixels> ClientSize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, ushort.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, ushort.MaxValue);
        return new((ushort)width, (ushort)height);
    }

    /// <summary>
    /// Centers an unfiltered native picture using equal integer scales on both axes. Returns an empty viewport when no
    /// whole picture fits; never silently crops or introduces TV-aspect stretching. Insets must be removed by the host
    /// first.
    /// </summary>
    public static AndroidDisplayViewport IntegerPixels(Extent16<DisplayPixels> client)
    {
        int scale = Math.Min(client.X / SnesPpuLayout.ScreenWidthPixels, client.Y / SnesPpuLayout.ScreenHeightPixels);
        var picture = new Extent16<DisplayPixels>(checked((ushort)(SnesPpuLayout.ScreenWidthPixels * scale)),
            checked((ushort)(SnesPpuLayout.ScreenHeightPixels * scale)));
        return new(new Offset16<DisplayPixels>(checked((short)((client.X - picture.X) / 2)), checked((short)((client.Y - picture.Y) / 2))),
            picture);
    }
}
