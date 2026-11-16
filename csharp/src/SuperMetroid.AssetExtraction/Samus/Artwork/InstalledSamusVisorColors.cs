using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>The installed visor colours shared by X-ray and room palette cycling, every shade kept as installed.</summary>
public sealed class InstalledSamusVisorColors
{
    /// <summary>The installed visor shades, in cycle order.</summary>
    private readonly RgbColor[] colors;

    /// <summary>Keeps a copy of the complete installed visor shades.</summary>
    /// <param name="colors">All <see cref="SamusVisorColorFormat.ColorCount"/> visor shades.</param>
    /// <exception cref="InvalidDataException">A shade is missing.</exception>
    internal InstalledSamusVisorColors(RgbColor[] colors)
    {
        if (colors.Length != SamusVisorColorFormat.ColorCount)
            throw new InvalidDataException($"Samus visor colors require {SamusVisorColorFormat.ColorCount} colors.");
        this.colors = (RgbColor[])colors.Clone();
    }

    /// <summary>Returns a stored visor colour, in the file's order: the X-ray widening glow's, then the steady cycle's.</summary>
    /// <param name="index">The colour's place in the file, 0..5.</param>
    /// <returns>The installed shade.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public RgbColor Resolve(int index)
    {
        if ((uint)index >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }

    /// <summary>The colour of shade <paramref name="shade"/> of the X-ray widening glow: the file's first run (#2285).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The glow has no such shade.</exception>
    public RgbColor XrayWidening(int shade) => (uint)shade < SamusShadeDefinitions.Visor.XrayWideningShades
        ? colors[shade]
        : throw new ArgumentOutOfRangeException(nameof(shade));

    /// <summary>The colour of shade <paramref name="shade"/> of the steady visor cycle: the file's run after the glow's (#2285).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The cycle has no such shade.</exception>
    public RgbColor Cycle(int shade) => (uint)shade < SamusShadeDefinitions.Visor.CycleShades
        ? colors[SamusShadeDefinitions.Visor.XrayWideningShades + shade]
        : throw new ArgumentOutOfRangeException(nameof(shade));
}
