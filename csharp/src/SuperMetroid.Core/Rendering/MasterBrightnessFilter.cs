using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software counterpart of the SNES INIDISP master-brightness multiplier.</summary>
public static class MasterBrightnessFilter
{
    /// <summary>Scales every opaque color by a native brightness level from zero to 15.</summary>
    public static void Apply(Span<Rgba32> pixels, int level)
    {
        if (level is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level), level, "SNES master brightness must be between 0 and 15.");
        }
        if (level == 15)
            return;

        for (int index = 0; index < pixels.Length; index++)
        {
            Rgba32 color = pixels[index];
            if (color.A == 0)
                continue;

            pixels[index] = level == 0
                ? new Rgba32(0, 0, 0, color.A)
                : new Rgba32(
                    (byte)(color.R * level / 15),
                    (byte)(color.G * level / 15),
                    (byte)(color.B * level / 15),
                    color.A);
        }
    }
}
