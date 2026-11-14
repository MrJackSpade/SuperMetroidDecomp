using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Palette objects spawned by the ending's scene steps.</summary>
internal static class EndingColourEffectDefinitions
{
    /// <summary>Fades the zoomed-out exploding Zebes.</summary>
    public const ColourEffectId FadePlanet = ColourEffectId.FadeOutZoomedOutExplodingZebes;
    /// <summary>The initial supernova color animation.</summary>
    public const ColourEffectId Supernova = ColourEffectId.WidePartOfZebesExplosionForeground;
    /// <summary>The final supernova color animation.</summary>
    public const ColourEffectId SupernovaFinale = ColourEffectId.ZebesExplosionFinale;
    /// <summary>The explosion color animation, spawned with the initial supernova.</summary>
    public const ColourEffectId Explosion = ColourEffectId.WhiteOutSpaceDuringZebesExplosion;
    /// <summary>Exploding lava.</summary>
    public const ColourEffectId Lava = ColourEffectId.ExplodingZebesLava;
    /// <summary>Fading crust, spawned with the exploding lava.</summary>
    public const ColourEffectId Crust = ColourEffectId.FadeOutExplodingZebesCrust;
    /// <summary>Fading grey clouds.</summary>
    public const ColourEffectId GreyClouds = ColourEffectId.FadeOutExplodingZebesGreyClouds;
    /// <summary>The wide supernova background, spawned with the initial supernova.</summary>
    public const ColourEffectId WideExplosion = ColourEffectId.WidePartOfZebesExplosionBackground;
    /// <summary>Planet afterglow.</summary>
    public const ColourEffectId PlanetAfterglow = ColourEffectId.ZebesExplosionPlanetAfterglow;
    /// <summary>The gunship emerging from the explosion, spawned with the afterglow.</summary>
    public const ColourEffectId GunshipEmergence = ColourEffectId.GunshipEmergingFromZebesExplosion;

    /// <summary>
    /// The colours whose first is the space behind the exploding planet (#1512): a record of them that writes that colour
    /// shows it as the ending's backdrop.
    /// </summary>
    public const ColourEffectColours SpaceColours = ColourEffectColours.SpaceDuringZebesExplosion;
}
