using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed-resource identity, schema revision, and sequence dimensions for explosion fixed-color artwork.</summary>
public static class PowerBombFixedColorFormat
{
    /// <summary>Returns the number of radius-addressable colours in the selected sequence.</summary>
    /// <param name="sequence">Pre-explosion or explosion artwork domain; this is not an animation phase or composable bit mask.</param>
    /// <returns>Sixteen for pre-explosion or thirty-two for explosion, defining the exclusive upper index bound.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sequence value is not defined.</exception>
    public static int Count(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount,
        PowerBombFixedColorSequence.Explosion => SamusPaletteDefinitions.PowerBomb.ExplosionColorCount,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };
}
