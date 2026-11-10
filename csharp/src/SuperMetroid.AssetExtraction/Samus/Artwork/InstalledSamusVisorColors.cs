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

    /// <summary>Returns a visor shade for the owners that animate Samus's visor colour.</summary>
    /// <param name="index">Visor shade 0..5: 0..2 are X-ray widening colors and 3..5 are the steady X-ray/room-backdrop cycle.</param>
    /// <returns>The installed shade.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public RgbColor Resolve(int index)
    {
        if ((uint)index >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }
}
