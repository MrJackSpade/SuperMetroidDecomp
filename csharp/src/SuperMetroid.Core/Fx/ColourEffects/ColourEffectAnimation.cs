namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects' animations (#1739): one for each <see cref="ColourEffectId"/>, named for it, and one for each
/// suit Samus's heat shimmer can show. Each is its colours, its frames and how it loops (<see cref="ColourEffectAnimations"/>).
/// </summary>
public enum ColourEffectAnimation
{
    /// <summary>The animation of <see cref="ColourEffectId.FadeInSuperMetroidTitleLogo"/>.</summary>
    FadeInSuperMetroidTitleLogo = 1,
    /// <summary>The animation of <see cref="ColourEffectId.FadeInNintendoCopyright"/>.</summary>
    FadeInNintendoCopyright,
    /// <summary>The animation of <see cref="ColourEffectId.TitleScreenBabyMetroidTubeLight"/>.</summary>
    TitleScreenBabyMetroidTubeLight,
    /// <summary>The animation of <see cref="ColourEffectId.TitleScreenFlickeringDisplays"/>.</summary>
    TitleScreenFlickeringDisplays,
    /// <summary>The animation of <see cref="ColourEffectId.CutsceneGunshipEngineFlicker"/>.</summary>
    CutsceneGunshipEngineFlicker,
    /// <summary>The animation of <see cref="ColourEffectId.CutsceneCeresNavigationLightsSprite"/>.</summary>
    CutsceneCeresNavigationLightsSprite,
    /// <summary>The animation of <see cref="ColourEffectId.FadeInPlanetZebesText"/>.</summary>
    FadeInPlanetZebesText,
    /// <summary>The animation of <see cref="ColourEffectId.FadeOutPlanetZebesText"/>.</summary>
    FadeOutPlanetZebesText,
    /// <summary>The animation of <see cref="ColourEffectId.CutsceneCeresNavigationLightsBackground"/>.</summary>
    CutsceneCeresNavigationLightsBackground,
    /// <summary>The animation of <see cref="ColourEffectId.OldMotherBrainFightBackgroundLights"/>.</summary>
    OldMotherBrainFightBackgroundLights,
    /// <summary>The animation of <see cref="ColourEffectId.GunshipGlow"/>.</summary>
    GunshipGlow,
    /// <summary>The animation of <see cref="ColourEffectId.FadeOutZoomedOutExplodingZebes"/>.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary>The animation of <see cref="ColourEffectId.WidePartOfZebesExplosionForeground"/>.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary>The animation of <see cref="ColourEffectId.ZebesExplosionFinale"/>.</summary>
    ZebesExplosionFinale,
    /// <summary>The animation of <see cref="ColourEffectId.WhiteOutSpaceDuringZebesExplosion"/>.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary>The animation of <see cref="ColourEffectId.ZebesExplosionPlanetAfterglow"/>.</summary>
    ZebesExplosionPlanetAfterglow,
    /// <summary>The animation of <see cref="ColourEffectId.ExplodingZebesLava"/>.</summary>
    ExplodingZebesLava,
    /// <summary>The animation of <see cref="ColourEffectId.FadeOutExplodingZebesCrust"/>.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary>The animation of <see cref="ColourEffectId.FadeOutExplodingZebesGreyClouds"/>.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary>The animation of <see cref="ColourEffectId.GunshipEmergingFromZebesExplosion"/>.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary>The animation of <see cref="ColourEffectId.WidePartOfZebesExplosionBackground"/>.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary>The animation of <see cref="ColourEffectId.HyperBeam"/>.</summary>
    HyperBeam,
    /// <summary>The animation of <see cref="ColourEffectId.SamusLoadingPowerSuit"/>.</summary>
    SamusLoadingPowerSuit,
    /// <summary>The animation of <see cref="ColourEffectId.SamusLoadingVariaSuit"/>.</summary>
    SamusLoadingVariaSuit,
    /// <summary>The animation of <see cref="ColourEffectId.SamusLoadingGravitySuit"/>.</summary>
    SamusLoadingGravitySuit,
    /// <summary>The animation of <see cref="ColourEffectId.PostCreditsSuperMetroidIcon"/>.</summary>
    PostCreditsSuperMetroidIcon,
    /// <summary>The animation of <see cref="ColourEffectId.GreyOutTourianStatueDraygon"/>.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary>The animation of <see cref="ColourEffectId.GreyOutTourianStatueKraid"/>.</summary>
    GreyOutTourianStatueKraid,
    /// <summary>The animation of <see cref="ColourEffectId.GreyOutTourianStatueRidley"/>.</summary>
    GreyOutTourianStatueRidley,
    /// <summary>The animation of <see cref="ColourEffectId.GreyOutTourianStatuePhantoon"/>.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary>The animation of <see cref="ColourEffectId.BombTorizoBelly"/>.</summary>
    BombTorizoBelly,
    /// <summary>The animation of <see cref="ColourEffectId.GoldenTorizoBelly"/>.</summary>
    GoldenTorizoBelly,
    /// <summary>
    /// Samus's heat shimmer in the Power Suit (<see cref="ColourEffectId.SamusInHeat"/>): watches the heat, names her
    /// suit's colours, then runs its pulse, one frame for each <see cref="HeatPhase"/>, in phase order, round again.
    /// </summary>
    SamusInHeatPowerSuit,
    /// <summary>
    /// Samus's heat shimmer in the Varia Suit (<see cref="ColourEffectId.SamusInHeat"/>): watches the heat, names her
    /// suit's colours, then runs its pulse, one frame for each <see cref="HeatPhase"/>, in phase order, round again.
    /// </summary>
    SamusInHeatVariaSuit,
    /// <summary>
    /// Samus's heat shimmer in the Gravity Suit (<see cref="ColourEffectId.SamusInHeat"/>): watches the heat, names her
    /// suit's colours, then runs its pulse, one frame for each <see cref="HeatPhase"/>, in phase order, round again.
    /// </summary>
    SamusInHeatGravitySuit,
    /// <summary>The animation of <see cref="ColourEffectId.CrateriaLightning"/>.</summary>
    CrateriaLightning,
    /// <summary>The animation of <see cref="ColourEffectId.WreckedShipGreenLights"/>.</summary>
    WreckedShipGreenLights,
    /// <summary>The animation of <see cref="ColourEffectId.BrinstarBlueBackgroundSpores"/>.</summary>
    BrinstarBlueBackgroundSpores,
    /// <summary>The animation of <see cref="ColourEffectId.SporeSpawnBlueBackgroundSpores"/>.</summary>
    SporeSpawnBlueBackgroundSpores,
    /// <summary>The animation of <see cref="ColourEffectId.BrinstarRedBackgroundGlow"/>.</summary>
    BrinstarRedBackgroundGlow,
    /// <summary>The animation of <see cref="ColourEffectId.BeaconFlashing"/>.</summary>
    BeaconFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.NorfairForegroundAndHeatPhase"/>.</summary>
    NorfairForegroundAndHeatPhase,
    /// <summary>The animation of <see cref="ColourEffectId.NorfairGreenForegroundHeatGlow"/>.</summary>
    NorfairGreenForegroundHeatGlow,
    /// <summary>The animation of <see cref="ColourEffectId.NorfairPurpleForegroundHeatGlow"/>.</summary>
    NorfairPurpleForegroundHeatGlow,
    /// <summary>The animation of <see cref="ColourEffectId.NorfairDarkForegroundHeatGlow"/>.</summary>
    NorfairDarkForegroundHeatGlow,
    /// <summary>The animation of <see cref="ColourEffectId.MaridiaSandPits"/>.</summary>
    MaridiaSandPits,
    /// <summary>The animation of <see cref="ColourEffectId.MaridiaSandFalls"/>.</summary>
    MaridiaSandFalls,
    /// <summary>The animation of <see cref="ColourEffectId.MaridiaBackgroundWaterfalls"/>.</summary>
    MaridiaBackgroundWaterfalls,
    /// <summary>The animation of <see cref="ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs"/>.</summary>
    TourianGlowingArkanoidBlocksAndRedOrbs,
    /// <summary>The animation of <see cref="ColourEffectId.TourianShutterRedFlashing"/>.</summary>
    TourianShutterRedFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.TourianBackgroundRedFlashing"/>.</summary>
    TourianBackgroundRedFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.TourianGeneralLevelRedFlashing"/>.</summary>
    TourianGeneralLevelRedFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs"/>.</summary>
    TourianRedFlashingArkanoidBlocksAndRedOrbs,
    /// <summary>The animation of <see cref="ColourEffectId.OldTourianRedFlashing"/>.</summary>
    OldTourianRedFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.OldTourianBackgroundRailingsFlashYellow"/>.</summary>
    OldTourianBackgroundRailingsFlashYellow,
    /// <summary>The animation of <see cref="ColourEffectId.OldTourianBackgroundPanelsFlashYellow"/>.</summary>
    OldTourianBackgroundPanelsFlashYellow,
    /// <summary>The animation of <see cref="ColourEffectId.UpperCrateriaRedFlashing"/>.</summary>
    UpperCrateriaRedFlashing,
    /// <summary>The animation of <see cref="ColourEffectId.CrateriaEscapeYellowLightning"/>.</summary>
    CrateriaEscapeYellowLightning,
    /// <summary>The animation of <see cref="ColourEffectId.ParlorEscapeSkyFlicker"/>.</summary>
    ParlorEscapeSkyFlicker,
}
