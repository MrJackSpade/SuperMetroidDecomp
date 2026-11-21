using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The shades the import pre-colours pictures in for the colour effects' frames (#1512, #1598): one per colour group and
/// frame the group shows, which colours the pictures of that group show while the frame is the group's last. Each of an
/// effect's frames in one colour group is a step of one shade effect, a colour animation named for the group and the
/// effect (#1712, #1982).
/// </summary>
public static class ColourEffectShades
{
    /// <summary>
    /// Each colour group's name in its colour animations' installed names (#2511), written out so renaming a group's
    /// identity never changes a name on disk.
    /// </summary>
    private static readonly FrozenDictionary<ColourEffectColours, string> ColourKeys =
        new Dictionary<ColourEffectColours, string>
        {
            [ColourEffectColours.TitleLogo] = "TitleLogo",
            [ColourEffectColours.NintendoCopyright] = "NintendoCopyright",
            [ColourEffectColours.BabyMetroidTubeLight] = "BabyMetroidTubeLight",
            [ColourEffectColours.TitleScreenDisplays] = "TitleScreenDisplays",
            [ColourEffectColours.CutsceneGunshipEngine] = "CutsceneGunshipEngine",
            [ColourEffectColours.CeresNavigationLightsSprite] = "CeresNavigationLightsSprite",
            [ColourEffectColours.CeresNavigationLightsBackground] = "CeresNavigationLightsBackground",
            [ColourEffectColours.PlanetZebesText] = "PlanetZebesText",
            [ColourEffectColours.OldMotherBrainBackgroundLights] = "OldMotherBrainBackgroundLights",
            [ColourEffectColours.GunshipGlow] = "GunshipGlow",
            [ColourEffectColours.ZoomedOutExplodingZebes] = "ZoomedOutExplodingZebes",
            [ColourEffectColours.WideZebesExplosionForeground] = "WideZebesExplosionForeground",
            [ColourEffectColours.WideZebesExplosionBackground] = "WideZebesExplosionBackground",
            [ColourEffectColours.ZebesExplosionFinale] = "ZebesExplosionFinale",
            [ColourEffectColours.SpaceDuringZebesExplosion] = "SpaceDuringZebesExplosion",
            [ColourEffectColours.ZebesPlanetAfterglow] = "ZebesPlanetAfterglow",
            [ColourEffectColours.ExplodingZebesLava] = "ExplodingZebesLava",
            [ColourEffectColours.ExplodingZebesCrust] = "ExplodingZebesCrust",
            [ColourEffectColours.ExplodingZebesGreyClouds] = "ExplodingZebesGreyClouds",
            [ColourEffectColours.GunshipEmergingFromExplosion] = "GunshipEmergingFromExplosion",
            [ColourEffectColours.HyperBeam] = "HyperBeam",
            [ColourEffectColours.SamusLoadingSuit] = "SamusLoadingSuit",
            [ColourEffectColours.PostCreditsSuperMetroidIcon] = "PostCreditsSuperMetroidIcon",
            [ColourEffectColours.DraygonStatue] = "DraygonStatue",
            [ColourEffectColours.KraidStatue] = "KraidStatue",
            [ColourEffectColours.RidleyStatue] = "RidleyStatue",
            [ColourEffectColours.PhantoonStatue] = "PhantoonStatue",
            [ColourEffectColours.TorizoBelly] = "TorizoBelly",
            [ColourEffectColours.HeatSuitGlow] = "HeatSuitGlow",
            [ColourEffectColours.WreckedShipGreenLights] = "WreckedShipGreenLights",
            [ColourEffectColours.LandingSiteLightning] = "LandingSiteLightning",
            [ColourEffectColours.BrinstarBlueSpores] = "BrinstarBlueSpores",
            [ColourEffectColours.BrinstarRedGlow] = "BrinstarRedGlow",
            [ColourEffectColours.Beacon] = "Beacon",
            [ColourEffectColours.NorfairHeatForeground] = "NorfairHeatForeground",
            [ColourEffectColours.NorfairGreenForeground] = "NorfairGreenForeground",
            [ColourEffectColours.NorfairPurpleForeground] = "NorfairPurpleForeground",
            [ColourEffectColours.NorfairDarkForeground] = "NorfairDarkForeground",
            [ColourEffectColours.MaridiaSandPits] = "MaridiaSandPits",
            [ColourEffectColours.MaridiaSandFalls] = "MaridiaSandFalls",
            [ColourEffectColours.MaridiaBackgroundWaterfalls] = "MaridiaBackgroundWaterfalls",
            [ColourEffectColours.TourianArkanoidBlocksAndOrbs] = "TourianArkanoidBlocksAndOrbs",
            [ColourEffectColours.TourianShutters] = "TourianShutters",
            [ColourEffectColours.TourianBackground] = "TourianBackground",
            [ColourEffectColours.TourianLevel] = "TourianLevel",
            [ColourEffectColours.OldTourianLevel] = "OldTourianLevel",
            [ColourEffectColours.OldTourianRailings] = "OldTourianRailings",
            [ColourEffectColours.OldTourianPanels] = "OldTourianPanels",
            [ColourEffectColours.UpperCrateriaLevel] = "UpperCrateriaLevel",
            [ColourEffectColours.CrateriaEscapeLightning] = "CrateriaEscapeLightning",
            [ColourEffectColours.ParlorEscapeSky] = "ParlorEscapeSky",
        }.ToFrozenDictionary();

