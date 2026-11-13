using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>How many glow colours each of the explosion's glow sequences holds.</summary>
public static class PowerBombGlowColorFormat
{
    /// <summary>Returns the number of radius-addressable colours in the selected sequence.</summary>
    /// <param name="sequence">The pre-explosion flash's or the explosion's glow; this is not an animation phase.</param>
    /// <returns>Sixteen for pre-explosion or thirty-two for explosion, defining the exclusive upper index bound.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sequence value is not defined.</exception>
    public static int Count(PowerBombGlowSequence sequence) => sequence switch
    {
        PowerBombGlowSequence.PreExplosion => SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount,
        PowerBombGlowSequence.Explosion => SamusPaletteDefinitions.PowerBomb.ExplosionColorCount,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };
}
