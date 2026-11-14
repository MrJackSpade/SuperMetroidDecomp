using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects' colour groups the frontend screens' pictures show (#1512): the title's lights, logo and copyright, the
/// cinematics' gunship, Ceres lights and planet text, and the ending's explosion, gunship and icon effects. A screen shades
/// its pictures with the frames these groups last showed; which inks each group recolours is the import's.
/// </summary>
internal static class ColourEffectScreenColourDefinitions
{
    /// <summary>The groups the title, cinematic and ending screens' pictures show.</summary>
    internal static ImmutableHashSet<ColourEffectColours> FrontendScreenGroups { get; } =
    [
        ColourEffectColours.TitleLogo,
        ColourEffectColours.NintendoCopyright,
        ColourEffectColours.BabyMetroidTubeLight,
        ColourEffectColours.TitleScreenDisplays,
        ColourEffectColours.CutsceneGunshipEngine,
        ColourEffectColours.CeresNavigationLightsSprite,
        ColourEffectColours.CeresNavigationLightsBackground,
        ColourEffectColours.PlanetZebesText,
        ColourEffectColours.OldMotherBrainBackgroundLights,
        ColourEffectColours.ZoomedOutExplodingZebes,
        ColourEffectColours.WideZebesExplosionForeground,
        ColourEffectColours.WideZebesExplosionBackground,
        ColourEffectColours.ZebesExplosionFinale,
        ColourEffectColours.SpaceDuringZebesExplosion,
        ColourEffectColours.ZebesPlanetAfterglow,
        ColourEffectColours.ExplodingZebesLava,
        ColourEffectColours.ExplodingZebesCrust,
        ColourEffectColours.ExplodingZebesGreyClouds,
        ColourEffectColours.GunshipEmergingFromExplosion,
        ColourEffectColours.PostCreditsSuperMetroidIcon,
    ];
}
