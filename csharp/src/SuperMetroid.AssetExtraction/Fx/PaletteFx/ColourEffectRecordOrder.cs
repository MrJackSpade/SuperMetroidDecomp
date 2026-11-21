using SuperMetroid.Core.Fx.ColourEffects;
using A = SuperMetroid.Core.Fx.ColourEffects.ColourEffectId;
using L = SuperMetroid.AssetExtraction.Fx.PaletteFxRecordDefinitions;

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
        new(A.FadeInSuperMetroidTitleLogo, 0, 8, L.Writes15),
        new(A.FadeInNintendoCopyright, 0, 8, L.Writes2),
        new(A.TitleScreenBabyMetroidTubeLight, 0, 8, L.Writes4),
        new(A.TitleScreenFlickeringDisplays, 0, 2, L.Writes2),
        new(A.CutsceneGunshipEngineFlicker, 0, 2, L.Writes1),
        new(A.CutsceneCeresNavigationLightsSprite, 0, 14, L.Writes2),
        new(A.FadeInPlanetZebesText, 0, 8, L.Writes3),
        new(A.FadeOutPlanetZebesText, 0, 8, L.Writes3),
        new(A.OldMotherBrainFightBackgroundLights, 0, 14, L.Writes3),
        new(A.GunshipGlow, 0, 14, L.Writes1),
        new(A.FadeOutZoomedOutExplodingZebes, 0, 7, L.Writes8),
        new(A.WidePartOfZebesExplosionForeground, 0, 16, L.Writes15),
        new(A.ZebesExplosionFinale, 0, 45, L.Writes15),
        new(A.WhiteOutSpaceDuringZebesExplosion, 0, 15, L.Writes1),
        new(A.ZebesExplosionPlanetAfterglow, 0, 6, L.Writes8),
        new(A.ExplodingZebesLava, 0, 10, L.Writes1),
        new(A.FadeOutExplodingZebesCrust, 0, 8, L.Writes15),
        new(A.FadeOutExplodingZebesGreyClouds, 0, 8, L.Writes15),
        new(A.GunshipEmergingFromZebesExplosion, 0, 16, L.Writes16),
        new(A.HyperBeam, 0, 10, L.Writes8),
        new(A.SamusLoadingPowerSuit, 0, 2, L.Writes16),
        new(A.SamusLoadingPowerSuit, 2, 2, L.Writes16),
        new(A.SamusLoadingPowerSuit, 4, 2, L.Writes16),
        new(A.SamusLoadingPowerSuit, 6, 3, L.Writes16),
        new(A.SamusLoadingVariaSuit, 0, 2, L.Writes16),
        new(A.SamusLoadingVariaSuit, 2, 2, L.Writes16),
        new(A.SamusLoadingVariaSuit, 4, 2, L.Writes16),
        new(A.SamusLoadingVariaSuit, 6, 3, L.Writes16),
        new(A.SamusLoadingGravitySuit, 0, 2, L.Writes16),
        new(A.SamusLoadingGravitySuit, 2, 2, L.Writes16),
        new(A.SamusLoadingGravitySuit, 4, 2, L.Writes16),
        new(A.SamusLoadingGravitySuit, 6, 3, L.Writes16),
        new(A.PostCreditsSuperMetroidIcon, 0, 14, L.Writes16),
        new(A.GreyOutTourianStatuePhantoon, 0, 8, L.Writes8),
        new(A.BombTorizoBelly, 0, 6, L.Writes3),
        new(A.GoldenTorizoBelly, 0, 6, L.Writes3),
        new(A.PowerSuitHeatPulse, 0, 16, L.Writes15),
        new(A.VariaSuitHeatPulse, 0, 16, L.Writes15),
        new(A.GravitySuitHeatPulse, 0, 16, L.Writes15),
        new(A.WreckedShipGreenLights, 0, 8, L.Writes2),
        new(A.CrateriaLightning, 0, 1, L.Writes8),
        new(A.CrateriaLightning, 1, 8, L.Writes8),
        new(A.CrateriaLightning, 9, 4, L.Writes8),
        new(A.BrinstarBlueBackgroundSpores, 0, 14, L.Writes3),
        new(A.SporeSpawnBlueBackgroundSpores, 0, 14, L.Writes3),
        new(A.BrinstarRedBackgroundGlow, 0, 14, L.Writes8),
        new(A.BeaconFlashing, 0, 10, L.Writes3Skips9Writes1),
        new(A.NorfairForegroundAndHeatPhase, 0, 16, L.Writes3Skips4Writes2),
        new(A.NorfairGreenForegroundHeatGlow, 0, 16, L.Writes3Skips8Writes2),
        new(A.NorfairPurpleForegroundHeatGlow, 0, 16, L.Writes3Skips8Writes2),
        new(A.NorfairDarkForegroundHeatGlow, 0, 16, L.Writes3Skips8Writes2),
        new(A.MaridiaSandPits, 0, 4, L.Writes8),
        new(A.MaridiaSandFalls, 0, 4, L.Writes4),
        new(A.MaridiaBackgroundWaterfalls, 0, 8, L.Writes8),
        new(A.TourianGlowingArkanoidBlocksAndRedOrbs, 0, 11, L.Writes1Skips3Writes7),
        new(A.TourianShutterRedFlashing, 0, 14, L.Writes6),
        new(A.TourianBackgroundRedFlashing, 0, 14, L.Writes4),
        new(A.TourianRedFlashingArkanoidBlocksAndRedOrbs, 0, 14, L.Writes6Skips4Writes1),
        new(A.OldTourianRedFlashing, 0, 14, L.Writes3Skips4Writes4Skips2Writes1),
        new(A.OldTourianBackgroundRailingsFlashYellow, 0, 15, L.Writes3),
        new(A.OldTourianBackgroundPanelsFlashYellow, 0, 15, L.Writes3),
        new(A.UpperCrateriaRedFlashing, 0, 14, L.Writes7),
        new(A.CrateriaEscapeYellowLightning, 0, 11, L.Writes11),
        new(A.ParlorEscapeSkyFlicker, 0, 11, L.Writes5),
    ];

    /// <summary>The effects whose frames show another effect's records, by the effect whose records they show.</summary>
    internal static IReadOnlyDictionary<ColourEffectId, ColourEffectId> ShowsRecordsOf { get; } =
        new Dictionary<ColourEffectId, ColourEffectId>
        {
            [A.CutsceneCeresNavigationLightsBackground] = A.CutsceneCeresNavigationLightsSprite,
            [A.GreyOutTourianStatueDraygon] = A.GreyOutTourianStatuePhantoon,
            [A.GreyOutTourianStatueKraid] = A.GreyOutTourianStatuePhantoon,
            [A.GreyOutTourianStatueRidley] = A.GreyOutTourianStatuePhantoon,
            [A.TourianGeneralLevelRedFlashing] = A.TourianRedFlashingArkanoidBlocksAndRedOrbs,
            [A.WidePartOfZebesExplosionBackground] = A.WhiteOutSpaceDuringZebesExplosion,
        };
}
