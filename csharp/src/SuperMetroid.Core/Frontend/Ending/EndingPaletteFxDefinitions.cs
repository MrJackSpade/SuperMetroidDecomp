using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8D palette objects spawned by the ending's bank-$8B callbacks.</summary>
internal static class EndingPaletteFxDefinitions
{
    /// <summary>$8D:E1C4, fade zoomed-out exploding Zebes, spawned by $8B:F284.</summary>
    public const PaletteFxObject FadePlanet = PaletteFxObject.FadeOutZoomedOutExplodingZebes;
    /// <summary>$8D:E1C8, initial supernova color program, spawned by $8B:F2B7.</summary>
    public const PaletteFxObject Supernova = PaletteFxObject.WidePartOfZebesExplosionForeground;
    /// <summary>$8D:E1CC, final supernova color program, spawned by $8B:F2FA.</summary>
    public const PaletteFxObject SupernovaFinale = PaletteFxObject.ZebesExplosionFinale;
    /// <summary>$8D:E1D0, explosion color program, spawned by $8B:F2B7.</summary>
    public const PaletteFxObject Explosion = PaletteFxObject.WhiteOutSpaceDuringZebesExplosion;
    /// <summary>$8D:E1D8, exploding lava, spawned by $8B:D6D7.</summary>
    public const PaletteFxObject Lava = PaletteFxObject.ExplodingZebesLava;
    /// <summary>$8D:E1DC, fading crust, spawned by $8B:D6D7.</summary>
    public const PaletteFxObject Crust = PaletteFxObject.FadeOutExplodingZebesCrust;
    /// <summary>$8D:E1E0, fading grey clouds, spawned by $8B:D731.</summary>
    public const PaletteFxObject GreyClouds = PaletteFxObject.FadeOutExplodingZebesGreyClouds;
    /// <summary>$8D:E1E8, wide supernova background, spawned by $8B:F2B7.</summary>
    public const PaletteFxObject WideExplosion = PaletteFxObject.WidePartOfZebesExplosionBackground;
    /// <summary>$8D:E1D4, planet afterglow, spawned by $8B:DBC4.</summary>
    public const PaletteFxObject PlanetAfterglow = PaletteFxObject.ZebesExplosionPlanetAfterglow;
    /// <summary>$8D:E1E4, gunship emerging from the explosion, spawned by $8B:DBC4.</summary>
    public const PaletteFxObject GunshipEmergence = PaletteFxObject.GunshipEmergingFromZebesExplosion;
}
