namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The three mutually exclusive native Mode-7 ending backdrops.</summary>
public enum EndingMode7SceneId
{
    /// <summary>Zebes zooming out during the first escape backdrop, using native characters at $98:BCD6 and packed map at $99:D17E.</summary>
    EscapeA,
    /// <summary>Grey clouds during the second escape backdrop, using native characters at $98:ED4F and packed map at $99:D65B.</summary>
    EscapeB,
    /// <summary>The large Zebes backdrop for the planet explosion, using native characters at $99:9101 and packed map at $99:D932.</summary>
    PlanetExplosion,
}
