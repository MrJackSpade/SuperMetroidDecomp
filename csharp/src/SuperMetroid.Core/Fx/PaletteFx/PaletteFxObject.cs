namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour effects, named by what each shows. Each names how it starts
/// and an initial animation (<see cref="RoomPaletteFxSelection"/>); a running <see cref="RoomColourEffect"/> shows one.
/// </summary>
public enum PaletteFxObject
{
    /// <summary>The Super Metroid logo fading in on the title screen.</summary>
    FadeInSuperMetroidTitleLogo = 1,
    /// <summary>The Nintendo copyright line fading in on the title screen.</summary>
    FadeInNintendoCopyright,
    /// <summary>The light of the baby Metroid's tube on the title screen.</summary>
    TitleScreenBabyMetroidTubeLight,
    /// <summary>The flickering monitors on the title screen.</summary>
    TitleScreenFlickeringDisplays,
    /// <summary>The gunship's flickering engine in the intro cutscenes.</summary>
    CutsceneGunshipEngineFlicker,
    /// <summary>Ceres station's blinking navigation lights on the cutscene's sprites.</summary>
    CutsceneCeresNavigationLightsSprite,
    /// <summary>The "Planet Zebes" caption fading in.</summary>
    FadeInPlanetZebesText,
    /// <summary>The "Planet Zebes" caption fading out.</summary>
    FadeOutPlanetZebesText,
    /// <summary>Ceres station's blinking navigation lights on the cutscene's background.</summary>
    CutsceneCeresNavigationLightsBackground,
    /// <summary>The background lights of the Mother Brain fight scene.</summary>
    OldMotherBrainFightBackgroundLights,
    /// <summary>The gunship's glowing lights.</summary>
    GunshipGlow,
    /// <summary>Exploding Zebes fading out in the ending's distant shot.</summary>
    FadeOutZoomedOutExplodingZebes,
    /// <summary>The foreground of the wide shot of the planet's explosion.</summary>
    WidePartOfZebesExplosionForeground,
    /// <summary>The final burst of the planet's explosion.</summary>
    ZebesExplosionFinale,
    /// <summary>Space whiting out as the planet explodes.</summary>
    WhiteOutSpaceDuringZebesExplosion,
    /// <summary>The planet's afterglow once it has exploded.</summary>
    ZebesExplosionPlanetAfterglow,
    /// <summary>The lava glowing through the exploding planet.</summary>
    ExplodingZebesLava,
    /// <summary>The exploding planet's crust fading out.</summary>
    FadeOutExplodingZebesCrust,
    /// <summary>The grey clouds around the exploding planet fading out.</summary>
    FadeOutExplodingZebesGreyClouds,
    /// <summary>The gunship emerging from the planet's explosion.</summary>
    GunshipEmergingFromZebesExplosion,
    /// <summary>The background of the wide shot of the planet's explosion.</summary>
    WidePartOfZebesExplosionBackground,
    /// <summary>The Hyper Beam's cycling colours.</summary>
    HyperBeam,
    /// <summary>Samus materialising in the Power Suit as a game loads.</summary>
    SamusLoadingPowerSuit,
    /// <summary>Samus materialising in the Varia Suit as a game loads.</summary>
    SamusLoadingVariaSuit,
    /// <summary>Samus materialising in the Gravity Suit as a game loads.</summary>
    SamusLoadingGravitySuit,
    /// <summary>The glare on the Super Metroid icon after the credits.</summary>
    PostCreditsSuperMetroidIcon,
    /// <summary>An object that shows nothing and deletes itself.</summary>
    Nothing,
    /// <summary>Draygon's statue in the Tourian entrance greying out.</summary>
    GreyOutTourianStatueDraygon,
    /// <summary>Kraid's statue in the Tourian entrance greying out.</summary>
    GreyOutTourianStatueKraid,
    /// <summary>Ridley's statue in the Tourian entrance greying out.</summary>
    GreyOutTourianStatueRidley,
    /// <summary>Phantoon's statue in the Tourian entrance greying out.</summary>
    GreyOutTourianStatuePhantoon,
    /// <summary>The Bomb Torizo's glowing belly.</summary>
    BombTorizoBelly,
    /// <summary>The Golden Torizo's glowing belly.</summary>
    GoldenTorizoBelly,
    /// <summary>Samus's heat shimmer in a heated room, which also burns her without a heat-protecting suit.</summary>
    SamusInHeat,
    /// <summary>The lightning flashing over the Crateria surface.</summary>
    CrateriaLightning,
    /// <summary>The Wrecked Ship's pulsing green lights.</summary>
    WreckedShipGreenLights,
    /// <summary>The blue spores drifting in Brinstar's background.</summary>
    BrinstarBlueBackgroundSpores,
    /// <summary>The blue background spores of Spore Spawn's room.</summary>
    SporeSpawnBlueBackgroundSpores,
    /// <summary>Red Brinstar's glowing background.</summary>
    BrinstarRedBackgroundGlow,
    /// <summary>The flashing beacon lights.</summary>
    BeaconFlashing,
    /// <summary>Norfair's glowing foreground, which also paces the heat shimmer.</summary>
    NorfairForegroundAndHeatPhase,
    /// <summary>
    /// The heat glow on the green and red parts of a heated Norfair room's foreground, which pulse to orange in step with
    /// <see cref="NorfairForegroundAndHeatPhase"/>.
    /// </summary>
    NorfairGreenForegroundHeatGlow,
    /// <summary>
    /// The heat glow on the brown and purple parts of a heated Norfair room's foreground, which pulse to red in step with
    /// <see cref="NorfairForegroundAndHeatPhase"/>.
    /// </summary>
    NorfairPurpleForegroundHeatGlow,
    /// <summary>
    /// The heat glow on the dark brown parts of a heated Norfair room's foreground, which pulse to red in step with
    /// <see cref="NorfairForegroundAndHeatPhase"/>.
    /// </summary>
    NorfairDarkForegroundHeatGlow,
    /// <summary>Maridia's shifting sand pits.</summary>
    MaridiaSandPits,
    /// <summary>Maridia's falling sand.</summary>
    MaridiaSandFalls,
    /// <summary>The waterfalls in Maridia's background.</summary>
    MaridiaBackgroundWaterfalls,
    /// <summary>Tourian's glowing blocks and red orbs.</summary>
    TourianGlowingArkanoidBlocksAndRedOrbs,
    /// <summary>Tourian's shutters flashing red.</summary>
    TourianShutterRedFlashing,
    /// <summary>Tourian's background flashing red during the escape.</summary>
    TourianBackgroundRedFlashing,
    /// <summary>Tourian's rooms flashing red during the escape.</summary>
    TourianGeneralLevelRedFlashing,
    /// <summary>Tourian's blocks and red orbs flashing red during the escape.</summary>
    TourianRedFlashingArkanoidBlocksAndRedOrbs,
    /// <summary>Old Tourian's rooms flashing red during the escape.</summary>
    OldTourianRedFlashing,
    /// <summary>Old Tourian's background railings flashing yellow during the escape.</summary>
    OldTourianBackgroundRailingsFlashYellow,
    /// <summary>Old Tourian's background panels flashing yellow during the escape.</summary>
    OldTourianBackgroundPanelsFlashYellow,
    /// <summary>Upper Crateria flashing red during the escape.</summary>
    UpperCrateriaRedFlashing,
    /// <summary>The yellow lightning over Crateria during the escape.</summary>
    CrateriaEscapeYellowLightning,
    /// <summary>The Parlor's dark sky flickering in the cadence of the escape's yellow lightning.</summary>
    ParlorEscapeSkyFlicker,
}
