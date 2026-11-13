using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// How the cartridge lays out the colour effects' lists (#1600): the lists that end by running on into the list
/// authored next, so their frames and that list's are one run of consecutive records.
/// </summary>
internal static class PaletteFxListLayout
{
    /// <summary>The segments whose list runs on into the next authored list.</summary>
    internal static IReadOnlySet<PaletteFxSegment> RunOnIntoNext { get; } = new HashSet<PaletteFxSegment>
    {
        PaletteFxSegment.FadeInNintendoBootLogo,
        PaletteFxSegment.TitleScreenBabyMetroidTubeLight0,
        PaletteFxSegment.TitleScreenFlickeringDisplays0,
        PaletteFxSegment.CutsceneGunshipEngineFlicker0,
        PaletteFxSegment.CutsceneCeresNavigationLightSprite,
        PaletteFxSegment.OldMotherBrainFightBGLights0,
        PaletteFxSegment.GunshipGlow0,
        PaletteFxSegment.WhiteOutSpaceDuringZebesExplosion,
        PaletteFxSegment.ZebesExplosionPlanetAfterGlow0,
        PaletteFxSegment.ExplodingZebesLava0,
        PaletteFxSegment.HyperBeam0,
        PaletteFxSegment.SamusLoadingPowerSuit0,
        PaletteFxSegment.SamusLoadingPowerSuit1,
        PaletteFxSegment.SamusLoadingPowerSuit2,
        PaletteFxSegment.SamusLoadingPowerSuit3,
        PaletteFxSegment.SamusLoadingVairaSuit0,
        PaletteFxSegment.SamusLoadingVairaSuit1,
        PaletteFxSegment.SamusLoadingVairaSuit2,
        PaletteFxSegment.SamusLoadingVairaSuit3,
        PaletteFxSegment.SamusLoadingGravitySuit0,
        PaletteFxSegment.SamusLoadingGravitySuit1,
        PaletteFxSegment.SamusLoadingGravitySuit2,
        PaletteFxSegment.SamusLoadingGravitySuit3,
        PaletteFxSegment.GreyOutTourianStatuePhantoon,
        PaletteFxSegment.BombTorizoBelly0,
        PaletteFxSegment.GoldenTorizoBelly0,
        PaletteFxSegment.SamusInHeatPowerSuit,
        PaletteFxSegment.SamusInHeatVariaSuit,
        PaletteFxSegment.SamusInHeatGravitySuit,
        PaletteFxSegment.WreckedShipGreenLights,
        PaletteFxSegment.CrateriaLightning,
        PaletteFxSegment.CrateriaLightningCycle,
        PaletteFxSegment.CrateriaLightningRepeatedFlash,
        PaletteFxSegment.UnusedDarkLightning,
        PaletteFxSegment.UnusedDarkLightningCycle,
        PaletteFxSegment.UnusedDarkLightningRepeatedFlash,
        PaletteFxSegment.BrinstarBlueBackgroundSpores,
        PaletteFxSegment.SporeSpawnBlueBackgroundSpores,
        PaletteFxSegment.BrinstarRedBackgroundGlow,
        PaletteFxSegment.BeaconFlashing,
        PaletteFxSegment.NorfairForegroundAndHeatPhase,
        PaletteFxSegment.NorfairGreenForegroundHeatGlow,
        PaletteFxSegment.NorfairPurpleForegroundHeatGlow,
        PaletteFxSegment.NorfairDarkForegroundHeatGlow,
        PaletteFxSegment.MaridiaSandPits,
        PaletteFxSegment.MaridiaSandFalls,
        PaletteFxSegment.MaridiaBackgroundWaterfalls,
        PaletteFxSegment.TourianGlowingArkanoidBlocksAndRedOrbs,
        PaletteFxSegment.TourianShutterRedFlashing,
        PaletteFxSegment.TourianBackgroundRedFlashing,
        PaletteFxSegment.TourianRedFlashingArkanoidBlocksAndRedOrbs,
        PaletteFxSegment.OldTourianRedFlashing,
        PaletteFxSegment.OldTourianBackgroundRailingsFlashYellow,
        PaletteFxSegment.OldTourianBackgroundPanelsFlashYellow,
        PaletteFxSegment.UpperCrateriaRedFlashing,
        PaletteFxSegment.CrateriaEscapeYellowLightning,
        PaletteFxSegment.ParlorEscapeSkyFlicker,
    };
}
