namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colours a palette-FX object animates (#1464): what a <see cref="PaletteFxCue.SetColours"/> cue makes its
/// object's later records colour. Which of an owner's colours each names is the single mapping in
/// <see cref="PaletteFxColourPlacementDefinitions"/>.
/// </summary>
public enum PaletteFxColours
{
    /// <summary>The Super Metroid title logo, faded in on the title screen.</summary>
    TitleLogo,
    /// <summary>The Nintendo boot logo, faded in by the unused boot code.</summary>
    NintendoBootLogo,
    /// <summary>The Nintendo copyright line, faded in under the title.</summary>
    NintendoCopyright,
    /// <summary>The light in the title screen's Baby Metroid tube.</summary>
    BabyMetroidTubeLight,
    /// <summary>The title screen's flickering lab displays.</summary>
    TitleScreenDisplays,
    /// <summary>The gunship's engine in the intro cutscene.</summary>
    CutsceneGunshipEngine,
    /// <summary>Ceres station's navigation lights in the sprite-drawn cutscene.</summary>
    CeresNavigationLightsSprite,
    /// <summary>Ceres station's navigation lights in the background-drawn cutscene.</summary>
    CeresNavigationLightsBackground,
    /// <summary>The "Planet Zebes" caption, faded in and out.</summary>
    PlanetZebesText,
    /// <summary>The background lights of the old Mother Brain fight in the intro.</summary>
    OldMotherBrainBackgroundLights,
    /// <summary>The glow on the gunship in the ending.</summary>
    GunshipGlow,
    /// <summary>The zoomed-out exploding Zebes, faded out.</summary>
    ZoomedOutExplodingZebes,
    /// <summary>The foreground of the wide part of Zebes's explosion.</summary>
    WideZebesExplosionForeground,
    /// <summary>The background of the wide part of Zebes's explosion.</summary>
    WideZebesExplosionBackground,
    /// <summary>The finale of Zebes's explosion.</summary>
    ZebesExplosionFinale,
    /// <summary>Space, whited out during Zebes's explosion.</summary>
    SpaceDuringZebesExplosion,
    /// <summary>The planet's afterglow after Zebes explodes.</summary>
    ZebesPlanetAfterglow,
    /// <summary>The lava of the exploding Zebes.</summary>
    ExplodingZebesLava,
    /// <summary>The crust of the exploding Zebes, faded out.</summary>
    ExplodingZebesCrust,
    /// <summary>The grey clouds of the exploding Zebes, faded out.</summary>
    ExplodingZebesGreyClouds,
    /// <summary>The gunship emerging from Zebes's explosion.</summary>
    GunshipEmergingFromExplosion,
    /// <summary>The colours the unused cinematic fade program fades.</summary>
    UnusedCinematicFade,
    /// <summary>Samus's beam colours while she holds the Hyper Beam.</summary>
    HyperBeam,
    /// <summary>Samus's suit while she materializes at a load.</summary>
    SamusLoadingSuit,
    /// <summary>The Super Metroid icon after the credits.</summary>
    PostCreditsSuperMetroidIcon,
    /// <summary>Draygon's Tourian statue, greyed out once Draygon is dead.</summary>
    DraygonStatue,
    /// <summary>Kraid's Tourian statue, greyed out once Kraid is dead.</summary>
    KraidStatue,
    /// <summary>Ridley's Tourian statue, greyed out once Ridley is dead.</summary>
    RidleyStatue,
    /// <summary>Phantoon's Tourian statue, greyed out once Phantoon is dead.</summary>
    PhantoonStatue,
    /// <summary>The glowing belly of the Bomb Torizo and the Golden Torizo.</summary>
    TorizoBelly,
    /// <summary>Samus's suit glowing in a heated room.</summary>
    HeatSuitGlow,
    /// <summary>Wrecked Ship's green lights, once its power is on.</summary>
    WreckedShipGreenLights,
    /// <summary>The Landing Site's sky, flashed by its lightning.</summary>
    LandingSiteLightning,
    /// <summary>The colours the unused dark-lightning program darkens.</summary>
    UnusedDarkLightning,
    /// <summary>Brinstar's blue background spores, in the ordinary rooms and Spore Spawn's.</summary>
    BrinstarBlueSpores,
    /// <summary>Brinstar's red background glow.</summary>
    BrinstarRedGlow,
    /// <summary>The flashing beacons of Crateria and Brinstar.</summary>
    Beacon,
    /// <summary>Norfair's foreground that publishes the heat phase Samus's suit glows by.</summary>
    NorfairHeatForeground,
    /// <summary>The Norfair foreground its <see cref="PaletteFxObject.NorfairForegroundPalette4"/> object animates.</summary>
    NorfairForegroundPalette4,
    /// <summary>The Norfair foreground its <see cref="PaletteFxObject.NorfairForegroundPalette5"/> object animates.</summary>
    NorfairForegroundPalette5,
    /// <summary>The Norfair foreground its <see cref="PaletteFxObject.NorfairForegroundPalette6"/> object animates.</summary>
    NorfairForegroundPalette6,
    /// <summary>Maridia's sand pits.</summary>
    MaridiaSandPits,
    /// <summary>Maridia's sand falls.</summary>
    MaridiaSandFalls,
    /// <summary>Maridia's background waterfalls.</summary>
    MaridiaBackgroundWaterfalls,
    /// <summary>Tourian's Arkanoid blocks and red orbs, which glow and, during the escape, flash red.</summary>
    TourianArkanoidBlocksAndOrbs,
    /// <summary>Tourian's shutters, flashing red during the escape.</summary>
    TourianShutters,
    /// <summary>Tourian's background, flashing red during the escape.</summary>
    TourianBackground,
    /// <summary>Tourian's level colours, flashing red during the escape.</summary>
    TourianLevel,
    /// <summary>Old Tourian's level colours, flashing red during the escape.</summary>
    OldTourianLevel,
    /// <summary>Old Tourian's background railings, flashing yellow during the escape.</summary>
    OldTourianRailings,
    /// <summary>Old Tourian's background panels, flashing yellow during the escape.</summary>
    OldTourianPanels,
    /// <summary>Upper Crateria's level colours, flashing red during the escape.</summary>
    UpperCrateriaLevel,
    /// <summary>Crateria's sky, flashed by the yellow lightning during the escape.</summary>
    CrateriaEscapeLightning,
    /// <summary>The pixel of the CRE block the escape slightly changes.</summary>
    CreBlockPixel,
}
