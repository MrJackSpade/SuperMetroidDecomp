namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The solid colours the ending's scenes flash colours to (#1512). Which colours each flash covers, and in what colour,
/// the import alone knows; the backdrop colour each shows is the scene's.
/// </summary>
public enum EndingFlash
{
    /// <summary>The exploding planet's glow, darkened as the planet's fade begins.</summary>
    PlanetGlow,

    /// <summary>The backdrop whitened as the explosion's silhouette appears.</summary>
    SilhouetteBackdrop,

    /// <summary>The whiteout as the planet's explosion ends, which whitens the sky and part of the scene.</summary>
    ExplosionWhiteout,

    /// <summary>The whiteout's sky cleared to black as the flyaway begins.</summary>
    WhiteoutCleared,

    /// <summary>The logo's background colours blacked out before its crossfade.</summary>
    LogoBackground,
}
