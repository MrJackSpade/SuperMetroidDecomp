namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The bank-$8D palette-FX objects, named by the disassembly's <c>PaletteFXObjects_</c> labels. Each selects a setup
/// and an initial program (<see cref="RoomPaletteFxDefinitions"/>); a slot holds the object it runs.
/// </summary>
public enum PaletteFxObject
{
    /// <summary><c>PaletteFXObjects_FadeInSuperMetroidTitleLogo</c>.</summary>
    FadeInSuperMetroidTitleLogo = 1,
    /// <summary><c>PaletteFXObjects_FadeInNintendoBootLogoForUnusedCode</c>.</summary>
    FadeInNintendoBootLogo,
    /// <summary><c>PaletteFXObjects_FadeInNintendoCopyright</c>.</summary>
    FadeInNintendoCopyright,
    /// <summary><c>PaletteFXObjects_TitleScreenBabyMetroidTubeLight</c>.</summary>
    TitleScreenBabyMetroidTubeLight,
    /// <summary><c>PaletteFXObjects_TitleScreenFlickeringDisplays</c>.</summary>
    TitleScreenFlickeringDisplays,
    /// <summary><c>PaletteFXObjects_CutsceneGunshipEngineFlicker</c>.</summary>
    CutsceneGunshipEngineFlicker,
    /// <summary><c>PaletteFXObjects_CutsceneCeresNavigationLights_SpriteCeres</c>.</summary>
    CutsceneCeresNavigationLightsSprite,
    /// <summary><c>PaletteFXObjects_FadeInPlanetZebesText</c>.</summary>
    FadeInPlanetZebesText,
    /// <summary><c>PaletteFXObjects_FadeOutPlanetZebesText</c>.</summary>
    FadeOutPlanetZebesText,
    /// <summary><c>PaletteFXObjects_CutsceneCeresNavigationLights_BGCeres</c>.</summary>
    CutsceneCeresNavigationLightsBackground,
    /// <summary><c>PaletteFXObjects_OldMotherBrainFightBackgroundLights</c>.</summary>
    OldMotherBrainFightBackgroundLights,
    /// <summary><c>PaletteFXObjects_GunshipGlow</c>.</summary>
    GunshipGlow,
    /// <summary><c>PaletteFXObjects_FadeOutZoomedOutExplodingZebes</c>.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary><c>PaletteFXObjects_WidePartOfZebesExplosion_Foreground</c>.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary><c>PaletteFXObjects_ZebesExplosionFinale</c>.</summary>
    ZebesExplosionFinale,
    /// <summary><c>PaletteFXObjects_WhiteOutSpaceDuringZebesExplosion</c>.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary><c>PaletteFXObjects_ZebesExplosionPlanetAfterglow</c>.</summary>
    ZebesExplosionPlanetAfterglow,
    /// <summary><c>PaletteFXObjects_ExplodingZebesLava</c>.</summary>
    ExplodingZebesLava,
    /// <summary><c>PaletteFXObjects_FadeOutExplodingZebesCrust</c>.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary><c>PaletteFXObjects_FadeOutExplodingZebesGreyClouds</c>.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary><c>PaletteFXObjects_GunshipEmergineFromZebesExplosion</c>.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary><c>PaletteFXObjects_WidePartOfZebesExplosion_Background</c>.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary><c>UNUSED_PaletteFXObjects_8DE1EC</c>.</summary>
    UnusedCinematicFade,
    /// <summary><c>PaletteFXObjects_HyperBeam</c>.</summary>
    HyperBeam,
    /// <summary><c>PaletteFXObjects_SamusLoading_PowerSuit</c>.</summary>
    SamusLoadingPowerSuit,
    /// <summary><c>PaletteFXObjects_SamusLoading_VariaSuit</c>.</summary>
    SamusLoadingVariaSuit,
    /// <summary><c>PaletteFXObjects_SamusLoading_GravitySuit</c>.</summary>
    SamusLoadingGravitySuit,
    /// <summary><c>PaletteFXObjects_PostCreditsSuperMetroidIcon</c>.</summary>
    PostCreditsSuperMetroidIcon,
    /// <summary><c>PaletteFXObjects_Nothing</c>.</summary>
    Nothing,
    /// <summary><c>PaletteFXObjects_GreyOutTourianStatue_Draygon</c>.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary><c>PaletteFXObjects_GreyOutTourianStatue_Kraid</c>.</summary>
    GreyOutTourianStatueKraid,
    /// <summary><c>PaletteFXObjects_GreyOutTourianStatue_Ridley</c>.</summary>
    GreyOutTourianStatueRidley,
    /// <summary><c>PaletteFXObjects_GreyOutTourianStatue_Phantoon</c>.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary><c>PaletteFXObjects_BombTorizoBelly</c>.</summary>
    BombTorizoBelly,
    /// <summary><c>PaletteFXObjects_GoldenTorizoBelly</c>.</summary>
    GoldenTorizoBelly,
    /// <summary><c>PaletteFXObjects_Norfair1_Tourian1</c>.</summary>
    SamusInHeat,
    /// <summary><c>PaletteFXObjects_Crateria1_Lightning</c>.</summary>
    CrateriaLightning,
    /// <summary><c>UNUSED_PaletteFXObjects_DarkLightning_8DF769</c>.</summary>
    UnusedDarkLightning,
    /// <summary><c>PaletteFXObjects_WreckedShip1_GreenLights</c>.</summary>
    WreckedShipGreenLights,
    /// <summary><c>UNUSED_PaletteFXObjects_WreckedShip1_GreenLights_dup_8DF771</c>.</summary>
    UnusedWreckedShipGreenLightsDuplicate,
    /// <summary><c>PaletteFXObjects_Brinstar1_BlueBackgroundSpores</c>.</summary>
    BrinstarBlueBackgroundSpores,
    /// <summary><c>PaletteFXObjects_Brinstar8_SporeSpawnBlueBackgroundSpores</c>.</summary>
    SporeSpawnBlueBackgroundSpores,
    /// <summary><c>PaletteFXObjects_Brinstar2_RedBackgroundGlow</c>.</summary>
    BrinstarRedBackgroundGlow,
    /// <summary><c>PaletteFXObjects_Crateria80_Brinstar4_BeaconFlashing</c>.</summary>
    BeaconFlashing,
    /// <summary><c>PaletteFXObjects_Norfair2</c>.</summary>
    NorfairForegroundAndHeatPhase,
    /// <summary><c>PaletteFXObjects_Norfair4</c>.</summary>
    NorfairForegroundPalette4,
    /// <summary><c>PaletteFXObjects_Norfair8</c>.</summary>
    NorfairForegroundPalette5,
    /// <summary><c>PaletteFXObjects_Norfair10</c>.</summary>
    NorfairForegroundPalette6,
    /// <summary><c>PaletteFXObjects_Maridia1_SandPits</c>.</summary>
    MaridiaSandPits,
    /// <summary><c>PaletteFXObjects_Maridia2_SandFalls</c>.</summary>
    MaridiaSandFalls,
    /// <summary><c>PaletteFXObjects_Maridia4_BackgroundWaterfalls</c>.</summary>
    MaridiaBackgroundWaterfalls,
    /// <summary><c>PaletteFXObjects_Tourian2_GlowingArkanoidBlocksAndRedOrbs</c>.</summary>
    TourianGlowingArkanoidBlocksAndRedOrbs,
    /// <summary><c>UNUSED_PaletteFXObjects_Tourian4_8DF7A5</c>.</summary>
    UnusedTourianGlowClone,
    /// <summary><c>PaletteFXObjects_Tourian8_ShutterRedFlashing</c>.</summary>
    TourianShutterRedFlashing,
    /// <summary><c>PaletteFXObjects_Tourian10_BackgroundRedFlashing</c>.</summary>
    TourianBackgroundRedFlashing,
    /// <summary><c>PaletteFXObjects_Tourian20_GeneralLevelRedFlashing</c>.</summary>
    TourianGeneralLevelRedFlashing,
    /// <summary><c>PaletteFXObjects_Tourian40_RedFlashingArkanoidBlocksRedOrbs</c>.</summary>
    TourianRedFlashingArkanoidBlocksAndRedOrbs,
    /// <summary><c>PaletteFXObjects_Crateria8_OldTourianRedFlashing</c>.</summary>
    OldTourianRedFlashing,
    /// <summary><c>PaletteFXObjects_Crateria10_OldTourianBGRailingsFlashYellow</c>.</summary>
    OldTourianBackgroundRailingsFlashYellow,
    /// <summary><c>PaletteFXObjects_Crateria20_OldTourianBGPanelsFlashYellow</c>.</summary>
    OldTourianBackgroundPanelsFlashYellow,
    /// <summary><c>PaletteFXObjects_Crateria2_UpperCrateriaRedFlashing</c>.</summary>
    UpperCrateriaRedFlashing,
    /// <summary><c>PaletteFXObjects_Crateria4_YellowLightning</c>.</summary>
    CrateriaEscapeYellowLightning,
    /// <summary><c>PaletteFXObjects_Crateria40_SlightlyModifiesAPixelInCREBlock</c>.</summary>
    CrateriaEscapeCreBlockPixel,
}
