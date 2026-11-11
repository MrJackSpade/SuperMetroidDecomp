namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge values and helpers of <see cref="ColourKey"/> that only verification reads.</summary>
internal static class ColourKeyConstants
{
    /// <summary>The key of the pixels of <paramref name="colour"/> in a picture that recolours each colour alike.</summary>
    public static ColourKey Of(RgbColor colour) => new(colour, 0);
}
