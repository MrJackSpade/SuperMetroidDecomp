namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX objects' programs (#1298), one per bank-$8D list an object, a setup, a pre-instruction or a branch
/// enters, in address order. Generated from the verified native lists; see <see cref="PaletteFxAnimations"/>.
/// </summary>
public enum PaletteFxProgram
{
    /// <summary><c>InstList_PaletteFXObject_FadeInSuperMetroidTitleLogo</c> at $8D:C696.</summary>
    FadeInSuperMetroidTitleLogo,
    /// <summary><c>InstList_PaletteFXObject_FadeInNintendoBootLogo</c> at $8D:C7AC.</summary>
    FadeInNintendoBootLogo,
    /// <summary><c>InstList_PaletteFXObject_Common_FadeInLogo</c> at $8D:C7B0.</summary>
    CommonFadeInLogo,
    /// <summary><c>InstList_PaletteFXObject_FadeInNintendoCopyright</c> at $8D:C7F2.</summary>
    FadeInNintendoCopyright,
    /// <summary><c>InstList_PaletteFXObject_TitleScreenBabyMetroidTubeLight_0</c> at $8D:C7FA.</summary>
    TitleScreenBabyMetroidTubeLight0,
    /// <summary><c>InstList_PaletteFXObject_TitleScreenBabyMetroidTubeLight_1</c> at $8D:C7FE.</summary>
    TitleScreenBabyMetroidTubeLight1,
    /// <summary><c>InstList_PaletteFXObject_TitleScreenFlickeringDisplays_0</c> at $8D:C862.</summary>
    TitleScreenFlickeringDisplays0,
    /// <summary><c>InstList_PaletteFXObject_TitleScreenFlickeringDisplays_1</c> at $8D:C866.</summary>
    TitleScreenFlickeringDisplays1,
    /// <summary><c>InstList_PaletteFXObject_CutsceneGunshipEngineFlicker_0</c> at $8D:C87A.</summary>
    CutsceneGunshipEngineFlicker0,
    /// <summary><c>InstList_PaletteFXObject_CutsceneGunshipEngineFlicker_1</c> at $8D:C87E.</summary>
    CutsceneGunshipEngineFlicker1,
    /// <summary><c>InstList_PaletteFXObject_CutsceneCeresNavigationLight_Sprite</c> at $8D:C88E.</summary>
    CutsceneCeresNavigationLightSprite,
    /// <summary><c>InstList_PaletteFXObject_Common_CeresNavigationLights</c> at $8D:C892.</summary>
    CommonCeresNavigationLights,
    /// <summary><c>InstList_PaletteFXObject_CutsceneCeresNavigationLights_BG</c> at $8D:C906.</summary>
    CutsceneCeresNavigationLightsBG,
    /// <summary><c>InstList_PaletteFXObject_FadeInPlanetZebesText</c> at $8D:C90E.</summary>
    FadeInPlanetZebesText,
    /// <summary><c>InstList_PaletteFXObject_FadeOutPlanetZebesText</c> at $8D:C964.</summary>
    FadeOutPlanetZebesText,
    /// <summary><c>InstList_PaletteFXObject_OldMotherBrainFightBGLights_0</c> at $8D:C9BA.</summary>
    OldMotherBrainFightBGLights0,
    /// <summary><c>InstList_PaletteFXObject_OldMotherBrainFightBGLights_1</c> at $8D:C9BE.</summary>
    OldMotherBrainFightBGLights1,
    /// <summary><c>InstList_PaletteFXObject_GunshipGlow_0</c> at $8D:CA4E.</summary>
    GunshipGlow0,
    /// <summary><c>InstList_PaletteFXObject_GunshipGlow_1</c> at $8D:CA52.</summary>
    GunshipGlow1,
    /// <summary><c>InstList_PaletteFXObject_FadeOutZoomedOutExplodingZebes</c> at $8D:CAAA.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary><c>InstList_PaletteFXObject_WidePartOfZebesExplosion_Foreground</c> at $8D:CB3C.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary><c>InstList_PaletteFXObject_ZebesExplosionFinale</c> at $8D:CD62.</summary>
    ZebesExplosionFinale,
    /// <summary><c>InstList_PaletteFXObject_WidePartOfZebesExplosion_Background</c> at $8D:D362.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary><c>InstList_PaletteFXObject_WhiteOutSpaceDuringZebesExplosion</c> at $8D:D36A.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary><c>InstList_PaletteFXObject_Common_ZebesExplosion</c> at $8D:D36E.</summary>
    CommonZebesExplosion,
    /// <summary><c>InstList_PaletteFXObject_ZebesExplosionPlanetAfterGlow_0</c> at $8D:D3CA.</summary>
    ZebesExplosionPlanetAfterGlow0,
    /// <summary><c>InstList_PaletteFXObject_ZebesExplosionPlanetAfterGlow_1</c> at $8D:D3CE.</summary>
    ZebesExplosionPlanetAfterGlow1,
    /// <summary><c>InstList_PaletteFXObject_ExplodingZebesLava_0</c> at $8D:D44A.</summary>
    ExplodingZebesLava0,
    /// <summary><c>InstList_PaletteFXObject_ExplodingZebesLava_1</c> at $8D:D44E.</summary>
    ExplodingZebesLava1,
    /// <summary><c>InstList_PaletteFXObject_FadeOutExplodingZebesCrust</c> at $8D:D48E.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary><c>InstList_PaletteFXObject_FadeOutExplodingZebesGreyClouds</c> at $8D:D5A4.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary><c>InstList_PaletteFXObject_GunshipEmergingFromZebesExplosion</c> at $8D:D6BA.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary><c>InstList_PaletteFXObject_HyperBeam_0</c> at $8D:D900.</summary>
    HyperBeam0,
    /// <summary><c>InstList_PaletteFXObject_HyperBeam_1</c> at $8D:D904.</summary>
    HyperBeam1,
    /// <summary><c>UNUSED_InstList_PaletteFXObject_8DD9D0</c> at $8D:D9D0.</summary>
    ListD9D0,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_PowerSuit_0</c> at $8D:DB62.</summary>
    SamusLoadingPowerSuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_PowerSuit_1</c> at $8D:DB69.</summary>
    SamusLoadingPowerSuit1,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_PowerSuit_2</c> at $8D:DBB8.</summary>
    SamusLoadingPowerSuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_PowerSuit_3</c> at $8D:DC07.</summary>
    SamusLoadingPowerSuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_PowerSuit_4</c> at $8D:DC56.</summary>
    SamusLoadingPowerSuit4,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_VairaSuit_0</c> at $8D:DCC8.</summary>
    SamusLoadingVairaSuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_VairaSuit_1</c> at $8D:DCCF.</summary>
    SamusLoadingVairaSuit1,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_VairaSuit_2</c> at $8D:DD1E.</summary>
    SamusLoadingVairaSuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_VairaSuit_3</c> at $8D:DD6D.</summary>
    SamusLoadingVairaSuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_VairaSuit_4</c> at $8D:DDBC.</summary>
    SamusLoadingVairaSuit4,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_GravitySuit_0</c> at $8D:DE2E.</summary>
    SamusLoadingGravitySuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_GravitySuit_1</c> at $8D:DE35.</summary>
    SamusLoadingGravitySuit1,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_GravitySuit_2</c> at $8D:DE84.</summary>
    SamusLoadingGravitySuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_GravitySuit_3</c> at $8D:DED3.</summary>
    SamusLoadingGravitySuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusLoading_GravitySuit_4</c> at $8D:DF22.</summary>
    SamusLoadingGravitySuit4,
    /// <summary><c>InstList_PaletteFXObject_PostCreditsSuperMetroidIconGlare</c> at $8D:DF94.</summary>
    PostCreditsSuperMetroidIconGlare,
    /// <summary><c>InstList_PaletteFXObject_Nothing</c> at $8D:E220.</summary>
    Nothing,
    /// <summary><c>InstList_PaletteFXObject_GreyOutTourianStatue_Draygon</c> at $8D:E222.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary><c>InstList_PaletteFXObject_GreyOutTourianStatue_Kraid</c> at $8D:E22A.</summary>
    GreyOutTourianStatueKraid,
    /// <summary><c>InstList_PaletteFXObject_GreyOutTourianStatue_Ridley</c> at $8D:E232.</summary>
    GreyOutTourianStatueRidley,
    /// <summary><c>InstList_PaletteFXObject_GreyOutTourianStatue_Phantoon</c> at $8D:E23A.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary><c>InstList_PaletteFXObject_Common_GreyOutTourianStatue</c> at $8D:E23E.</summary>
    CommonGreyOutTourianStatue,
    /// <summary><c>InstList_PaletteFXObject_BombTorizoBelly_0</c> at $8D:E2E9.</summary>
    BombTorizoBelly0,
    /// <summary><c>InstList_PaletteFXObject_BombTorizoBelly_1</c> at $8D:E2F1.</summary>
    BombTorizoBelly1,
    /// <summary><c>InstList_PaletteFXObject_GoldenTorizoBelly_0</c> at $8D:E331.</summary>
    GoldenTorizoBelly0,
    /// <summary><c>InstList_PaletteFXObject_GoldenTorizoBelly_1</c> at $8D:E339.</summary>
    GoldenTorizoBelly1,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_0</c> at $8D:E45E.</summary>
    SamusInHeatPowerSuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_1</c> at $8D:E466.</summary>
    SamusInHeatPowerSuit1,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_2</c> at $8D:E488.</summary>
    SamusInHeatPowerSuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_3</c> at $8D:E4AA.</summary>
    SamusInHeatPowerSuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_4</c> at $8D:E4CC.</summary>
    SamusInHeatPowerSuit4,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_5</c> at $8D:E4EE.</summary>
    SamusInHeatPowerSuit5,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_6</c> at $8D:E510.</summary>
    SamusInHeatPowerSuit6,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_7</c> at $8D:E532.</summary>
    SamusInHeatPowerSuit7,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_8</c> at $8D:E554.</summary>
    SamusInHeatPowerSuit8,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_9</c> at $8D:E576.</summary>
    SamusInHeatPowerSuit9,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_A</c> at $8D:E598.</summary>
    SamusInHeatPowerSuitA,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_B</c> at $8D:E5BA.</summary>
    SamusInHeatPowerSuitB,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_C</c> at $8D:E5DC.</summary>
    SamusInHeatPowerSuitC,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_D</c> at $8D:E5FE.</summary>
    SamusInHeatPowerSuitD,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_E</c> at $8D:E620.</summary>
    SamusInHeatPowerSuitE,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_F</c> at $8D:E642.</summary>
    SamusInHeatPowerSuitF,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_PowerSuit_10</c> at $8D:E664.</summary>
    SamusInHeatPowerSuit10,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_0</c> at $8D:E68A.</summary>
    SamusInHeatVariaSuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_2</c> at $8D:E692.</summary>
    SamusInHeatVariaSuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_3</c> at $8D:E6B4.</summary>
    SamusInHeatVariaSuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_4</c> at $8D:E6D6.</summary>
    SamusInHeatVariaSuit4,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_5</c> at $8D:E6F8.</summary>
    SamusInHeatVariaSuit5,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_6</c> at $8D:E71A.</summary>
    SamusInHeatVariaSuit6,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_7</c> at $8D:E73C.</summary>
    SamusInHeatVariaSuit7,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_8</c> at $8D:E75E.</summary>
    SamusInHeatVariaSuit8,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_9</c> at $8D:E780.</summary>
    SamusInHeatVariaSuit9,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_A</c> at $8D:E7A2.</summary>
    SamusInHeatVariaSuitA,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_B</c> at $8D:E7C4.</summary>
    SamusInHeatVariaSuitB,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_C</c> at $8D:E7E6.</summary>
    SamusInHeatVariaSuitC,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_D</c> at $8D:E808.</summary>
    SamusInHeatVariaSuitD,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_E</c> at $8D:E82A.</summary>
    SamusInHeatVariaSuitE,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_F</c> at $8D:E84C.</summary>
    SamusInHeatVariaSuitF,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_10</c> at $8D:E86E.</summary>
    SamusInHeatVariaSuit10,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_VariaSuit_11</c> at $8D:E890.</summary>
    SamusInHeatVariaSuit11,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_0</c> at $8D:E8B6.</summary>
    SamusInHeatGravitySuit0,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_2</c> at $8D:E8BE.</summary>
    SamusInHeatGravitySuit2,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_3</c> at $8D:E8E0.</summary>
    SamusInHeatGravitySuit3,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_4</c> at $8D:E902.</summary>
    SamusInHeatGravitySuit4,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_5</c> at $8D:E924.</summary>
    SamusInHeatGravitySuit5,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_6</c> at $8D:E946.</summary>
    SamusInHeatGravitySuit6,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_7</c> at $8D:E968.</summary>
    SamusInHeatGravitySuit7,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_8</c> at $8D:E98A.</summary>
    SamusInHeatGravitySuit8,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_9</c> at $8D:E9AC.</summary>
    SamusInHeatGravitySuit9,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_A</c> at $8D:E9CE.</summary>
    SamusInHeatGravitySuitA,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_B</c> at $8D:E9F0.</summary>
    SamusInHeatGravitySuitB,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_C</c> at $8D:EA12.</summary>
    SamusInHeatGravitySuitC,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_D</c> at $8D:EA34.</summary>
    SamusInHeatGravitySuitD,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_E</c> at $8D:EA56.</summary>
    SamusInHeatGravitySuitE,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_F</c> at $8D:EA78.</summary>
    SamusInHeatGravitySuitF,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_10</c> at $8D:EA9A.</summary>
    SamusInHeatGravitySuit10,
    /// <summary><c>InstList_PaletteFXObject_SamusInHeat_GravitySuit_11</c> at $8D:EABC.</summary>
    SamusInHeatGravitySuit11,
    /// <summary><c>InstList_PaletteFXObject_WreckedShip1_0</c> at $8D:EAE2.</summary>
    WreckedShip10,
    /// <summary><c>InstList_PaletteFXObject_WreckedShip1_1</c> at $8D:EAE6.</summary>
    WreckedShip11,
    /// <summary><c>InstList_PaletteFXObject_Crateria1_0</c> at $8D:EB3B.</summary>
    Crateria10,
    /// <summary><c>InstList_PaletteFXObject_Crateria1_1</c> at $8D:EB43.</summary>
    Crateria11,
    /// <summary><c>InstList_PaletteFXObject_Crateria1_2</c> at $8D:EB5A.</summary>
    Crateria12,
    /// <summary><c>InstList_PaletteFXObject_Crateria1_3</c> at $8D:EC01.</summary>
    Crateria13,
    /// <summary><c>UNUSED_InstList_PaletteFXObject_DarkLightning_0_8DEC6E</c> at $8D:EC6E.</summary>
    ListEC6E,
    /// <summary><c>UNUSED_InstList_PaletteFXObject_DarkLightning_1_8DEC76</c> at $8D:EC76.</summary>
    CrateriaAboveY380Second,
    /// <summary><c>UNUSED_InstList_PaletteFXObject_DarkLightning_2_8DEC8B</c> at $8D:EC8B.</summary>
    ListEC8B,
    /// <summary><c>UNUSED_InstList_PaletteFXObject_DarkLightning_3_8DED34</c> at $8D:ED34.</summary>
    ListED34,
    /// <summary><c>InstList_PaletteFXObject_Brinstar1_0</c> at $8D:ED99.</summary>
    Brinstar10,
    /// <summary><c>InstList_PaletteFXObject_Brinstar1_1</c> at $8D:ED9D.</summary>
    Brinstar11,
    /// <summary><c>InstList_PaletteFXObject_Brinstar8_0</c> at $8D:EE2D.</summary>
    Brinstar80,
    /// <summary><c>InstList_PaletteFXObject_Brinstar8_1</c> at $8D:EE35.</summary>
    Brinstar81,
    /// <summary><c>InstList_PaletteFXObject_Brinstar2_0</c> at $8D:EED7.</summary>
    Brinstar20,
    /// <summary><c>InstList_PaletteFXObject_Brinstar2_1</c> at $8D:EEDB.</summary>
    Brinstar21,
    /// <summary><c>InstList_PaletteFXObject_Crateria80_Brinstar4_0</c> at $8D:EFF7.</summary>
    Crateria80Brinstar40,
    /// <summary><c>InstList_PaletteFXObject_Crateria80_Brinstar4_1</c> at $8D:EFFB.</summary>
    Crateria80Brinstar41,
    /// <summary><c>InstList_PaletteFXObject_Norfair2_0</c> at $8D:F08E.</summary>
    Norfair20,
    /// <summary><c>InstList_PaletteFXObject_Norfair2_1</c> at $8D:F092.</summary>
    Norfair21,
    /// <summary><c>InstList_PaletteFXObject_Norfair4_0</c> at $8D:F1D1.</summary>
    Norfair40,
    /// <summary><c>InstList_PaletteFXObject_Norfair4_1</c> at $8D:F1D5.</summary>
    Norfair41,
    /// <summary><c>InstList_PaletteFXObject_Norfair8_0</c> at $8D:F2D9.</summary>
    Norfair80,
    /// <summary><c>InstList_PaletteFXObject_Norfair8_1</c> at $8D:F2DD.</summary>
    Norfair81,
    /// <summary><c>InstList_PaletteFXObject_Norfair10_0</c> at $8D:F3E1.</summary>
    Norfair100,
    /// <summary><c>InstList_PaletteFXObject_Norfair10_1</c> at $8D:F3E5.</summary>
    Norfair101,
    /// <summary><c>InstList_PaletteFXObject_Maridia1_0</c> at $8D:F4E9.</summary>
    Maridia10,
    /// <summary><c>InstList_PaletteFXObject_Maridia1_1</c> at $8D:F4ED.</summary>
    Maridia11,
    /// <summary><c>InstList_PaletteFXObject_Maridia2_0</c> at $8D:F541.</summary>
    Maridia20,
    /// <summary><c>InstList_PaletteFXObject_Maridia2_1</c> at $8D:F545.</summary>
    Maridia21,
    /// <summary><c>InstList_PaletteFXObject_Maridia4_0</c> at $8D:F579.</summary>
    Maridia40,
    /// <summary><c>InstList_PaletteFXObject_Maridia4_1</c> at $8D:F57D.</summary>
    Maridia41,
    /// <summary><c>InstList_PaletteFXObject_Tourian4</c> at $8D:F62A.</summary>
    Tourian4,
    /// <summary><c>InstList_PaletteFXObject_Tourian2</c> at $8D:F632.</summary>
    Tourian2,
    /// <summary><c>InstList_PaletteFXObject_Common_Tourian2_Tourian4_0</c> at $8D:F636.</summary>
    CommonTourian2Tourian40,
    /// <summary><c>InstList_PaletteFXObject_Common_Tourian2_Tourian4_1</c> at $8D:F63A.</summary>
    CommonTourian2Tourian41,
    /// <summary><c>InstList_PaletteFXObject_Tourian8_0</c> at $8D:F7A9.</summary>
    Tourian80,
    /// <summary><c>InstList_PaletteFXObject_Tourian8_1</c> at $8D:F7AD.</summary>
    Tourian81,
    /// <summary><c>InstList_PaletteFXObject_Tourian10_0</c> at $8D:F891.</summary>
    Tourian100,
    /// <summary><c>InstList_PaletteFXObject_Tourian10_1</c> at $8D:F895.</summary>
    Tourian101,
    /// <summary><c>InstList_PaletteFXObject_Tourian20</c> at $8D:F941.</summary>
    Tourian20,
    /// <summary><c>InstList_PaletteFXObject_Tourian40</c> at $8D:F949.</summary>
    Tourian40,
    /// <summary><c>InstList_PaletteFXObject_Common_Tourian20_Tourian40</c> at $8D:F94D.</summary>
    CommonTourian20Tourian40,
    /// <summary><c>InstList_PaletteFXObject_Crateria8_0</c> at $8D:FA69.</summary>
    Crateria80,
    /// <summary><c>InstList_PaletteFXObject_Crateria8_1</c> at $8D:FA6D.</summary>
    Crateria81,
    /// <summary><c>InstList_PaletteFXObject_Crateria10_0</c> at $8D:FBC1.</summary>
    Crateria100,
    /// <summary><c>InstList_PaletteFXObject_Crateria10_1</c> at $8D:FBC5.</summary>
    Crateria101,
    /// <summary><c>InstList_PaletteFXObject_Crateria20_0</c> at $8D:FC5F.</summary>
    Crateria200,
    /// <summary><c>InstList_PaletteFXObject_Crateria20_1</c> at $8D:FC63.</summary>
    Crateria201,
    /// <summary><c>InstList_PaletteFXObject_Crateria2_0</c> at $8D:FCFD.</summary>
    Crateria20,
    /// <summary><c>InstList_PaletteFXObject_Crateria2_1</c> at $8D:FD01.</summary>
    Crateria21,
    /// <summary><c>InstList_PaletteFXObject_Crateria4_0</c> at $8D:FE01.</summary>
    Crateria40,
    /// <summary><c>InstList_PaletteFXObject_Crateria4_1</c> at $8D:FE05.</summary>
    Crateria41,
    /// <summary><c>InstList_PaletteFXObject_Crateria40_0</c> at $8D:FF27.</summary>
    Crateria400,
    /// <summary><c>InstList_PaletteFXObject_Crateria40_1</c> at $8D:FF2B.</summary>
    Crateria401,
}