    /// <summary>
    /// Each colour effect's name in its colour animations' installed names (#2511), written out so renaming an effect's
    /// identity never changes a name on disk.
    /// </summary>
    private static readonly IdentityTable<ColourEffectId, string> EffectKeys =
        new(new Dictionary<ColourEffectId, string>
        {
            [ColourEffectId.FadeInSuperMetroidTitleLogo] = "FadeInSuperMetroidTitleLogo",
            [ColourEffectId.FadeInNintendoCopyright] = "FadeInNintendoCopyright",
            [ColourEffectId.TitleScreenBabyMetroidTubeLight] = "TitleScreenBabyMetroidTubeLight",
            [ColourEffectId.TitleScreenFlickeringDisplays] = "TitleScreenFlickeringDisplays",
            [ColourEffectId.CutsceneGunshipEngineFlicker] = "CutsceneGunshipEngineFlicker",
            [ColourEffectId.CutsceneCeresNavigationLightsSprite] = "CutsceneCeresNavigationLightsSprite",
            [ColourEffectId.FadeInPlanetZebesText] = "FadeInPlanetZebesText",
            [ColourEffectId.FadeOutPlanetZebesText] = "FadeOutPlanetZebesText",
            [ColourEffectId.CutsceneCeresNavigationLightsBackground] = "CutsceneCeresNavigationLightsBackground",
            [ColourEffectId.OldMotherBrainFightBackgroundLights] = "OldMotherBrainFightBackgroundLights",
            [ColourEffectId.GunshipGlow] = "GunshipGlow",
            [ColourEffectId.FadeOutZoomedOutExplodingZebes] = "FadeOutZoomedOutExplodingZebes",
            [ColourEffectId.WidePartOfZebesExplosionForeground] = "WidePartOfZebesExplosionForeground",
            [ColourEffectId.ZebesExplosionFinale] = "ZebesExplosionFinale",
            [ColourEffectId.WhiteOutSpaceDuringZebesExplosion] = "WhiteOutSpaceDuringZebesExplosion",
            [ColourEffectId.ZebesExplosionPlanetAfterglow] = "ZebesExplosionPlanetAfterglow",
            [ColourEffectId.ExplodingZebesLava] = "ExplodingZebesLava",
            [ColourEffectId.FadeOutExplodingZebesCrust] = "FadeOutExplodingZebesCrust",
            [ColourEffectId.FadeOutExplodingZebesGreyClouds] = "FadeOutExplodingZebesGreyClouds",
            [ColourEffectId.GunshipEmergingFromZebesExplosion] = "GunshipEmergingFromZebesExplosion",
            [ColourEffectId.WidePartOfZebesExplosionBackground] = "WidePartOfZebesExplosionBackground",
            [ColourEffectId.HyperBeam] = "HyperBeam",
            [ColourEffectId.SamusLoadingPowerSuit] = "SamusLoadingPowerSuit",
            [ColourEffectId.SamusLoadingVariaSuit] = "SamusLoadingVariaSuit",
            [ColourEffectId.SamusLoadingGravitySuit] = "SamusLoadingGravitySuit",
            [ColourEffectId.PostCreditsSuperMetroidIcon] = "PostCreditsSuperMetroidIcon",
            [ColourEffectId.GreyOutTourianStatueDraygon] = "GreyOutTourianStatueDraygon",
            [ColourEffectId.GreyOutTourianStatueKraid] = "GreyOutTourianStatueKraid",
            [ColourEffectId.GreyOutTourianStatueRidley] = "GreyOutTourianStatueRidley",
            [ColourEffectId.GreyOutTourianStatuePhantoon] = "GreyOutTourianStatuePhantoon",
            [ColourEffectId.BombTorizoBelly] = "BombTorizoBelly",
            [ColourEffectId.GoldenTorizoBelly] = "GoldenTorizoBelly",
            [ColourEffectId.SamusInHeat] = "SamusInHeat",
            [ColourEffectId.PowerSuitHeatPulse] = "PowerSuitHeatPulse",
            [ColourEffectId.VariaSuitHeatPulse] = "VariaSuitHeatPulse",
            [ColourEffectId.GravitySuitHeatPulse] = "GravitySuitHeatPulse",
            [ColourEffectId.CrateriaLightning] = "CrateriaLightning",
            [ColourEffectId.WreckedShipGreenLights] = "WreckedShipGreenLights",
            [ColourEffectId.BrinstarBlueBackgroundSpores] = "BrinstarBlueBackgroundSpores",
            [ColourEffectId.SporeSpawnBlueBackgroundSpores] = "SporeSpawnBlueBackgroundSpores",
            [ColourEffectId.BrinstarRedBackgroundGlow] = "BrinstarRedBackgroundGlow",
            [ColourEffectId.BeaconFlashing] = "BeaconFlashing",
            [ColourEffectId.NorfairForegroundAndHeatPhase] = "NorfairForegroundAndHeatPhase",
            [ColourEffectId.NorfairGreenForegroundHeatGlow] = "NorfairGreenForegroundHeatGlow",
            [ColourEffectId.NorfairPurpleForegroundHeatGlow] = "NorfairPurpleForegroundHeatGlow",
            [ColourEffectId.NorfairDarkForegroundHeatGlow] = "NorfairDarkForegroundHeatGlow",
            [ColourEffectId.MaridiaSandPits] = "MaridiaSandPits",
            [ColourEffectId.MaridiaSandFalls] = "MaridiaSandFalls",
            [ColourEffectId.MaridiaBackgroundWaterfalls] = "MaridiaBackgroundWaterfalls",
            [ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs] = "TourianGlowingArkanoidBlocksAndRedOrbs",
            [ColourEffectId.TourianShutterRedFlashing] = "TourianShutterRedFlashing",
            [ColourEffectId.TourianBackgroundRedFlashing] = "TourianBackgroundRedFlashing",
            [ColourEffectId.TourianGeneralLevelRedFlashing] = "TourianGeneralLevelRedFlashing",
            [ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs] = "TourianRedFlashingArkanoidBlocksAndRedOrbs",
            [ColourEffectId.OldTourianRedFlashing] = "OldTourianRedFlashing",
            [ColourEffectId.OldTourianBackgroundRailingsFlashYellow] = "OldTourianBackgroundRailingsFlashYellow",
            [ColourEffectId.OldTourianBackgroundPanelsFlashYellow] = "OldTourianBackgroundPanelsFlashYellow",
            [ColourEffectId.UpperCrateriaRedFlashing] = "UpperCrateriaRedFlashing",
            [ColourEffectId.CrateriaEscapeYellowLightning] = "CrateriaEscapeYellowLightning",
            [ColourEffectId.ParlorEscapeSkyFlicker] = "ParlorEscapeSkyFlicker",
        });

    /// <summary>The shade of <paramref name="colours"/> showing <paramref name="frame"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group or the frame's effect has no installed name.</exception>
    public static PictureShade Of(ColourEffectColours colours, ColourEffectFrame frame) =>
        new($"colour-animation-{ColourKeys[colours]}-{EffectKeys[frame.Effect]}", frame.Frame);
}
