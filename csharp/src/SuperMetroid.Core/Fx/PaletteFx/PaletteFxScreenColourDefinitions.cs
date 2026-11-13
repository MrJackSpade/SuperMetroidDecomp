using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX colour groups the frontend screens' pictures show (#1512): the title's lights, logo and copyright, the
/// cinematics' gunship, Ceres lights and planet text, and the ending's explosion, gunship and icon effects. A screen shades
/// its pictures with the records these groups last showed; which inks each group recolours is the import's.
/// </summary>
internal static class PaletteFxScreenColourDefinitions
{
    /// <summary>The groups the title, cinematic and ending screens' pictures show.</summary>
    internal static ImmutableHashSet<PaletteFxColours> FrontendScreenGroups { get; } =
    [
        PaletteFxColours.TitleLogo,
        PaletteFxColours.NintendoBootLogo,
        PaletteFxColours.NintendoCopyright,
        PaletteFxColours.BabyMetroidTubeLight,
        PaletteFxColours.TitleScreenDisplays,
        PaletteFxColours.CutsceneGunshipEngine,
        PaletteFxColours.CeresNavigationLightsSprite,
        PaletteFxColours.CeresNavigationLightsBackground,
        PaletteFxColours.PlanetZebesText,
        PaletteFxColours.OldMotherBrainBackgroundLights,
        PaletteFxColours.ZoomedOutExplodingZebes,
        PaletteFxColours.WideZebesExplosionForeground,
        PaletteFxColours.WideZebesExplosionBackground,
        PaletteFxColours.ZebesExplosionFinale,
        PaletteFxColours.SpaceDuringZebesExplosion,
        PaletteFxColours.ZebesPlanetAfterglow,
        PaletteFxColours.ExplodingZebesLava,
        PaletteFxColours.ExplodingZebesCrust,
        PaletteFxColours.ExplodingZebesGreyClouds,
        PaletteFxColours.GunshipEmergingFromExplosion,
        PaletteFxColours.PostCreditsSuperMetroidIcon,
    ];
}
