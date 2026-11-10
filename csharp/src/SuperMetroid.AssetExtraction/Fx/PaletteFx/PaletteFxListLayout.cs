using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// How the cartridge lays out the colour effects' lists (#1600): the lists that end by running on into the list
/// authored next, so their frames and that list's are one run of consecutive records.
/// </summary>
internal static class PaletteFxListLayout
{
    /// <summary>The segments whose list runs on into the next authored list.</summary>
    internal static IReadOnlySet<PaletteFxProgram> RunOnIntoNext { get; } = new HashSet<PaletteFxProgram>
    {
        PaletteFxProgram.FadeInNintendoBootLogo,
        PaletteFxProgram.TitleScreenBabyMetroidTubeLight0,
        PaletteFxProgram.TitleScreenFlickeringDisplays0,
        PaletteFxProgram.CutsceneGunshipEngineFlicker0,
        PaletteFxProgram.CutsceneCeresNavigationLightSprite,
        PaletteFxProgram.OldMotherBrainFightBGLights0,
        PaletteFxProgram.GunshipGlow0,
        PaletteFxProgram.WhiteOutSpaceDuringZebesExplosion,
        PaletteFxProgram.ZebesExplosionPlanetAfterGlow0,
        PaletteFxProgram.ExplodingZebesLava0,
        PaletteFxProgram.HyperBeam0,
        PaletteFxProgram.SamusLoadingPowerSuit0,
        PaletteFxProgram.SamusLoadingPowerSuit1,
        PaletteFxProgram.SamusLoadingPowerSuit2,
        PaletteFxProgram.SamusLoadingPowerSuit3,
        PaletteFxProgram.SamusLoadingVairaSuit0,
        PaletteFxProgram.SamusLoadingVairaSuit1,
        PaletteFxProgram.SamusLoadingVairaSuit2,
        PaletteFxProgram.SamusLoadingVairaSuit3,
        PaletteFxProgram.SamusLoadingGravitySuit0,
        PaletteFxProgram.SamusLoadingGravitySuit1,
        PaletteFxProgram.SamusLoadingGravitySuit2,
        PaletteFxProgram.SamusLoadingGravitySuit3,
        PaletteFxProgram.GreyOutTourianStatuePhantoon,
        PaletteFxProgram.BombTorizoBelly0,
        PaletteFxProgram.GoldenTorizoBelly0,
        PaletteFxProgram.SamusInHeatPowerSuit,
        PaletteFxProgram.SamusInHeatVariaSuit,
        PaletteFxProgram.SamusInHeatGravitySuit,
        PaletteFxProgram.WreckedShipGreenLights,
        PaletteFxProgram.CrateriaLightning,
        PaletteFxProgram.CrateriaLightningCycle,
        PaletteFxProgram.CrateriaLightningRepeatedFlash,
        PaletteFxProgram.UnusedDarkLightning,
        PaletteFxProgram.UnusedDarkLightningCycle,
        PaletteFxProgram.UnusedDarkLightningRepeatedFlash,
        PaletteFxProgram.BrinstarBlueBackgroundSpores,
        PaletteFxProgram.SporeSpawnBlueBackgroundSpores,
        PaletteFxProgram.BrinstarRedBackgroundGlow,
        PaletteFxProgram.BeaconFlashing,
        PaletteFxProgram.NorfairForegroundAndHeatPhase,
        PaletteFxProgram.NorfairGreenForegroundHeatGlow,
        PaletteFxProgram.NorfairPurpleForegroundHeatGlow,
        PaletteFxProgram.NorfairDarkForegroundHeatGlow,
        PaletteFxProgram.MaridiaSandPits,
        PaletteFxProgram.MaridiaSandFalls,
        PaletteFxProgram.MaridiaBackgroundWaterfalls,
        PaletteFxProgram.TourianGlowingArkanoidBlocksAndRedOrbs,
        PaletteFxProgram.TourianShutterRedFlashing,
        PaletteFxProgram.TourianBackgroundRedFlashing,
        PaletteFxProgram.TourianRedFlashingArkanoidBlocksAndRedOrbs,
        PaletteFxProgram.OldTourianRedFlashing,
        PaletteFxProgram.OldTourianBackgroundRailingsFlashYellow,
        PaletteFxProgram.OldTourianBackgroundPanelsFlashYellow,
        PaletteFxProgram.UpperCrateriaRedFlashing,
        PaletteFxProgram.CrateriaEscapeYellowLightning,
        PaletteFxProgram.ParlorEscapeSkyFlicker,
    };
}
