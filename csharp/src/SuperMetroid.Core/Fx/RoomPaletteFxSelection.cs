using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects how each colour effect starts and the animation it starts.
/// </summary>
internal static class RoomPaletteFxSelection
{
    /// <summary>How <paramref name="paletteFxObject"/> starts, and the animation it starts.</summary>
    /// <remarks>Distinct effects may share an animation. The intro's start only watched a condition that does nothing, so
    /// the Mother Brain fight scene's lights start plainly (#1554). Both fields are independently checked against
    /// supported NTSC J/U v1.0 and the pinned reference sources (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    internal static RoomPaletteFxDefinition Get(PaletteFxObject paletteFxObject) => paletteFxObject switch
    {
        PaletteFxObject.FadeInSuperMetroidTitleLogo => new(ColourEffectStart.Plain, PaletteFxSegment.FadeInSuperMetroidTitleLogo),
        PaletteFxObject.FadeInNintendoBootLogo => new(ColourEffectStart.Plain, PaletteFxSegment.FadeInNintendoBootLogo),
        PaletteFxObject.FadeInNintendoCopyright => new(ColourEffectStart.Plain, PaletteFxSegment.FadeInNintendoCopyright),
        PaletteFxObject.TitleScreenBabyMetroidTubeLight => new(ColourEffectStart.Plain, PaletteFxSegment.TitleScreenBabyMetroidTubeLight0),
        PaletteFxObject.TitleScreenFlickeringDisplays => new(ColourEffectStart.Plain, PaletteFxSegment.TitleScreenFlickeringDisplays0),
        PaletteFxObject.CutsceneGunshipEngineFlicker => new(ColourEffectStart.Plain, PaletteFxSegment.CutsceneGunshipEngineFlicker0),
        PaletteFxObject.CutsceneCeresNavigationLightsSprite => new(ColourEffectStart.Plain, PaletteFxSegment.CutsceneCeresNavigationLightSprite),
        PaletteFxObject.FadeInPlanetZebesText => new(ColourEffectStart.Plain, PaletteFxSegment.FadeInPlanetZebesText),
        PaletteFxObject.FadeOutPlanetZebesText => new(ColourEffectStart.Plain, PaletteFxSegment.FadeOutPlanetZebesText),
        PaletteFxObject.CutsceneCeresNavigationLightsBackground => new(ColourEffectStart.Plain, PaletteFxSegment.CutsceneCeresNavigationLightsBG),
        PaletteFxObject.OldMotherBrainFightBackgroundLights => new(ColourEffectStart.Plain, PaletteFxSegment.OldMotherBrainFightBGLights0),
        PaletteFxObject.GunshipGlow => new(ColourEffectStart.Plain, PaletteFxSegment.GunshipGlow0),
        PaletteFxObject.FadeOutZoomedOutExplodingZebes => new(ColourEffectStart.Plain, PaletteFxSegment.FadeOutZoomedOutExplodingZebes),
        PaletteFxObject.WidePartOfZebesExplosionForeground => new(ColourEffectStart.Plain, PaletteFxSegment.WidePartOfZebesExplosionForeground),
        PaletteFxObject.ZebesExplosionFinale => new(ColourEffectStart.Plain, PaletteFxSegment.ZebesExplosionFinale),
        PaletteFxObject.WhiteOutSpaceDuringZebesExplosion => new(ColourEffectStart.Plain, PaletteFxSegment.WhiteOutSpaceDuringZebesExplosion),
        PaletteFxObject.ZebesExplosionPlanetAfterglow => new(ColourEffectStart.Plain, PaletteFxSegment.ZebesExplosionPlanetAfterGlow0),
        PaletteFxObject.ExplodingZebesLava => new(ColourEffectStart.Plain, PaletteFxSegment.ExplodingZebesLava0),
        PaletteFxObject.FadeOutExplodingZebesCrust => new(ColourEffectStart.Plain, PaletteFxSegment.FadeOutExplodingZebesCrust),
        PaletteFxObject.FadeOutExplodingZebesGreyClouds => new(ColourEffectStart.Plain, PaletteFxSegment.FadeOutExplodingZebesGreyClouds),
        PaletteFxObject.GunshipEmergingFromZebesExplosion => new(ColourEffectStart.Plain, PaletteFxSegment.GunshipEmergingFromZebesExplosion),
        PaletteFxObject.WidePartOfZebesExplosionBackground => new(ColourEffectStart.Plain, PaletteFxSegment.WidePartOfZebesExplosionBackground),
        PaletteFxObject.HyperBeam => new(ColourEffectStart.Plain, PaletteFxSegment.HyperBeam0),
        PaletteFxObject.SamusLoadingPowerSuit => new(ColourEffectStart.Plain, PaletteFxSegment.SamusLoadingPowerSuit0),
        PaletteFxObject.SamusLoadingVariaSuit => new(ColourEffectStart.Plain, PaletteFxSegment.SamusLoadingVairaSuit0),
        PaletteFxObject.SamusLoadingGravitySuit => new(ColourEffectStart.Plain, PaletteFxSegment.SamusLoadingGravitySuit0),
        PaletteFxObject.PostCreditsSuperMetroidIcon => new(ColourEffectStart.Plain, PaletteFxSegment.PostCreditsSuperMetroidIconGlare),
        PaletteFxObject.Nothing => new(ColourEffectStart.Plain, PaletteFxSegment.Nothing),
        PaletteFxObject.GreyOutTourianStatueDraygon => new(ColourEffectStart.Plain, PaletteFxSegment.GreyOutTourianStatueDraygon),
        PaletteFxObject.GreyOutTourianStatueKraid => new(ColourEffectStart.Plain, PaletteFxSegment.GreyOutTourianStatueKraid),
        PaletteFxObject.GreyOutTourianStatueRidley => new(ColourEffectStart.Plain, PaletteFxSegment.GreyOutTourianStatueRidley),
        PaletteFxObject.GreyOutTourianStatuePhantoon => new(ColourEffectStart.Plain, PaletteFxSegment.GreyOutTourianStatuePhantoon),
        PaletteFxObject.BombTorizoBelly => new(ColourEffectStart.Plain, PaletteFxSegment.BombTorizoBelly0),
        PaletteFxObject.GoldenTorizoBelly => new(ColourEffectStart.Plain, PaletteFxSegment.GoldenTorizoBelly0),
        PaletteFxObject.SamusInHeat => new(ColourEffectStart.ChoosesHeatAnimationForSuit, PaletteFxSegment.SamusInHeatPowerSuit),
        PaletteFxObject.CrateriaLightning => new(ColourEffectStart.Plain, PaletteFxSegment.CrateriaLightning),
        PaletteFxObject.WreckedShipGreenLights => new(ColourEffectStart.Plain, PaletteFxSegment.WreckedShipGreenLights),
        PaletteFxObject.BrinstarBlueBackgroundSpores => new(ColourEffectStart.Plain, PaletteFxSegment.BrinstarBlueBackgroundSpores),
        PaletteFxObject.SporeSpawnBlueBackgroundSpores => new(ColourEffectStart.SkippedOnceAreaMiniBossIsDead, PaletteFxSegment.SporeSpawnBlueBackgroundSpores),
        PaletteFxObject.BrinstarRedBackgroundGlow => new(ColourEffectStart.Plain, PaletteFxSegment.BrinstarRedBackgroundGlow),
        PaletteFxObject.BeaconFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.BeaconFlashing),
        PaletteFxObject.NorfairForegroundAndHeatPhase => new(ColourEffectStart.Plain, PaletteFxSegment.NorfairForegroundAndHeatPhase),
        PaletteFxObject.NorfairGreenForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxSegment.NorfairGreenForegroundHeatGlow),
        PaletteFxObject.NorfairPurpleForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxSegment.NorfairPurpleForegroundHeatGlow),
        PaletteFxObject.NorfairDarkForegroundHeatGlow => new(ColourEffectStart.Plain, PaletteFxSegment.NorfairDarkForegroundHeatGlow),
        PaletteFxObject.MaridiaSandPits => new(ColourEffectStart.Plain, PaletteFxSegment.MaridiaSandPits),
        PaletteFxObject.MaridiaSandFalls => new(ColourEffectStart.Plain, PaletteFxSegment.MaridiaSandFalls),
        PaletteFxObject.MaridiaBackgroundWaterfalls => new(ColourEffectStart.Plain, PaletteFxSegment.MaridiaBackgroundWaterfalls),
        PaletteFxObject.TourianGlowingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, PaletteFxSegment.TourianGlowingArkanoidBlocksAndRedOrbs),
        PaletteFxObject.TourianShutterRedFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.TourianShutterRedFlashing),
        PaletteFxObject.TourianBackgroundRedFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.TourianBackgroundRedFlashing),
        PaletteFxObject.TourianGeneralLevelRedFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.TourianGeneralLevelRedFlashing),
        PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, PaletteFxSegment.TourianRedFlashingArkanoidBlocksAndRedOrbs),
        PaletteFxObject.OldTourianRedFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.OldTourianRedFlashing),
        PaletteFxObject.OldTourianBackgroundRailingsFlashYellow => new(ColourEffectStart.Plain, PaletteFxSegment.OldTourianBackgroundRailingsFlashYellow),
        PaletteFxObject.OldTourianBackgroundPanelsFlashYellow => new(ColourEffectStart.Plain, PaletteFxSegment.OldTourianBackgroundPanelsFlashYellow),
        PaletteFxObject.UpperCrateriaRedFlashing => new(ColourEffectStart.Plain, PaletteFxSegment.UpperCrateriaRedFlashing),
        PaletteFxObject.CrateriaEscapeYellowLightning => new(ColourEffectStart.Plain, PaletteFxSegment.CrateriaEscapeYellowLightning),
        PaletteFxObject.ParlorEscapeSkyFlicker => new(ColourEffectStart.Plain, PaletteFxSegment.ParlorEscapeSkyFlicker),
        _ => throw new ArgumentOutOfRangeException(nameof(paletteFxObject), paletteFxObject, "Not a palette-FX object."),
    };
}
