namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour effects' animations (#1298), one per entry point an effect, a start, a condition or a branch
/// enters, in authored order. Generated from the verified animations; see <see cref="PaletteFxAnimations"/>.
/// </summary>
public enum PaletteFxSegment
{
    /// <summary>The fade in Super Metroid title logo animation.</summary>
    FadeInSuperMetroidTitleLogo,
    /// <summary>The fade in logo animation the copyright's fade continues into.</summary>
    CommonFadeInLogo,
    /// <summary>The fade in Nintendo copyright animation.</summary>
    FadeInNintendoCopyright,
    /// <summary>The title screen Baby Metroid tube light animation, step 0.</summary>
    TitleScreenBabyMetroidTubeLight0,
    /// <summary>The title screen Baby Metroid tube light animation, step 1.</summary>
    TitleScreenBabyMetroidTubeLight1,
    /// <summary>The title screen flickering displays animation, step 0.</summary>
    TitleScreenFlickeringDisplays0,
    /// <summary>The title screen flickering displays animation, step 1.</summary>
    TitleScreenFlickeringDisplays1,
    /// <summary>The cutscene gunship engine flicker animation, step 0.</summary>
    CutsceneGunshipEngineFlicker0,
    /// <summary>The cutscene gunship engine flicker animation, step 1.</summary>
    CutsceneGunshipEngineFlicker1,
    /// <summary>The cutscene Ceres navigation light sprite animation.</summary>
    CutsceneCeresNavigationLightSprite,
    /// <summary>The shared Ceres navigation lights animation its variants continue into.</summary>
    CommonCeresNavigationLights,
    /// <summary>The cutscene Ceres navigation lights background animation.</summary>
    CutsceneCeresNavigationLightsBG,
    /// <summary>The fade in Planet Zebes text animation.</summary>
    FadeInPlanetZebesText,
    /// <summary>The fade out Planet Zebes text animation.</summary>
    FadeOutPlanetZebesText,
    /// <summary>The old Mother Brain fight background lights animation, step 0.</summary>
    OldMotherBrainFightBGLights0,
    /// <summary>The old Mother Brain fight background lights animation, step 1.</summary>
    OldMotherBrainFightBGLights1,
    /// <summary>The gunship glow animation, step 0.</summary>
    GunshipGlow0,
    /// <summary>The gunship glow animation, step 1.</summary>
    GunshipGlow1,
    /// <summary>The fade out zoomed out exploding Zebes animation.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary>The wide part of Zebes explosion foreground animation.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary>The Zebes explosion finale animation.</summary>
    ZebesExplosionFinale,
    /// <summary>The wide part of Zebes explosion background animation.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary>The white out Space during Zebes explosion animation.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary>The shared Zebes explosion animation its variants continue into.</summary>
    CommonZebesExplosion,
    /// <summary>The Zebes explosion Planet after glow animation, step 0.</summary>
    ZebesExplosionPlanetAfterGlow0,
    /// <summary>The Zebes explosion Planet after glow animation, step 1.</summary>
    ZebesExplosionPlanetAfterGlow1,
    /// <summary>The exploding Zebes lava animation, step 0.</summary>
    ExplodingZebesLava0,
    /// <summary>The exploding Zebes lava animation, step 1.</summary>
    ExplodingZebesLava1,
    /// <summary>The fade out exploding Zebes crust animation.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary>The fade out exploding Zebes grey clouds animation.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary>The gunship emerging from Zebes explosion animation.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary>The Hyper Beam animation, step 0.</summary>
    HyperBeam0,
    /// <summary>The Hyper Beam animation, step 1.</summary>
    HyperBeam1,
    /// <summary>The Samus loading Power suit animation, step 0.</summary>
    SamusLoadingPowerSuit0,
    /// <summary>The Samus loading Power suit animation, step 1.</summary>
    SamusLoadingPowerSuit1,
    /// <summary>The Samus loading Power suit animation, step 2.</summary>
    SamusLoadingPowerSuit2,
    /// <summary>The Samus loading Power suit animation, step 3.</summary>
    SamusLoadingPowerSuit3,
    /// <summary>The Samus loading Power suit animation, step 4.</summary>
    SamusLoadingPowerSuit4,
    /// <summary>The Samus loading Varia suit animation, step 0.</summary>
    SamusLoadingVairaSuit0,
    /// <summary>The Samus loading Varia suit animation, step 1.</summary>
    SamusLoadingVairaSuit1,
    /// <summary>The Samus loading Varia suit animation, step 2.</summary>
    SamusLoadingVairaSuit2,
    /// <summary>The Samus loading Varia suit animation, step 3.</summary>
    SamusLoadingVairaSuit3,
    /// <summary>The Samus loading Varia suit animation, step 4.</summary>
    SamusLoadingVairaSuit4,
    /// <summary>The Samus loading Gravity suit animation, step 0.</summary>
    SamusLoadingGravitySuit0,
    /// <summary>The Samus loading Gravity suit animation, step 1.</summary>
    SamusLoadingGravitySuit1,
    /// <summary>The Samus loading Gravity suit animation, step 2.</summary>
    SamusLoadingGravitySuit2,
    /// <summary>The Samus loading Gravity suit animation, step 3.</summary>
    SamusLoadingGravitySuit3,
    /// <summary>The Samus loading Gravity suit animation, step 4.</summary>
    SamusLoadingGravitySuit4,
    /// <summary>The post credits Super Metroid icon glare animation.</summary>
    PostCreditsSuperMetroidIconGlare,
    /// <summary>The nothing animation.</summary>
    Nothing,
    /// <summary>The grey out Tourian statue Draygon animation.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary>The grey out Tourian statue Kraid animation.</summary>
    GreyOutTourianStatueKraid,
    /// <summary>The grey out Tourian statue Ridley animation.</summary>
    GreyOutTourianStatueRidley,
    /// <summary>The grey out Tourian statue Phantoon animation.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary>The shared grey out Tourian statue animation its variants continue into.</summary>
    CommonGreyOutTourianStatue,
    /// <summary>The Bomb Torizo belly animation, step 0.</summary>
    BombTorizoBelly0,
    /// <summary>The Bomb Torizo belly animation, step 1.</summary>
    BombTorizoBelly1,
    /// <summary>The Golden Torizo belly animation, step 0.</summary>
    GoldenTorizoBelly0,
    /// <summary>The Golden Torizo belly animation, step 1.</summary>
    GoldenTorizoBelly1,
    /// <summary>Samus's heat shimmer in the Power Suit: watches the heat, names her suit's colours, then runs its pulse.</summary>
    SamusInHeatPowerSuit,
    /// <summary>
    /// The Power Suit's heat pulse: one frame for each <see cref="HeatPhase"/>, in phase order, then round again. The
    /// shimmer restarts at the frame of the phase the heated room's glow publishes.
    /// </summary>
    SamusInHeatPowerSuitPulse,
    /// <summary>Samus's heat shimmer in the Varia Suit: watches the heat, names her suit's colours, then runs its pulse.</summary>
    SamusInHeatVariaSuit,
    /// <summary>
    /// The Varia Suit's heat pulse: one frame for each <see cref="HeatPhase"/>, in phase order, then round again. The
    /// shimmer restarts at the frame of the phase the heated room's glow publishes.
    /// </summary>
    SamusInHeatVariaSuitPulse,
    /// <summary>Samus's heat shimmer in the Gravity Suit: watches the heat, names her suit's colours, then runs its pulse.</summary>
    SamusInHeatGravitySuit,
    /// <summary>
    /// The Gravity Suit's heat pulse: one frame for each <see cref="HeatPhase"/>, in phase order, then round again. The
    /// shimmer restarts at the frame of the phase the heated room's glow publishes.
    /// </summary>
    SamusInHeatGravitySuitPulse,
    /// <summary>The Wrecked Ship's green lights: names their colours, then runs their cycle.</summary>
    WreckedShipGreenLights,
    /// <summary>The Wrecked Ship's green lights' repeating pulse.</summary>
    WreckedShipGreenLightsCycle,
    /// <summary>The Crateria surface lightning: watches Samus's height, names the sky's colours, then runs its cycle.</summary>
    CrateriaLightning,
    /// <summary>One cycle of the Crateria surface lightning, restarted calm while Samus is high on the surface.</summary>
    CrateriaLightningCycle,
    /// <summary>The Crateria surface lightning's flash shown twice each cycle.</summary>
    CrateriaLightningRepeatedFlash,
    /// <summary>The Crateria surface lightning's flash shown once before the cycle repeats.</summary>
    CrateriaLightningClosingFlash,
    /// <summary>Brinstar's blue background spores: names their colours, then runs their cycle.</summary>
    BrinstarBlueBackgroundSpores,
    /// <summary>Brinstar's blue background spores' repeating drift.</summary>
    BrinstarBlueBackgroundSporesCycle,
    /// <summary>Spore Spawn's blue background spores: end with Spore Spawn, name their colours, then run their cycle.</summary>
    SporeSpawnBlueBackgroundSpores,
    /// <summary>Spore Spawn's blue background spores' repeating drift.</summary>
    SporeSpawnBlueBackgroundSporesCycle,
    /// <summary>Red Brinstar's background glow: names its colours, then runs its cycle.</summary>
    BrinstarRedBackgroundGlow,
    /// <summary>Red Brinstar's background glow's repeating pulse.</summary>
    BrinstarRedBackgroundGlowCycle,
    /// <summary>The flashing beacons: names their colours, then runs their cycle.</summary>
    BeaconFlashing,
    /// <summary>The beacons' repeating flash, which sounds the beacon each time it starts.</summary>
    BeaconFlashingCycle,
    /// <summary>Norfair's heated foreground glow that paces Samus's heat shimmer: names its colours, then runs its cycle.</summary>
    NorfairForegroundAndHeatPhase,
    /// <summary>Norfair's heated foreground glow's sixteen phases, each published as the heat phase Samus's suit glows by.</summary>
    NorfairForegroundAndHeatPhaseCycle,
    /// <summary>The heat glow on Norfair's green and red foreground: names its colours, then runs its cycle.</summary>
    NorfairGreenForegroundHeatGlow,
    /// <summary>The heat glow on Norfair's green and red foreground, in step with the heat phase.</summary>
    NorfairGreenForegroundHeatGlowCycle,
    /// <summary>The heat glow on Norfair's brown and purple foreground: names its colours, then runs its cycle.</summary>
    NorfairPurpleForegroundHeatGlow,
    /// <summary>The heat glow on Norfair's brown and purple foreground, in step with the heat phase.</summary>
    NorfairPurpleForegroundHeatGlowCycle,
    /// <summary>The heat glow on Norfair's dark brown foreground: names its colours, then runs its cycle.</summary>
    NorfairDarkForegroundHeatGlow,
    /// <summary>The heat glow on Norfair's dark brown foreground, in step with the heat phase.</summary>
    NorfairDarkForegroundHeatGlowCycle,
    /// <summary>Maridia's sand pits: names their colours, then runs their cycle.</summary>
    MaridiaSandPits,
    /// <summary>Maridia's sand pits' repeating shift.</summary>
    MaridiaSandPitsCycle,
    /// <summary>Maridia's falling sand: names its colours, then runs its cycle.</summary>
    MaridiaSandFalls,
    /// <summary>Maridia's falling sand's repeating flow.</summary>
    MaridiaSandFallsCycle,
    /// <summary>Maridia's background waterfalls: names their colours, then runs their cycle.</summary>
    MaridiaBackgroundWaterfalls,
    /// <summary>Maridia's background waterfalls' repeating flow.</summary>
    MaridiaBackgroundWaterfallsCycle,
    /// <summary>Tourian's glowing Arkanoid blocks and red orbs: names their colours, then runs the shared glow cycle.</summary>
    TourianGlowingArkanoidBlocksAndRedOrbs,
    /// <summary>Tourian's glow cycle, which the glow runs after naming its colours.</summary>
    CommonTourianGlowCycle,
    /// <summary>Tourian's shutters flashing red during the escape: names their colours, then runs their cycle.</summary>
    TourianShutterRedFlashing,
    /// <summary>Tourian's shutters' repeating red flash.</summary>
    TourianShutterRedFlashingCycle,
    /// <summary>Tourian's background flashing red during the escape: names its colours, then runs its cycle.</summary>
    TourianBackgroundRedFlashing,
    /// <summary>Tourian's background's repeating red flash.</summary>
    TourianBackgroundRedFlashingCycle,
    /// <summary>Tourian's rooms flashing red during the escape: names their colours, then jumps into the shared flash cycle.</summary>
    TourianGeneralLevelRedFlashing,
    /// <summary>Tourian's Arkanoid blocks and red orbs flashing red during the escape: names their colours, then runs the shared flash cycle.</summary>
    TourianRedFlashingArkanoidBlocksAndRedOrbs,
    /// <summary>Tourian's escape red flash cycle, shared by the rooms and the Arkanoid blocks and orbs.</summary>
    CommonTourianEscapeRedFlashingCycle,
    /// <summary>Old Tourian's rooms flashing red during the escape: names their colours, then runs their cycle.</summary>
    OldTourianRedFlashing,
    /// <summary>Old Tourian's rooms' repeating red flash.</summary>
    OldTourianRedFlashingCycle,
    /// <summary>Old Tourian's background railings flashing yellow during the escape: names their colours, then runs their cycle.</summary>
    OldTourianBackgroundRailingsFlashYellow,
    /// <summary>Old Tourian's background railings' repeating yellow flash.</summary>
    OldTourianBackgroundRailingsFlashYellowCycle,
    /// <summary>Old Tourian's background panels flashing yellow during the escape: names their colours, then runs their cycle.</summary>
    OldTourianBackgroundPanelsFlashYellow,
    /// <summary>Old Tourian's background panels' repeating yellow flash.</summary>
    OldTourianBackgroundPanelsFlashYellowCycle,
    /// <summary>Upper Crateria flashing red during the escape: names its colours, then runs its cycle.</summary>
    UpperCrateriaRedFlashing,
    /// <summary>Upper Crateria's repeating red flash.</summary>
    UpperCrateriaRedFlashingCycle,
    /// <summary>The yellow lightning over Crateria's sky during the escape: names its colours, then runs its cycle.</summary>
    CrateriaEscapeYellowLightning,
    /// <summary>The escape's yellow lightning's repeating flashes.</summary>
    CrateriaEscapeYellowLightningCycle,
    /// <summary>The Parlor's dark sky flickering with the escape's lightning: names its colours, then runs its cycle.</summary>
    ParlorEscapeSkyFlicker,
    /// <summary>The Parlor's dark sky's repeating flicker, in the escape lightning's cadence.</summary>
    ParlorEscapeSkyFlickerCycle,
}
