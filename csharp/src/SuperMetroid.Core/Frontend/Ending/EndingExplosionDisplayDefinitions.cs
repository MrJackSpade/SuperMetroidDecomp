using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Which explosion picture each part of the ending's Zebes explosion shows, by the part it plays (#1626).</summary>
internal static partial class EndingExplosionDisplayDefinitions
{
    /// <summary>The planet's explosion, shown over the stars from the explosion's start until the finale replaces it.</summary>
    public const EndingExplosionPicture PlanetExplosion = EndingExplosionPicture.WideExplosion;
    /// <summary>The glow the burst adds over the planet's explosion and its debris (<see cref="EndingExplosionPicture.BurstGlow"/>).</summary>
    public const EndingExplosionPicture BurstGlow = EndingExplosionPicture.BurstGlow;
    /// <summary>The explosion the finale shows over its glow.</summary>
    public const EndingExplosionPicture FinaleExplosion = EndingExplosionPicture.ConcentricExplosion;
    /// <summary>The finale's glow: pictured behind the finale's explosion and added over the whole screen.</summary>
    public const EndingExplosionPicture FinaleGlow = EndingExplosionPicture.Eclipse;
}
