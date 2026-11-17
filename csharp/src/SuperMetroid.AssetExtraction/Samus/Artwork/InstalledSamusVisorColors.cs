using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>The installed visor colours shared by X-ray and room palette cycling, every shade kept as installed.</summary>
public sealed class InstalledSamusVisorColors
{
    /// <summary>The installed visor shades, in cycle order.</summary>
    internal readonly RgbColor[] colors;

    /// <summary>Keeps a copy of the complete installed visor shades.</summary>
    /// <param name="colors">All <see cref="SamusVisorColorFormat.ColorCount"/> visor shades.</param>
    /// <exception cref="InvalidDataException">A shade is missing.</exception>
    internal InstalledSamusVisorColors(RgbColor[] colors)
    {
        if (colors.Length != SamusVisorColorFormat.ColorCount)
            throw new InvalidDataException($"Samus visor colors require {SamusVisorColorFormat.ColorCount} colors.");
        this.colors = (RgbColor[])colors.Clone();
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
