using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The screen colour group each title or cinematic palette effect animates (#1464). Palette effects name their colours
/// by <see cref="PaletteFxColours"/>; a frontend screen holds them as the group named here, resolved through
/// <see cref="ScreenColourGroups"/>. Each group holds every colour the effect writes, from its first.
/// </summary>
internal static class ScreenPaletteFxColourDefinitions
{
    /// <summary>The screen colour group holding each effect's colours.</summary>
    internal static IReadOnlyDictionary<PaletteFxColours, ScreenColourGroup> Groups { get; } =
        new Dictionary<PaletteFxColours, ScreenColourGroup>
        {
            [PaletteFxColours.BabyMetroidTubeLight] = ScreenColourGroup.TitleTubeLight,
            [PaletteFxColours.TitleScreenDisplays] = ScreenColourGroup.TitleDisplays,
            [PaletteFxColours.CutsceneGunshipEngine] = ScreenColourGroup.CinematicGunshipEngine,
            [PaletteFxColours.CeresNavigationLightsBackground] = ScreenColourGroup.CeresNavigationLights,
            [PaletteFxColours.OldMotherBrainBackgroundLights] = ScreenColourGroup.IntroOldMotherBrainLights,
            [PaletteFxColours.WideZebesExplosionForeground] = ScreenColourGroup.EndingWideExplosionForeground,
            [PaletteFxColours.WideZebesExplosionBackground] = ScreenColourGroup.EndingWideExplosionBackground,
            [PaletteFxColours.ZebesExplosionFinale] = ScreenColourGroup.EndingExplosionFinale,
            [PaletteFxColours.SpaceDuringZebesExplosion] = ScreenColourGroup.EndingExplosionSpace,
            [PaletteFxColours.ExplodingZebesLava] = ScreenColourGroup.EndingExplodingZebesLava,
            [PaletteFxColours.ExplodingZebesCrust] = ScreenColourGroup.EndingExplodingZebesCrust,
            [PaletteFxColours.ExplodingZebesGreyClouds] = ScreenColourGroup.EndingExplodingZebesGreyClouds,
            [PaletteFxColours.GunshipEmergingFromExplosion] = ScreenColourGroup.EndingGunshipEmerging,
            [PaletteFxColours.UnusedCinematicFade] = ScreenColourGroup.UnusedCinematicFade,
            [PaletteFxColours.TitleLogo] = ScreenColourGroup.TitleLogoGlow,
            [PaletteFxColours.NintendoBootLogo] = ScreenColourGroup.NintendoBootLogo,
            [PaletteFxColours.PlanetZebesText] = ScreenColourGroup.PlanetZebesText,
            [PaletteFxColours.GunshipGlow] = ScreenColourGroup.EndingGunshipGlow,
            [PaletteFxColours.ZoomedOutExplodingZebes] = ScreenColourGroup.EndingZoomedOutExplodingZebes,
            [PaletteFxColours.PostCreditsSuperMetroidIcon] = ScreenColourGroup.EndingPostCreditsIcon,
        };
}
