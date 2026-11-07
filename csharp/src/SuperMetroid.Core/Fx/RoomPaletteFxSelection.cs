using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects each palette-FX object's setup and initial program.
/// </summary>
internal static class RoomPaletteFxSelection
{
    /// <summary>Selects setup callback and initial program by palette-FX object.</summary>
    /// <remarks>Each object dispatches to one setup operation and animation program. Named setup cases preserve
    /// null/intro/Norfair/Brinstar semantics, including aliases between distinct objects. Both fields are
    /// independently checked against supported NTSC J/U v1.0 and the pinned reference sources
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    internal static RoomPaletteFxDefinition Get(PaletteFxObject paletteFxObject) => paletteFxObject switch
    {
        PaletteFxObject.FadeInSuperMetroidTitleLogo => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInSuperMetroidTitleLogo),
        PaletteFxObject.FadeInNintendoBootLogo => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInNintendoBootLogo),
        PaletteFxObject.FadeInNintendoCopyright => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInNintendoCopyright),
        PaletteFxObject.TitleScreenBabyMetroidTubeLight => new(PaletteFxSetup.Null, PaletteFxProgram.TitleScreenBabyMetroidTubeLight0),
        PaletteFxObject.TitleScreenFlickeringDisplays => new(PaletteFxSetup.Null, PaletteFxProgram.TitleScreenFlickeringDisplays0),
        PaletteFxObject.CutsceneGunshipEngineFlicker => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneGunshipEngineFlicker0),
        PaletteFxObject.CutsceneCeresNavigationLightsSprite => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneCeresNavigationLightSprite),
        PaletteFxObject.FadeInPlanetZebesText => new(PaletteFxSetup.Null, PaletteFxProgram.FadeInPlanetZebesText),
        PaletteFxObject.FadeOutPlanetZebesText => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutPlanetZebesText),
        PaletteFxObject.CutsceneCeresNavigationLightsBackground => new(PaletteFxSetup.Null, PaletteFxProgram.CutsceneCeresNavigationLightsBG),
        PaletteFxObject.OldMotherBrainFightBackgroundLights => new(PaletteFxSetup.Intro, PaletteFxProgram.OldMotherBrainFightBGLights0),
        PaletteFxObject.GunshipGlow => new(PaletteFxSetup.Null, PaletteFxProgram.GunshipGlow0),
        PaletteFxObject.FadeOutZoomedOutExplodingZebes => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutZoomedOutExplodingZebes),
        PaletteFxObject.WidePartOfZebesExplosionForeground => new(PaletteFxSetup.Null, PaletteFxProgram.WidePartOfZebesExplosionForeground),
        PaletteFxObject.ZebesExplosionFinale => new(PaletteFxSetup.Null, PaletteFxProgram.ZebesExplosionFinale),
        PaletteFxObject.WhiteOutSpaceDuringZebesExplosion => new(PaletteFxSetup.Null, PaletteFxProgram.WhiteOutSpaceDuringZebesExplosion),
        PaletteFxObject.ZebesExplosionPlanetAfterglow => new(PaletteFxSetup.Null, PaletteFxProgram.ZebesExplosionPlanetAfterGlow0),
        PaletteFxObject.ExplodingZebesLava => new(PaletteFxSetup.Null, PaletteFxProgram.ExplodingZebesLava0),
        PaletteFxObject.FadeOutExplodingZebesCrust => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutExplodingZebesCrust),
        PaletteFxObject.FadeOutExplodingZebesGreyClouds => new(PaletteFxSetup.Null, PaletteFxProgram.FadeOutExplodingZebesGreyClouds),
        PaletteFxObject.GunshipEmergingFromZebesExplosion => new(PaletteFxSetup.Null, PaletteFxProgram.GunshipEmergingFromZebesExplosion),
        PaletteFxObject.WidePartOfZebesExplosionBackground => new(PaletteFxSetup.Null, PaletteFxProgram.WidePartOfZebesExplosionBackground),
        PaletteFxObject.UnusedCinematicFade => new(PaletteFxSetup.Null, PaletteFxProgram.ListD9D0),
        PaletteFxObject.HyperBeam => new(PaletteFxSetup.Null, PaletteFxProgram.HyperBeam0),
        PaletteFxObject.SamusLoadingPowerSuit => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingPowerSuit0),
        PaletteFxObject.SamusLoadingVariaSuit => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingVairaSuit0),
        PaletteFxObject.SamusLoadingGravitySuit => new(PaletteFxSetup.Null, PaletteFxProgram.SamusLoadingGravitySuit0),
        PaletteFxObject.PostCreditsSuperMetroidIcon => new(PaletteFxSetup.Null, PaletteFxProgram.PostCreditsSuperMetroidIconGlare),
        PaletteFxObject.Nothing => new(PaletteFxSetup.Null, PaletteFxProgram.Nothing),
        PaletteFxObject.GreyOutTourianStatueDraygon => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueDraygon),
        PaletteFxObject.GreyOutTourianStatueKraid => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueKraid),
        PaletteFxObject.GreyOutTourianStatueRidley => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatueRidley),
        PaletteFxObject.GreyOutTourianStatuePhantoon => new(PaletteFxSetup.Null, PaletteFxProgram.GreyOutTourianStatuePhantoon),
        PaletteFxObject.BombTorizoBelly => new(PaletteFxSetup.Null, PaletteFxProgram.BombTorizoBelly0),
        PaletteFxObject.GoldenTorizoBelly => new(PaletteFxSetup.Null, PaletteFxProgram.GoldenTorizoBelly0),
        PaletteFxObject.SamusInHeat => new(PaletteFxSetup.Norfair, PaletteFxProgram.SamusInHeatPowerSuit0),
        PaletteFxObject.CrateriaLightning => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria10),
        PaletteFxObject.UnusedDarkLightning => new(PaletteFxSetup.Null, PaletteFxProgram.ListEC6E),
        PaletteFxObject.WreckedShipGreenLights => new(PaletteFxSetup.Null, PaletteFxProgram.WreckedShip10),
        PaletteFxObject.UnusedWreckedShipGreenLightsDuplicate => new(PaletteFxSetup.Null, PaletteFxProgram.WreckedShip10),
        PaletteFxObject.BrinstarBlueBackgroundSpores => new(PaletteFxSetup.Null, PaletteFxProgram.Brinstar10),
        PaletteFxObject.SporeSpawnBlueBackgroundSpores => new(PaletteFxSetup.Brinstar, PaletteFxProgram.Brinstar80),
        PaletteFxObject.BrinstarRedBackgroundGlow => new(PaletteFxSetup.Null, PaletteFxProgram.Brinstar20),
        PaletteFxObject.BeaconFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria80Brinstar40),
        PaletteFxObject.NorfairForegroundAndHeatPhase => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair20),
        PaletteFxObject.NorfairForegroundPalette4 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair40),
        PaletteFxObject.NorfairForegroundPalette5 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair80),
        PaletteFxObject.NorfairForegroundPalette6 => new(PaletteFxSetup.Null, PaletteFxProgram.Norfair100),
        PaletteFxObject.MaridiaSandPits => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia10),
        PaletteFxObject.MaridiaSandFalls => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia20),
        PaletteFxObject.MaridiaBackgroundWaterfalls => new(PaletteFxSetup.Null, PaletteFxProgram.Maridia40),
        PaletteFxObject.TourianGlowingArkanoidBlocksAndRedOrbs => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian2),
        PaletteFxObject.UnusedTourianGlowClone => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian4),
        PaletteFxObject.TourianShutterRedFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian80),
        PaletteFxObject.TourianBackgroundRedFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian100),
        PaletteFxObject.TourianGeneralLevelRedFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian20),
        PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs => new(PaletteFxSetup.Null, PaletteFxProgram.Tourian40),
        PaletteFxObject.OldTourianRedFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria80),
        PaletteFxObject.OldTourianBackgroundRailingsFlashYellow => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria100),
        PaletteFxObject.OldTourianBackgroundPanelsFlashYellow => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria200),
        PaletteFxObject.UpperCrateriaRedFlashing => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria20),
        PaletteFxObject.CrateriaEscapeYellowLightning => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria40),
        PaletteFxObject.CrateriaEscapeCreBlockPixel => new(PaletteFxSetup.Null, PaletteFxProgram.Crateria400),
        _ => throw new ArgumentOutOfRangeException(nameof(paletteFxObject), paletteFxObject, "Not a palette-FX object."),
    };
}
