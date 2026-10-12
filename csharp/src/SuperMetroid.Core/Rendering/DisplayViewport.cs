using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Host display rectangle; small clients clip the centered minimum-size picture.</summary>
/// <param name="Offset">The picture's top-left corner relative to the client's; negative when a small client clips it.</param>
/// <param name="Size">The drawn picture size.</param>
public readonly record struct DisplayViewport(Offset16<DisplayPixels> Offset, Extent16<DisplayPixels> Size)
{
    /// <summary>
    /// The host's client size. Window-system and view APIs report it as two signed integers;
    /// a negative or oversized dimension is rejected rather than wrapped.
    /// </summary>
    [SpatialComponents("Host boundary: window-system APIs report the client size as two signed integers.")]
    public static Extent16<DisplayPixels> ClientSize(int width, int height)
    {
        Ensure.AtLeastZero(width);
        Ensure.AtLeastZero(height);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, ushort.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, ushort.MaxValue);
        return new((ushort)width, (ushort)height);
    }

    /// <summary>
    /// Centers an unfiltered native framebuffer using equal integer scales on both axes.
    /// Returns an empty viewport when no whole framebuffer fits; never silently crops or
    /// introduces TV-aspect stretching. Insets must be removed by the host first.
    /// </summary>
    public static DisplayViewport IntegerPixels(Extent16<DisplayPixels> client)
    {
        int scale = Math.Min(client.X / SnesPpuLayout.ScreenWidthPixels, client.Y / SnesPpuLayout.ScreenHeightPixels);
        return Centered(client, Scaled(SnesPpuLayout.ScreenWidthPixels, scale));
    }

    /// <summary>Preserves the desktop's 4:3 TV correction and integral vertical scaling.</summary>
    public static DisplayViewport ForClient(Extent16<DisplayPixels> client)
    {
        int unitWidth = (SnesPpuLayout.ScreenHeightPixels * DisplayAspect.Numerator + DisplayAspect.Denominator / 2)
            / DisplayAspect.Denominator;
        int scale = Math.Max(1, Math.Min(client.X / unitWidth, client.Y / SnesPpuLayout.ScreenHeightPixels));
        return Centered(client, Scaled(unitWidth, scale));
    }

    /// <summary>A native-height picture <paramref name="unitWidth"/> pixels wide, multiplied by <paramref name="scale"/>.</summary>
    private static Extent16<DisplayPixels> Scaled(int unitWidth, int scale) =>
        new(checked((ushort)(unitWidth * scale)), checked((ushort)(SnesPpuLayout.ScreenHeightPixels * scale)));

    /// <summary>Centered.</summary>
    private static DisplayViewport Centered(Extent16<DisplayPixels> client, Extent16<DisplayPixels> picture) => new(
        new Offset16<DisplayPixels>(checked((short)((client.X - picture.X) / 2)), checked((short)((client.Y - picture.Y) / 2))),
        picture);
}

/// <summary>Consumer display aspect, distinct from the native SNES sample dimensions.</summary>
internal static class DisplayAspect
{
    /// <summary>Stores numerator.</summary>
    internal const int Numerator = 4;
    /// <summary>Stores denominator.</summary>
    internal const int Denominator = 3;
}
