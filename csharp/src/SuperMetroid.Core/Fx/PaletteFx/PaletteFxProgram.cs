namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX objects' programs (#1298), one per entry point an object, a setup, a frame behaviour or a branch
/// enters, in authored order. Generated from the verified animation scripts; see <see cref="PaletteFxAnimations"/>.
/// </summary>
public enum PaletteFxProgram
{
    /// <summary>The fade in Super Metroid title logo program.</summary>
    FadeInSuperMetroidTitleLogo,
    /// <summary>The fade in Nintendo boot logo program.</summary>
    FadeInNintendoBootLogo,
    /// <summary>The shared fade in logo program its variants continue into.</summary>
    CommonFadeInLogo,
    /// <summary>The fade in Nintendo copyright program.</summary>
    FadeInNintendoCopyright,
    /// <summary>The title screen Baby Metroid tube light program, step 0.</summary>
    TitleScreenBabyMetroidTubeLight0,
    /// <summary>The title screen Baby Metroid tube light program, step 1.</summary>
    TitleScreenBabyMetroidTubeLight1,
    /// <summary>The title screen flickering displays program, step 0.</summary>
    TitleScreenFlickeringDisplays0,
    /// <summary>The title screen flickering displays program, step 1.</summary>
    TitleScreenFlickeringDisplays1,
    /// <summary>The cutscene gunship engine flicker program, step 0.</summary>
    CutsceneGunshipEngineFlicker0,
    /// <summary>The cutscene gunship engine flicker program, step 1.</summary>
    CutsceneGunshipEngineFlicker1,
    /// <summary>The cutscene Ceres navigation light sprite program.</summary>
    CutsceneCeresNavigationLightSprite,
    /// <summary>The shared Ceres navigation lights program its variants continue into.</summary>
    CommonCeresNavigationLights,
    /// <summary>The cutscene Ceres navigation lights background program.</summary>
    CutsceneCeresNavigationLightsBG,
    /// <summary>The fade in Planet Zebes text program.</summary>
    FadeInPlanetZebesText,
    /// <summary>The fade out Planet Zebes text program.</summary>
    FadeOutPlanetZebesText,
    /// <summary>The old Mother Brain fight background lights program, step 0.</summary>
    OldMotherBrainFightBGLights0,
    /// <summary>The old Mother Brain fight background lights program, step 1.</summary>
    OldMotherBrainFightBGLights1,
    /// <summary>The gunship glow program, step 0.</summary>
    GunshipGlow0,
    /// <summary>The gunship glow program, step 1.</summary>
    GunshipGlow1,
    /// <summary>The fade out zoomed out exploding Zebes program.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary>The wide part of Zebes explosion foreground program.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary>The Zebes explosion finale program.</summary>
    ZebesExplosionFinale,
    /// <summary>The wide part of Zebes explosion background program.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary>The white out Space during Zebes explosion program.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary>The shared Zebes explosion program its variants continue into.</summary>
    CommonZebesExplosion,
    /// <summary>The Zebes explosion Planet after glow program, step 0.</summary>
    ZebesExplosionPlanetAfterGlow0,
    /// <summary>The Zebes explosion Planet after glow program, step 1.</summary>
    ZebesExplosionPlanetAfterGlow1,
    /// <summary>The exploding Zebes lava program, step 0.</summary>
    ExplodingZebesLava0,
    /// <summary>The exploding Zebes lava program, step 1.</summary>
    ExplodingZebesLava1,
    /// <summary>The fade out exploding Zebes crust program.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary>The fade out exploding Zebes grey clouds program.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary>The gunship emerging from Zebes explosion program.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary>The Hyper Beam program, step 0.</summary>
    HyperBeam0,
    /// <summary>The Hyper Beam program, step 1.</summary>
    HyperBeam1,
    /// <summary>An unused program.</summary>
    ListD9D0,
    /// <summary>The Samus loading Power suit program, step 0.</summary>
    SamusLoadingPowerSuit0,
    /// <summary>The Samus loading Power suit program, step 1.</summary>
    SamusLoadingPowerSuit1,
    /// <summary>The Samus loading Power suit program, step 2.</summary>
    SamusLoadingPowerSuit2,
    /// <summary>The Samus loading Power suit program, step 3.</summary>
    SamusLoadingPowerSuit3,
    /// <summary>The Samus loading Power suit program, step 4.</summary>
    SamusLoadingPowerSuit4,
    /// <summary>The Samus loading Varia suit program, step 0.</summary>
    SamusLoadingVairaSuit0,
    /// <summary>The Samus loading Varia suit program, step 1.</summary>
    SamusLoadingVairaSuit1,
    /// <summary>The Samus loading Varia suit program, step 2.</summary>
    SamusLoadingVairaSuit2,
    /// <summary>The Samus loading Varia suit program, step 3.</summary>
    SamusLoadingVairaSuit3,
    /// <summary>The Samus loading Varia suit program, step 4.</summary>
    SamusLoadingVairaSuit4,
    /// <summary>The Samus loading Gravity suit program, step 0.</summary>
    SamusLoadingGravitySuit0,
    /// <summary>The Samus loading Gravity suit program, step 1.</summary>
    SamusLoadingGravitySuit1,
    /// <summary>The Samus loading Gravity suit program, step 2.</summary>
    SamusLoadingGravitySuit2,
    /// <summary>The Samus loading Gravity suit program, step 3.</summary>
    SamusLoadingGravitySuit3,
    /// <summary>The Samus loading Gravity suit program, step 4.</summary>
    SamusLoadingGravitySuit4,
    /// <summary>The post credits Super Metroid icon glare program.</summary>
    PostCreditsSuperMetroidIconGlare,
    /// <summary>The nothing program.</summary>
    Nothing,
    /// <summary>The grey out Tourian statue Draygon program.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary>The grey out Tourian statue Kraid program.</summary>
    GreyOutTourianStatueKraid,
    /// <summary>The grey out Tourian statue Ridley program.</summary>
    GreyOutTourianStatueRidley,
    /// <summary>The grey out Tourian statue Phantoon program.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary>The shared grey out Tourian statue program its variants continue into.</summary>
    CommonGreyOutTourianStatue,
    /// <summary>The Bomb Torizo belly program, step 0.</summary>
    BombTorizoBelly0,
    /// <summary>The Bomb Torizo belly program, step 1.</summary>
    BombTorizoBelly1,
    /// <summary>The Golden Torizo belly program, step 0.</summary>
    GoldenTorizoBelly0,
    /// <summary>The Golden Torizo belly program, step 1.</summary>
    GoldenTorizoBelly1,
    /// <summary>The Samus in heat Power suit program, step 0.</summary>
    SamusInHeatPowerSuit0,
    /// <summary>The Samus in heat Power suit program, step 1.</summary>
    SamusInHeatPowerSuit1,
    /// <summary>The Samus in heat Power suit program, step 2.</summary>
    SamusInHeatPowerSuit2,
    /// <summary>The Samus in heat Power suit program, step 3.</summary>
    SamusInHeatPowerSuit3,
    /// <summary>The Samus in heat Power suit program, step 4.</summary>
    SamusInHeatPowerSuit4,
    /// <summary>The Samus in heat Power suit program, step 5.</summary>
    SamusInHeatPowerSuit5,
    /// <summary>The Samus in heat Power suit program, step 6.</summary>
    SamusInHeatPowerSuit6,
    /// <summary>The Samus in heat Power suit program, step 7.</summary>
    SamusInHeatPowerSuit7,
    /// <summary>The Samus in heat Power suit program, step 8.</summary>
    SamusInHeatPowerSuit8,
    /// <summary>The Samus in heat Power suit program, step 9.</summary>
    SamusInHeatPowerSuit9,
    /// <summary>The Samus in heat Power suit program, step A.</summary>
    SamusInHeatPowerSuitA,
    /// <summary>The Samus in heat Power suit program, step B.</summary>
    SamusInHeatPowerSuitB,
    /// <summary>The Samus in heat Power suit program, step C.</summary>
    SamusInHeatPowerSuitC,
    /// <summary>The Samus in heat Power suit program, step D.</summary>
    SamusInHeatPowerSuitD,
    /// <summary>The Samus in heat Power suit program, step E.</summary>
    SamusInHeatPowerSuitE,
    /// <summary>The Samus in heat Power suit program, step F.</summary>
    SamusInHeatPowerSuitF,
    /// <summary>The Samus in heat Power suit program, step 10.</summary>
    SamusInHeatPowerSuit10,
    /// <summary>The Samus in heat Varia suit program, step 0.</summary>
    SamusInHeatVariaSuit0,
    /// <summary>The Samus in heat Varia suit program, step 2.</summary>
    SamusInHeatVariaSuit2,
    /// <summary>The Samus in heat Varia suit program, step 3.</summary>
    SamusInHeatVariaSuit3,
    /// <summary>The Samus in heat Varia suit program, step 4.</summary>
    SamusInHeatVariaSuit4,
    /// <summary>The Samus in heat Varia suit program, step 5.</summary>
    SamusInHeatVariaSuit5,
    /// <summary>The Samus in heat Varia suit program, step 6.</summary>
    SamusInHeatVariaSuit6,
    /// <summary>The Samus in heat Varia suit program, step 7.</summary>
    SamusInHeatVariaSuit7,
    /// <summary>The Samus in heat Varia suit program, step 8.</summary>
    SamusInHeatVariaSuit8,
    /// <summary>The Samus in heat Varia suit program, step 9.</summary>
    SamusInHeatVariaSuit9,
    /// <summary>The Samus in heat Varia suit program, step A.</summary>
    SamusInHeatVariaSuitA,
    /// <summary>The Samus in heat Varia suit program, step B.</summary>
    SamusInHeatVariaSuitB,
    /// <summary>The Samus in heat Varia suit program, step C.</summary>
    SamusInHeatVariaSuitC,
    /// <summary>The Samus in heat Varia suit program, step D.</summary>
    SamusInHeatVariaSuitD,
    /// <summary>The Samus in heat Varia suit program, step E.</summary>
    SamusInHeatVariaSuitE,
    /// <summary>The Samus in heat Varia suit program, step F.</summary>
    SamusInHeatVariaSuitF,
    /// <summary>The Samus in heat Varia suit program, step 10.</summary>
    SamusInHeatVariaSuit10,
    /// <summary>The Samus in heat Varia suit program, step 11.</summary>
    SamusInHeatVariaSuit11,
    /// <summary>The Samus in heat Gravity suit program, step 0.</summary>
    SamusInHeatGravitySuit0,
    /// <summary>The Samus in heat Gravity suit program, step 2.</summary>
    SamusInHeatGravitySuit2,
    /// <summary>The Samus in heat Gravity suit program, step 3.</summary>
    SamusInHeatGravitySuit3,
    /// <summary>The Samus in heat Gravity suit program, step 4.</summary>
    SamusInHeatGravitySuit4,
    /// <summary>The Samus in heat Gravity suit program, step 5.</summary>
    SamusInHeatGravitySuit5,
    /// <summary>The Samus in heat Gravity suit program, step 6.</summary>
    SamusInHeatGravitySuit6,
    /// <summary>The Samus in heat Gravity suit program, step 7.</summary>
    SamusInHeatGravitySuit7,
    /// <summary>The Samus in heat Gravity suit program, step 8.</summary>
    SamusInHeatGravitySuit8,
    /// <summary>The Samus in heat Gravity suit program, step 9.</summary>
    SamusInHeatGravitySuit9,
    /// <summary>The Samus in heat Gravity suit program, step A.</summary>
    SamusInHeatGravitySuitA,
    /// <summary>The Samus in heat Gravity suit program, step B.</summary>
    SamusInHeatGravitySuitB,
    /// <summary>The Samus in heat Gravity suit program, step C.</summary>
    SamusInHeatGravitySuitC,
    /// <summary>The Samus in heat Gravity suit program, step D.</summary>
    SamusInHeatGravitySuitD,
    /// <summary>The Samus in heat Gravity suit program, step E.</summary>
    SamusInHeatGravitySuitE,
    /// <summary>The Samus in heat Gravity suit program, step F.</summary>
    SamusInHeatGravitySuitF,
    /// <summary>The Samus in heat Gravity suit program, step 10.</summary>
    SamusInHeatGravitySuit10,
    /// <summary>The Samus in heat Gravity suit program, step 11.</summary>
    SamusInHeatGravitySuit11,
    /// <summary>The Wrecked Ship palette effect 1 program, step 0.</summary>
    WreckedShip10,
    /// <summary>The Wrecked Ship palette effect 1 program, step 1.</summary>
    WreckedShip11,
    /// <summary>The Crateria palette effect 1 program, step 0.</summary>
    Crateria10,
    /// <summary>The Crateria palette effect 1 program, step 1.</summary>
    Crateria11,
    /// <summary>The Crateria palette effect 1 program, step 2.</summary>
    Crateria12,
    /// <summary>The Crateria palette effect 1 program, step 3.</summary>
    Crateria13,
    /// <summary>An unused dark lightning program, step 0.</summary>
    ListEC6E,
    /// <summary>An unused dark lightning program, step 1.</summary>
    CrateriaAboveY380Second,
    /// <summary>An unused dark lightning program, step 2.</summary>
    ListEC8B,
    /// <summary>An unused dark lightning program, step 3.</summary>
    ListED34,
    /// <summary>The Brinstar palette effect 1 program, step 0.</summary>
    Brinstar10,
    /// <summary>The Brinstar palette effect 1 program, step 1.</summary>
    Brinstar11,
    /// <summary>The Brinstar palette effect 8 program, step 0.</summary>
    Brinstar80,
    /// <summary>The Brinstar palette effect 8 program, step 1.</summary>
    Brinstar81,
    /// <summary>The Brinstar palette effect 2 program, step 0.</summary>
    Brinstar20,
    /// <summary>The Brinstar palette effect 2 program, step 1.</summary>
    Brinstar21,
    /// <summary>The Crateria palette effect 80 and Brinstar palette effect 4 program, step 0.</summary>
    Crateria80Brinstar40,
    /// <summary>The Crateria palette effect 80 and Brinstar palette effect 4 program, step 1.</summary>
    Crateria80Brinstar41,
    /// <summary>The Norfair palette effect 2 program, step 0.</summary>
    Norfair20,
    /// <summary>The Norfair palette effect 2 program, step 1.</summary>
    Norfair21,
    /// <summary>The Norfair palette effect 4 program, step 0.</summary>
    Norfair40,
    /// <summary>The Norfair palette effect 4 program, step 1.</summary>
    Norfair41,
    /// <summary>The Norfair palette effect 8 program, step 0.</summary>
    Norfair80,
    /// <summary>The Norfair palette effect 8 program, step 1.</summary>
    Norfair81,
    /// <summary>The Norfair palette effect 10 program, step 0.</summary>
    Norfair100,
    /// <summary>The Norfair palette effect 10 program, step 1.</summary>
    Norfair101,
    /// <summary>The Maridia palette effect 1 program, step 0.</summary>
    Maridia10,
    /// <summary>The Maridia palette effect 1 program, step 1.</summary>
    Maridia11,
    /// <summary>The Maridia palette effect 2 program, step 0.</summary>
    Maridia20,
    /// <summary>The Maridia palette effect 2 program, step 1.</summary>
    Maridia21,
    /// <summary>The Maridia palette effect 4 program, step 0.</summary>
    Maridia40,
    /// <summary>The Maridia palette effect 4 program, step 1.</summary>
    Maridia41,
    /// <summary>The Tourian palette effect 4 program.</summary>
    Tourian4,
    /// <summary>The Tourian palette effect 2 program.</summary>
    Tourian2,
    /// <summary>The shared Tourian palette effect 2 and Tourian palette effect 4 program its variants continue into.</summary>
    CommonTourian2Tourian41,
    /// <summary>The Tourian palette effect 8 program, step 0.</summary>
    Tourian80,
    /// <summary>The Tourian palette effect 8 program, step 1.</summary>
    Tourian81,
    /// <summary>The Tourian palette effect 10 program, step 0.</summary>
    Tourian100,
    /// <summary>The Tourian palette effect 10 program, step 1.</summary>
    Tourian101,
    /// <summary>The Tourian palette effect 20 program.</summary>
    Tourian20,
    /// <summary>The Tourian palette effect 40 program.</summary>
    Tourian40,
    /// <summary>The shared Tourian palette effect 20 and Tourian palette effect 40 program its variants continue into.</summary>
    CommonTourian20Tourian40,
    /// <summary>The Crateria palette effect 8 program, step 0.</summary>
    Crateria80,
    /// <summary>The Crateria palette effect 8 program, step 1.</summary>
    Crateria81,
    /// <summary>The Crateria palette effect 10 program, step 0.</summary>
    Crateria100,
    /// <summary>The Crateria palette effect 10 program, step 1.</summary>
    Crateria101,
    /// <summary>The Crateria palette effect 20 program, step 0.</summary>
    Crateria200,
    /// <summary>The Crateria palette effect 20 program, step 1.</summary>
    Crateria201,
    /// <summary>The Crateria palette effect 2 program, step 0.</summary>
    Crateria20,
    /// <summary>The Crateria palette effect 2 program, step 1.</summary>
    Crateria21,
    /// <summary>The Crateria palette effect 4 program, step 0.</summary>
    Crateria40,
    /// <summary>The Crateria palette effect 4 program, step 1.</summary>
    Crateria41,
    /// <summary>The Crateria palette effect 40 program, step 0.</summary>
    Crateria400,
    /// <summary>The Crateria palette effect 40 program, step 1.</summary>
    Crateria401,
}
