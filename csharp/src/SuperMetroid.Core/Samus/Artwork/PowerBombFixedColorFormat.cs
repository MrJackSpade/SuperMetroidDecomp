using SuperMetroid.Core.Samus.Palettes;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed-resource identity, schema revision, and native sequence dimensions for explosion fixed-color artwork.</summary>
public static class PowerBombFixedColorFormat
{
    /// <summary>JSON resource filename containing the pre-explosion and explosion RGB5 sequences.</summary>
    public const string FileName = "power-bomb-fixed-colors.json";
    /// <summary>Supported schema revision, one, fixing the sixteen- and thirty-two-entry sequence dimensions.</summary>
    public const int Version = 1;


    /// <summary>Returns the number of radius-addressable RGB5 triplets in the selected native sequence.</summary>
    /// <param name="sequence">Pre-explosion or explosion artwork domain; this is not an animation phase or composable bit mask.</param>
    /// <returns>Sixteen for pre-explosion or thirty-two for explosion, defining the exclusive upper index bound.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sequence value is not defined.</exception>
    public static int Count(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount,
        PowerBombFixedColorSequence.Explosion => SamusPaletteDefinitions.PowerBomb.ExplosionColorCount,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };
    /// <summary>
    /// $88:9079 PowerBomb_PreExplosion_Colors: white-to-yellow rise and decline;
    /// $88:8D85 PowerBombExplosion_Colors: yellow opening and grayscale white crest.
    /// Calculates all sixteen pre-explosion colors and explosion colors zero through twenty.
    /// Later explosion colors remain independently supplied pending their own conversion.
    /// </summary>
    internal static bool TryCalculateStock(PowerBombFixedColorSequence sequence, int index,
        out Bgr555 color)
    {
        color = default;
        if (sequence == PowerBombFixedColorSequence.PreExplosion && (uint)index < 16)
        {
            int red = index == 0 ? 16 : index <= 12 ? 2 * index + 2 : 50 - 2 * index;
            int blue = index <= 5 ? red : index is 6 or 12 ? 10 : index < 12 ? 8 : 34 - 2 * index;
            color = new Bgr555(red, red, blue);
            return true;
        }
        if (sequence != PowerBombFixedColorSequence.Explosion || (uint)index > 20) return false;
        if (index < 14)
        {
            int yellow = Math.Min(14 + index, 26);
            color = new Bgr555(yellow, yellow, Math.Max(10 - index, 0));
        }
        else
        {
            int white = index <= 18 ? 26 + (index - 14) / 2 : 46 - index;
            color = new Bgr555(white, white, white);
        }
        return true;
    }
}
