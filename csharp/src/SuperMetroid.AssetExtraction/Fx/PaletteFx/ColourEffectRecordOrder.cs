using SuperMetroid.Core.Fx.ColourEffects;
using A = SuperMetroid.Core.Fx.ColourEffects.ColourEffectAnimation;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Where the cartridge keeps the colour effects' records (#1598, #1739): the import's private knowledge of their order.
/// The records lie in runs, each run some consecutive frames of one animation, in the order <see cref="Runs"/> lists them;
/// an animation whose frames are another's (<see cref="ShowsRecordsOf"/>) shows that animation's records, frame for frame.
/// The palette-FX program checks compare each record's place with its cartridge address.
/// </summary>
internal static class ColourEffectRecordOrder
{
    /// <summary>The runs of records, in the cartridge's order.</summary>
    internal static IReadOnlyList<ColourEffectRecordRun> Runs { get; } =
    [
        new(A.FadeInSuperMetroidTitleLogo, 0, 8),
        new(A.FadeInNintendoCopyright, 0, 8),
        new(A.TitleScreenBabyMetroidTubeLight, 0, 8),
        new(A.TitleScreenFlickeringDisplays, 0, 2),
        new(A.CutsceneGunshipEngineFlicker, 0, 2),
        new(A.CutsceneCeresNavigationLightsSprite, 0, 14),
        new(A.FadeInPlanetZebesText, 0, 8),
        new(A.FadeOutPlanetZebesText, 0, 8),
        new(A.OldMotherBrainFightBackgroundLights, 0, 14),
        new(A.GunshipGlow, 0, 14),
        new(A.FadeOutZoomedOutExplodingZebes, 0, 7),
        new(A.WidePartOfZebesExplosionForeground, 0, 16),
        new(A.ZebesExplosionFinale, 0, 45),
        new(A.WhiteOutSpaceDuringZebesExplosion, 0, 15),
        new(A.ZebesExplosionPlanetAfterglow, 0, 6),
        new(A.ExplodingZebesLava, 0, 10),
        new(A.FadeOutExplodingZebesCrust, 0, 8),
        new(A.FadeOutExplodingZebesGreyClouds, 0, 8),
        new(A.GunshipEmergingFromZebesExplosion, 0, 16),
        new(A.HyperBeam, 0, 10),
        new(A.SamusLoadingPowerSuit, 0, 2),
        new(A.SamusLoadingPowerSuit, 2, 2),
        new(A.SamusLoadingPowerSuit, 4, 2),
        new(A.SamusLoadingPowerSuit, 6, 3),
        new(A.SamusLoadingVariaSuit, 0, 2),
        new(A.SamusLoadingVariaSuit, 2, 2),
        new(A.SamusLoadingVariaSuit, 4, 2),
        new(A.SamusLoadingVariaSuit, 6, 3),
        new(A.SamusLoadingGravitySuit, 0, 2),
        new(A.SamusLoadingGravitySuit, 2, 2),
        new(A.SamusLoadingGravitySuit, 4, 2),
        new(A.SamusLoadingGravitySuit, 6, 3),
        new(A.PostCreditsSuperMetroidIcon, 0, 14),
        new(A.GreyOutTourianStatuePhantoon, 0, 8),
        new(A.BombTorizoBelly, 0, 6),
        new(A.GoldenTorizoBelly, 0, 6),
        new(A.SamusInHeatPowerSuit, 0, 16),
        new(A.SamusInHeatVariaSuit, 0, 16),
        new(A.SamusInHeatGravitySuit, 0, 16),
        new(A.WreckedShipGreenLights, 0, 8),
        new(A.CrateriaLightning, 0, 1),
        new(A.CrateriaLightning, 1, 8),
        new(A.CrateriaLightning, 9, 4),
        new(A.BrinstarBlueBackgroundSpores, 0, 14),
        new(A.SporeSpawnBlueBackgroundSpores, 0, 14),
        new(A.BrinstarRedBackgroundGlow, 0, 14),
        new(A.BeaconFlashing, 0, 10),
        new(A.NorfairForegroundAndHeatPhase, 0, 16),
        new(A.NorfairGreenForegroundHeatGlow, 0, 16),
        new(A.NorfairPurpleForegroundHeatGlow, 0, 16),
        new(A.NorfairDarkForegroundHeatGlow, 0, 16),
        new(A.MaridiaSandPits, 0, 4),
        new(A.MaridiaSandFalls, 0, 4),
        new(A.MaridiaBackgroundWaterfalls, 0, 8),
        new(A.TourianGlowingArkanoidBlocksAndRedOrbs, 0, 11),
        new(A.TourianShutterRedFlashing, 0, 14),
        new(A.TourianBackgroundRedFlashing, 0, 14),
        new(A.TourianRedFlashingArkanoidBlocksAndRedOrbs, 0, 14),
        new(A.OldTourianRedFlashing, 0, 14),
        new(A.OldTourianBackgroundRailingsFlashYellow, 0, 15),
        new(A.OldTourianBackgroundPanelsFlashYellow, 0, 15),
        new(A.UpperCrateriaRedFlashing, 0, 14),
        new(A.CrateriaEscapeYellowLightning, 0, 11),
        new(A.ParlorEscapeSkyFlicker, 0, 11),
    ];

    /// <summary>The animations whose frames show another animation's records, by the animation whose records they show.</summary>
    internal static IReadOnlyDictionary<ColourEffectAnimation, ColourEffectAnimation> ShowsRecordsOf { get; } =
        new Dictionary<ColourEffectAnimation, ColourEffectAnimation>
        {
            [A.CutsceneCeresNavigationLightsBackground] = A.CutsceneCeresNavigationLightsSprite,
            [A.GreyOutTourianStatueDraygon] = A.GreyOutTourianStatuePhantoon,
            [A.GreyOutTourianStatueKraid] = A.GreyOutTourianStatuePhantoon,
            [A.GreyOutTourianStatueRidley] = A.GreyOutTourianStatuePhantoon,
            [A.TourianGeneralLevelRedFlashing] = A.TourianRedFlashingArkanoidBlocksAndRedOrbs,
            [A.WidePartOfZebesExplosionBackground] = A.WhiteOutSpaceDuringZebesExplosion,
        };
}
