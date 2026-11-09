using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects how each colour effect starts and the program it starts.
/// </summary>
internal static class RoomPaletteFxSelection
{
    /// <summary>How <paramref name="paletteFxObject"/> starts, and the program it starts.</summary>
    /// <remarks>Distinct effects may share a program. The intro's start only watched a condition that does nothing, so
    /// the Mother Brain fight scene's lights start plainly (#1554). Both fields are independently checked against
    /// supported NTSC J/U v1.0 and the pinned reference sources (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    internal static RoomPaletteFxDefinition Get(PaletteFxObject paletteFxObject) => paletteFxObject switch
    {
        PaletteFxObject.FadeInSuperMetroidTitleLogo => new(ColourEffectStart.Plain, PaletteFxProgram.FadeInSuperMetroidTitleLogo),
        PaletteFxObject.FadeInNintendoBootLogo => new(ColourEffectStart.Plain, PaletteFxProgram.FadeInNintendoBootLogo),
        PaletteFxObject.FadeInNintendoCopyright => new(ColourEffectStart.Plain, PaletteFxProgram.FadeInNintendoCopyright),
        PaletteFxObject.TitleScreenBabyMetroidTubeLight => new(ColourEffectStart.Plain, PaletteFxProgram.TitleScreenBabyMetroidTubeLight0),
        PaletteFxObject.TitleScreenFlickeringDisplays => new(ColourEffectStart.Plain, PaletteFxProgram.TitleScreenFlickeringDisplays0),
        PaletteFxObject.CutsceneGunshipEngineFlicker => new(ColourEffectStart.Plain, PaletteFxProgram.CutsceneGunshipEngineFlicker0),
        PaletteFxObject.CutsceneCeresNavigationLightsSprite => new(ColourEffectStart.Plain, PaletteFxProgram.CutsceneCeresNavigationLightSprite),
        PaletteFxObject.FadeInPlanetZebesText => new(ColourEffectStart.Plain, PaletteFxProgram.FadeInPlanetZebesText),
        PaletteFxObject.FadeOutPlanetZebesText => new(ColourEffectStart.Plain, PaletteFxProgram.FadeOutPlanetZebesText),
        PaletteFxObject.CutsceneCeresNavigationLightsBackground => new(ColourEffectStart.Plain, PaletteFxProgram.CutsceneCeresNavigationLightsBG),
        PaletteFxObject.OldMotherBrainFightBackgroundLights => new(ColourEffectStart.Plain, PaletteFxProgram.OldMotherBrainFightBGLights0),
        PaletteFxObject.GunshipGlow => new(ColourEffectStart.Plain, PaletteFxProgram.GunshipGlow0),
        PaletteFxObject.FadeOutZoomedOutExplodingZebes => new(ColourEffectStart.Plain, PaletteFxProgram.FadeOutZoomedOutExplodingZebes),
        PaletteFxObject.WidePartOfZebesExplosionForeground => new(ColourEffectStart.Plain, PaletteFxProgram.WidePartOfZebesExplosionForeground),
        PaletteFxObject.ZebesExplosionFinale => new(ColourEffectStart.Plain, PaletteFxProgram.ZebesExplosionFinale),
        PaletteFxObject.WhiteOutSpaceDuringZebesExplosion => new(ColourEffectStart.Plain, PaletteFxProgram.WhiteOutSpaceDuringZebesExplosion),
        PaletteFxObject.ZebesExplosionPlanetAfterglow => new(ColourEffectStart.Plain, PaletteFxProgram.ZebesExplosionPlanetAfterGlow0),
        PaletteFxObject.ExplodingZebesLava => new(ColourEffectStart.Plain, PaletteFxProgram.ExplodingZebesLava0),
        PaletteFxObject.FadeOutExplodingZebesCrust => new(ColourEffectStart.Plain, PaletteFxProgram.FadeOutExplodingZebesCrust),
        PaletteFxObject.FadeOutExplodingZebesGreyClouds => new(ColourEffectStart.Plain, PaletteFxProgram.FadeOutExplodingZebesGreyClouds),
        PaletteFxObject.GunshipEmergingFromZebesExplosion => new(ColourEffectStart.Plain, PaletteFxProgram.GunshipEmergingFromZebesExplosion),
        PaletteFxObject.WidePartOfZebesExplosionBackground => new(ColourEffectStart.Plain, PaletteFxProgram.WidePartOfZebesExplosionBackground),
        PaletteFxObject.UnusedCinematicFade => new(ColourEffectStart.Plain, PaletteFxProgram.UnusedCinematicFade),
        PaletteFxObject.HyperBeam => new(ColourEffectStart.Plain, PaletteFxProgram.HyperBeam0),
        PaletteFxObject.SamusLoadingPowerSuit => new(ColourEffectStart.Plain, PaletteFxProgram.SamusLoadingPowerSuit0),
        PaletteFxObject.SamusLoadingVariaSuit => new(ColourEffectStart.Plain, PaletteFxProgram.SamusLoadingVairaSuit0),
        PaletteFxObject.SamusLoadingGravitySuit => new(ColourEffectStart.Plain, PaletteFxProgram.SamusLoadingGravitySuit0),
        PaletteFxObject.PostCreditsSuperMetroidIcon => new(ColourEffectStart.Plain, PaletteFxProgram.PostCreditsSuperMetroidIconGlare),
        PaletteFxObject.Nothing => new(ColourEffectStart.Plain, PaletteFxProgram.Nothing),
        PaletteFxObject.GreyOutTourianStatueDraygon => new(ColourEffectStart.Plain, PaletteFxProgram.GreyOutTourianStatueDraygon),
        PaletteFxObject.GreyOutTourianStatueKraid => new(ColourEffectStart.Plain, PaletteFxProgram.GreyOutTourianStatueKraid),
        PaletteFxObject.GreyOutTourianStatueRidley => new(ColourEffectStart.Plain, PaletteFxProgram.GreyOutTourianStatueRidley),
        PaletteFxObject.GreyOutTourianStatuePhantoon => new(ColourEffectStart.Plain, PaletteFxProgram.GreyOutTourianStatuePhantoon),
        PaletteFxObject.BombTorizoBelly => new(ColourEffectStart.Plain, PaletteFxProgram.BombTorizoBelly0),
        PaletteFxObject.GoldenTorizoBelly => new(ColourEffectStart.Plain, PaletteFxProgram.GoldenTorizoBelly0),
        PaletteFxObject.SamusInHeat => new(ColourEffectStart.ChoosesHeatProgramForSuit, PaletteFxProgram.SamusInHeatPowerSuit0),
        PaletteFxObject.CrateriaLightning => new(ColourEffectStart.Plain, PaletteFxProgram.CrateriaLightning),
        PaletteFxObject.UnusedDarkLightning => new(ColourEffectStart.Plain, PaletteFxProgram.UnusedDarkLightning),
        PaletteFxObject.WreckedShipGreenLights => new(ColourEffectStart.Plain, PaletteFxProgram.WreckedShipGreenLights),
        PaletteFxObject.UnusedWreckedShipGreenLightsDuplicate => new(ColourEffectStart.Plain, PaletteFxProgram.WreckedShipGreenLights),
        PaletteFxObject.BrinstarBlueBackgroundSpores => new(ColourEffectStart.Plain, PaletteFxProgram.BrinstarBlueBackgroundSpores),
        PaletteFxObject.SporeSpawnBlueBackgroundSpores => new(ColourEffectStart.SkippedOnceAreaMiniBossIsDead, PaletteFxProgram.SporeSpawnBlueBackgroundSpores),
        PaletteFxObject.BrinstarRedBackgroundGlow => new(ColourEffectStart.Plain, PaletteFxProgram.BrinstarRedBackgroundGlow),
        PaletteFxObject.BeaconFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.BeaconFlashing),
        PaletteFxObject.NorfairForegroundAndHeatPhase => new(ColourEffectStart.Plain, PaletteFxProgram.NorfairForegroundAndHeatPhase),
        PaletteFxObject.NorfairGreenForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxProgram.NorfairGreenForegroundHeatGlow),
        PaletteFxObject.NorfairPurpleForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxProgram.NorfairPurpleForegroundHeatGlow),
        PaletteFxObject.NorfairDarkForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxProgram.NorfairDarkForegroundHeatGlow),
        PaletteFxObject.MaridiaSandPits => new(ColourEffectStart.Plain, PaletteFxProgram.MaridiaSandPits),
        PaletteFxObject.MaridiaSandFalls => new(ColourEffectStart.Plain, PaletteFxProgram.MaridiaSandFalls),
        PaletteFxObject.MaridiaBackgroundWaterfalls => new(ColourEffectStart.Plain, PaletteFxProgram.MaridiaBackgroundWaterfalls),
        PaletteFxObject.TourianGlowingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, PaletteFxProgram.TourianGlowingArkanoidBlocksAndRedOrbs),
        PaletteFxObject.UnusedTourianGlowClone => new(ColourEffectStart.Plain, PaletteFxProgram.UnusedTourianGlowClone),
        PaletteFxObject.TourianShutterRedFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.TourianShutterRedFlashing),
        PaletteFxObject.TourianBackgroundRedFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.TourianBackgroundRedFlashing),
        PaletteFxObject.TourianGeneralLevelRedFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.TourianGeneralLevelRedFlashing),
        PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, PaletteFxProgram.TourianRedFlashingArkanoidBlocksAndRedOrbs),
        PaletteFxObject.OldTourianRedFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.OldTourianRedFlashing),
        PaletteFxObject.OldTourianBackgroundRailingsFlashYellow => new(ColourEffectStart.Plain, PaletteFxProgram.OldTourianBackgroundRailingsFlashYellow),
        PaletteFxObject.OldTourianBackgroundPanelsFlashYellow => new(ColourEffectStart.Plain, PaletteFxProgram.OldTourianBackgroundPanelsFlashYellow),
        PaletteFxObject.UpperCrateriaRedFlashing => new(ColourEffectStart.Plain, PaletteFxProgram.UpperCrateriaRedFlashing),
        PaletteFxObject.CrateriaEscapeYellowLightning => new(ColourEffectStart.Plain, PaletteFxProgram.CrateriaEscapeYellowLightning),
        PaletteFxObject.ParlorEscapeSkyFlicker => new(ColourEffectStart.Plain, PaletteFxProgram.ParlorEscapeSkyFlicker),
        _ => throw new ArgumentOutOfRangeException(nameof(paletteFxObject), paletteFxObject, "Not a palette-FX object."),
    };
}
