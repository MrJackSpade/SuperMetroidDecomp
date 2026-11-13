using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Palette objects spawned by the ending's callbacks.</summary>
internal static class EndingPaletteFxDefinitions
{
    /// <summary>Fades the zoomed-out exploding Zebes.</summary>
    public const PaletteFxObject FadePlanet = PaletteFxObject.FadeOutZoomedOutExplodingZebes;
    /// <summary>The initial supernova color animation.</summary>
    public const PaletteFxObject Supernova = PaletteFxObject.WidePartOfZebesExplosionForeground;
    /// <summary>The final supernova color animation.</summary>
    public const PaletteFxObject SupernovaFinale = PaletteFxObject.ZebesExplosionFinale;
    /// <summary>The explosion color animation, spawned with the initial supernova.</summary>
    public const PaletteFxObject Explosion = PaletteFxObject.WhiteOutSpaceDuringZebesExplosion;
    /// <summary>Exploding lava.</summary>
    public const PaletteFxObject Lava = PaletteFxObject.ExplodingZebesLava;
    /// <summary>Fading crust, spawned with the exploding lava.</summary>
    public const PaletteFxObject Crust = PaletteFxObject.FadeOutExplodingZebesCrust;
    /// <summary>Fading grey clouds.</summary>
    public const PaletteFxObject GreyClouds = PaletteFxObject.FadeOutExplodingZebesGreyClouds;
    /// <summary>The wide supernova background, spawned with the initial supernova.</summary>
    public const PaletteFxObject WideExplosion = PaletteFxObject.WidePartOfZebesExplosionBackground;
    /// <summary>Planet afterglow.</summary>
    public const PaletteFxObject PlanetAfterglow = PaletteFxObject.ZebesExplosionPlanetAfterglow;
    /// <summary>The gunship emerging from the explosion, spawned with the afterglow.</summary>
    public const PaletteFxObject GunshipEmergence = PaletteFxObject.GunshipEmergingFromZebesExplosion;

    /// <summary>
    /// The colours whose first is the space behind the exploding planet (#1512): a record of them that writes that colour
    /// shows it as the ending's backdrop.
    /// </summary>
    public const PaletteFxColours SpaceColours = PaletteFxColours.SpaceDuringZebesExplosion;
}
