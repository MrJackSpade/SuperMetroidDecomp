using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Which explosion picture each part of the ending's Zebes explosion shows, by the part it plays (#1626).</summary>
internal static class EndingExplosionDisplayDefinitions
{
    /// <summary>The planet's explosion, shown over the stars from the explosion's start until the finale replaces it.</summary>
    public const EndingObjectFragmentId PlanetExplosion = EndingObjectFragmentId.WideExplosion;
    /// <summary>The glow the burst adds over the planet's explosion and its debris.</summary>
    public const EndingObjectFragmentId BurstGlow = EndingObjectFragmentId.Blank;
    /// <summary>The explosion the finale shows over its glow.</summary>
    public const EndingObjectFragmentId FinaleExplosion = EndingObjectFragmentId.ConcentricExplosion;
    /// <summary>The finale's glow: pictured behind the finale's explosion and added over the whole screen.</summary>
    public const EndingObjectFragmentId FinaleGlow = EndingObjectFragmentId.Eclipse;
}
