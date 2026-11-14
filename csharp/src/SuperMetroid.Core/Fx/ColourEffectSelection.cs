using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects how each colour effect starts and the animation it plays.
/// </summary>
internal static class ColourEffectSelection
{
    /// <summary>How <paramref name="colourEffect"/> starts, and the animation it starts.</summary>
    /// <remarks>Distinct effects may share an animation. The intro's start only watched a condition that does nothing, so
    /// the Mother Brain fight scene's lights start plainly (#1554). Both fields are independently checked against
    /// the supported Japanese and North American v1.0 release and the pinned reference sources (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    internal static ColourEffectDefinition Get(ColourEffectId colourEffect) => colourEffect switch
    {
        ColourEffectId.FadeInSuperMetroidTitleLogo => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeInSuperMetroidTitleLogo),
        ColourEffectId.FadeInNintendoCopyright => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeInNintendoCopyright),
        ColourEffectId.TitleScreenBabyMetroidTubeLight => new(ColourEffectStart.Plain, ColourEffectAnimation.TitleScreenBabyMetroidTubeLight),
        ColourEffectId.TitleScreenFlickeringDisplays => new(ColourEffectStart.Plain, ColourEffectAnimation.TitleScreenFlickeringDisplays),
        ColourEffectId.CutsceneGunshipEngineFlicker => new(ColourEffectStart.Plain, ColourEffectAnimation.CutsceneGunshipEngineFlicker),
        ColourEffectId.CutsceneCeresNavigationLightsSprite => new(ColourEffectStart.Plain, ColourEffectAnimation.CutsceneCeresNavigationLightsSprite),
        ColourEffectId.FadeInPlanetZebesText => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeInPlanetZebesText),
        ColourEffectId.FadeOutPlanetZebesText => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeOutPlanetZebesText),
        ColourEffectId.CutsceneCeresNavigationLightsBackground => new(ColourEffectStart.Plain, ColourEffectAnimation.CutsceneCeresNavigationLightsBackground),
        ColourEffectId.OldMotherBrainFightBackgroundLights => new(ColourEffectStart.Plain, ColourEffectAnimation.OldMotherBrainFightBackgroundLights),
        ColourEffectId.GunshipGlow => new(ColourEffectStart.Plain, ColourEffectAnimation.GunshipGlow),
        ColourEffectId.FadeOutZoomedOutExplodingZebes => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeOutZoomedOutExplodingZebes),
        ColourEffectId.WidePartOfZebesExplosionForeground => new(ColourEffectStart.Plain, ColourEffectAnimation.WidePartOfZebesExplosionForeground),
        ColourEffectId.ZebesExplosionFinale => new(ColourEffectStart.Plain, ColourEffectAnimation.ZebesExplosionFinale),
        ColourEffectId.WhiteOutSpaceDuringZebesExplosion => new(ColourEffectStart.Plain, ColourEffectAnimation.WhiteOutSpaceDuringZebesExplosion),
        ColourEffectId.ZebesExplosionPlanetAfterglow => new(ColourEffectStart.Plain, ColourEffectAnimation.ZebesExplosionPlanetAfterglow),
        ColourEffectId.ExplodingZebesLava => new(ColourEffectStart.Plain, ColourEffectAnimation.ExplodingZebesLava),
        ColourEffectId.FadeOutExplodingZebesCrust => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeOutExplodingZebesCrust),
        ColourEffectId.FadeOutExplodingZebesGreyClouds => new(ColourEffectStart.Plain, ColourEffectAnimation.FadeOutExplodingZebesGreyClouds),
        ColourEffectId.GunshipEmergingFromZebesExplosion => new(ColourEffectStart.Plain, ColourEffectAnimation.GunshipEmergingFromZebesExplosion),
        ColourEffectId.WidePartOfZebesExplosionBackground => new(ColourEffectStart.Plain, ColourEffectAnimation.WidePartOfZebesExplosionBackground),
        ColourEffectId.HyperBeam => new(ColourEffectStart.Plain, ColourEffectAnimation.HyperBeam),
        ColourEffectId.SamusLoadingPowerSuit => new(ColourEffectStart.Plain, ColourEffectAnimation.SamusLoadingPowerSuit),
        ColourEffectId.SamusLoadingVariaSuit => new(ColourEffectStart.Plain, ColourEffectAnimation.SamusLoadingVariaSuit),
        ColourEffectId.SamusLoadingGravitySuit => new(ColourEffectStart.Plain, ColourEffectAnimation.SamusLoadingGravitySuit),
        ColourEffectId.PostCreditsSuperMetroidIcon => new(ColourEffectStart.Plain, ColourEffectAnimation.PostCreditsSuperMetroidIcon),
        ColourEffectId.GreyOutTourianStatueDraygon => new(ColourEffectStart.Plain, ColourEffectAnimation.GreyOutTourianStatueDraygon),
        ColourEffectId.GreyOutTourianStatueKraid => new(ColourEffectStart.Plain, ColourEffectAnimation.GreyOutTourianStatueKraid),
        ColourEffectId.GreyOutTourianStatueRidley => new(ColourEffectStart.Plain, ColourEffectAnimation.GreyOutTourianStatueRidley),
        ColourEffectId.GreyOutTourianStatuePhantoon => new(ColourEffectStart.Plain, ColourEffectAnimation.GreyOutTourianStatuePhantoon),
        ColourEffectId.BombTorizoBelly => new(ColourEffectStart.Plain, ColourEffectAnimation.BombTorizoBelly),
        ColourEffectId.GoldenTorizoBelly => new(ColourEffectStart.Plain, ColourEffectAnimation.GoldenTorizoBelly),
        ColourEffectId.SamusInHeat => new(ColourEffectStart.ChoosesHeatAnimationForSuit, ColourEffectAnimation.SamusInHeatPowerSuit),
        ColourEffectId.CrateriaLightning => new(ColourEffectStart.Plain, ColourEffectAnimation.CrateriaLightning),
        ColourEffectId.WreckedShipGreenLights => new(ColourEffectStart.Plain, ColourEffectAnimation.WreckedShipGreenLights),
        ColourEffectId.BrinstarBlueBackgroundSpores => new(ColourEffectStart.Plain, ColourEffectAnimation.BrinstarBlueBackgroundSpores),
        ColourEffectId.SporeSpawnBlueBackgroundSpores => new(ColourEffectStart.SkippedOnceAreaMiniBossIsDead, ColourEffectAnimation.SporeSpawnBlueBackgroundSpores),
        ColourEffectId.BrinstarRedBackgroundGlow => new(ColourEffectStart.Plain, ColourEffectAnimation.BrinstarRedBackgroundGlow),
        ColourEffectId.BeaconFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.BeaconFlashing),
        ColourEffectId.NorfairForegroundAndHeatPhase => new(ColourEffectStart.Plain, ColourEffectAnimation.NorfairForegroundAndHeatPhase),
        ColourEffectId.NorfairGreenForegroundHeatGlow => new(ColourEffectStart.Plain, ColourEffectAnimation.NorfairGreenForegroundHeatGlow),
        ColourEffectId.NorfairPurpleForegroundHeatGlow => new(ColourEffectStart.Plain, ColourEffectAnimation.NorfairPurpleForegroundHeatGlow),
        ColourEffectId.NorfairDarkForegroundHeatGlow => new(ColourEffectStart.Plain, ColourEffectAnimation.NorfairDarkForegroundHeatGlow),
        ColourEffectId.MaridiaSandPits => new(ColourEffectStart.Plain, ColourEffectAnimation.MaridiaSandPits),
        ColourEffectId.MaridiaSandFalls => new(ColourEffectStart.Plain, ColourEffectAnimation.MaridiaSandFalls),
        ColourEffectId.MaridiaBackgroundWaterfalls => new(ColourEffectStart.Plain, ColourEffectAnimation.MaridiaBackgroundWaterfalls),
        ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, ColourEffectAnimation.TourianGlowingArkanoidBlocksAndRedOrbs),
        ColourEffectId.TourianShutterRedFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.TourianShutterRedFlashing),
        ColourEffectId.TourianBackgroundRedFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.TourianBackgroundRedFlashing),
        ColourEffectId.TourianGeneralLevelRedFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.TourianGeneralLevelRedFlashing),
        ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs => new(ColourEffectStart.Plain, ColourEffectAnimation.TourianRedFlashingArkanoidBlocksAndRedOrbs),
        ColourEffectId.OldTourianRedFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.OldTourianRedFlashing),
        ColourEffectId.OldTourianBackgroundRailingsFlashYellow => new(ColourEffectStart.Plain, ColourEffectAnimation.OldTourianBackgroundRailingsFlashYellow),
        ColourEffectId.OldTourianBackgroundPanelsFlashYellow => new(ColourEffectStart.Plain, ColourEffectAnimation.OldTourianBackgroundPanelsFlashYellow),
        ColourEffectId.UpperCrateriaRedFlashing => new(ColourEffectStart.Plain, ColourEffectAnimation.UpperCrateriaRedFlashing),
        ColourEffectId.CrateriaEscapeYellowLightning => new(ColourEffectStart.Plain, ColourEffectAnimation.CrateriaEscapeYellowLightning),
        ColourEffectId.ParlorEscapeSkyFlicker => new(ColourEffectStart.Plain, ColourEffectAnimation.ParlorEscapeSkyFlicker),
        _ => throw new ArgumentOutOfRangeException(nameof(colourEffect), colourEffect, "Not a colour effect."),
    };
}
