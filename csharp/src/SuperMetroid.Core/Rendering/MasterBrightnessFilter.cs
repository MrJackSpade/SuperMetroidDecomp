using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software counterpart of the SNES INIDISP master-brightness multiplier.</summary>
public static class MasterBrightnessFilter
{
    /// <summary>Scales every opaque color by a native brightness level from zero to 15.</summary>
    public static void Apply(Span<Rgba32> pixels, int level)
    {
        int clampedLevel = Math.Clamp(level, 0, 15);
        if (clampedLevel == 15)
            return;

        for (int index = 0; index < pixels.Length; index++)
        {
            Rgba32 color = pixels[index];
            if (color.A == 0)
                continue;

            pixels[index] = clampedLevel == 0
                ? new Rgba32(0, 0, 0, color.A)
                : new Rgba32(
                    (byte)(color.R * clampedLevel / 15),
                    (byte)(color.G * clampedLevel / 15),
                    (byte)(color.B * clampedLevel / 15),
                    color.A);
        }
    }
}
