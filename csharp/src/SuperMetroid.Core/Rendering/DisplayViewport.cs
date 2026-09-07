using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Host display rectangle; small clients clip the centered minimum-size picture.</summary>
public readonly record struct DisplayViewport(int Left, int Top, int Width, int Height)
{
    /// <summary>Preserves the desktop's 4:3 TV correction and integral vertical scaling.</summary>
    public static DisplayViewport ForClient(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        int unitWidth = (SnesPpuLayout.ScreenHeightPixels * DisplayAspect.Numerator + DisplayAspect.Denominator / 2)
            / DisplayAspect.Denominator;
        int scale = Math.Max(1, Math.Min(width / unitWidth, height / SnesPpuLayout.ScreenHeightPixels));
        int drawWidth = unitWidth * scale, drawHeight = SnesPpuLayout.ScreenHeightPixels * scale;
        return new((width - drawWidth) / 2, (height - drawHeight) / 2, drawWidth, drawHeight);
    }
}

/// <summary>Consumer display aspect, distinct from the native SNES sample dimensions.</summary>
internal static class DisplayAspect
{
    internal const int Numerator = 4;
    internal const int Denominator = 3;
}
