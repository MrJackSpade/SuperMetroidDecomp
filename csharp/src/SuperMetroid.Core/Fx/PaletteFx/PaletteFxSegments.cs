using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Fx.PaletteFx.PaletteFxSegment;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour effects' segments (#1298, #1554, #1598, #1600): each segment's frames, named by their place among the
/// segment's frames (a <see cref="PaletteFxFrame"/> of that segment), with their durations, and the
/// <see cref="PaletteFxCue"/> events the running effect reacts to between them.
/// </summary>
internal static class PaletteFxSegments
{
    /// <summary>Every segment.</summary>
    internal static AnimationSegments<PaletteFxSegment, int, PaletteFxCue> All { get; } = new(
        new Dictionary<PaletteFxSegment, AnimationBeat<int, PaletteFxCue>[]>
    {
        [P.FadeInSuperMetroidTitleLogo] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TitleLogo)),
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
        ],
        [P.FadeInNintendoBootLogo] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NintendoBootLogo)),
        ],
        [P.CommonFadeInLogo] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
        ],
        [P.FadeInNintendoCopyright] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NintendoCopyright)),
        ],
        [P.TitleScreenBabyMetroidTubeLight0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.BabyMetroidTubeLight)),
        ],
        [P.TitleScreenBabyMetroidTubeLight1] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
        ],
        [P.TitleScreenFlickeringDisplays0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TitleScreenDisplays)),
        ],
        [P.TitleScreenFlickeringDisplays1] =
        [
            F(0, 1),
            F(1, 1),
        ],
        [P.CutsceneGunshipEngineFlicker0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.CutsceneGunshipEngine)),
        ],
        [P.CutsceneGunshipEngineFlicker1] =
        [
            F(0, 1),
            F(1, 1),
        ],
        [P.CutsceneCeresNavigationLightSprite] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.CeresNavigationLightsSprite)),
        ],
        [P.CommonCeresNavigationLights] =
        [
            F(0, 4),
            F(1, 4),
            F(2, 4),
            F(3, 4),
            F(4, 4),
            F(5, 4),
            F(6, 4),
            F(7, 4),
            F(8, 4),
            F(9, 4),
            F(10, 4),
            F(11, 4),
            F(12, 4),
            F(13, 4),
        ],
        [P.CutsceneCeresNavigationLightsBG] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.CeresNavigationLightsBackground)),
        ],
        [P.FadeInPlanetZebesText] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.PlanetZebesText)),
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
        ],
        [P.FadeOutPlanetZebesText] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.PlanetZebesText)),
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
        ],
        [P.OldMotherBrainFightBGLights0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.OldMotherBrainBackgroundLights)),
        ],
        [P.OldMotherBrainFightBGLights1] =
        [
            F(0, 6),
            F(1, 6),
            F(2, 6),
            F(3, 6),
            F(4, 6),
            F(5, 6),
            F(6, 6),
            F(7, 6),
            F(8, 6),
            F(9, 6),
            F(10, 6),
            F(11, 6),
            F(12, 6),
            F(13, 6),
        ],
        [P.GunshipGlow0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.GunshipGlow)),
        ],
        [P.GunshipGlow1] =
        [
            F(0, 5),
            F(1, 5),
            F(2, 5),
            F(3, 5),
            F(4, 5),
            F(5, 5),
            F(6, 5),
            F(7, 5),
            F(8, 5),
            F(9, 5),
            F(10, 5),
            F(11, 5),
            F(12, 5),
            F(13, 5),
        ],
        [P.FadeOutZoomedOutExplodingZebes] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ZoomedOutExplodingZebes)),
            F(0, 8),
            F(1, 8),
            F(2, 8),
            F(3, 8),
            F(4, 8),
            F(5, 8),
            F(6, 8),
        ],
        [P.WidePartOfZebesExplosionForeground] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.WideZebesExplosionForeground)),
            F(0, 4),
            F(1, 4),
            F(2, 4),
            F(3, 60),
            F(4, 6),
            F(5, 6),
            F(6, 6),
            F(7, 6),
            F(8, 6),
            F(9, 6),
            F(10, 6),
            F(11, 6),
            F(12, 6),
            F(13, 6),
            F(14, 6),
            F(15, 6),
        ],
        [P.ZebesExplosionFinale] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ZebesExplosionFinale)),
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
            F(10, 2),
            F(11, 2),
            F(12, 2),
            F(13, 2),
            F(14, 2),
            F(15, 9),
            F(16, 9),
            F(17, 9),
            F(18, 9),
            F(19, 9),
            F(20, 9),
            F(21, 9),
            F(22, 9),
            F(23, 9),
            F(24, 9),
            F(25, 9),
            F(26, 9),
            F(27, 9),
            F(28, 9),
            F(29, 9),
            F(30, 9),
            F(31, 9),
            F(32, 9),
            F(33, 9),
            F(34, 9),
            F(35, 9),
            F(36, 9),
            F(37, 9),
            F(38, 9),
            F(39, 9),
            F(40, 9),
            F(41, 9),
            F(42, 9),
            F(43, 9),
            F(44, 9),
        ],
        [P.WidePartOfZebesExplosionBackground] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.WideZebesExplosionBackground)),
        ],
        [P.WhiteOutSpaceDuringZebesExplosion] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.SpaceDuringZebesExplosion)),
        ],
        [P.CommonZebesExplosion] =
        [
            F(0, 14),
            F(1, 14),
            F(2, 14),
            F(3, 14),
            F(4, 14),
            F(5, 14),
            F(6, 14),
            F(7, 14),
            F(8, 14),
            F(9, 14),
            F(10, 14),
            F(11, 14),
            F(12, 14),
            F(13, 14),
            F(14, 14),
        ],
        [P.ZebesExplosionPlanetAfterGlow0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ZebesPlanetAfterglow)),
        ],
        [P.ZebesExplosionPlanetAfterGlow1] =
        [
            F(0, 16),
            F(1, 16),
            F(2, 16),
            F(3, 16),
            F(4, 16),
            F(5, 16),
        ],
        [P.ExplodingZebesLava0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesLava)),
        ],
        [P.ExplodingZebesLava1] =
        [
            F(0, 9),
            F(1, 8),
            F(2, 7),
            F(3, 6),
            F(4, 5),
            F(5, 5),
            F(6, 6),
            F(7, 7),
            F(8, 8),
            F(9, 9),
        ],
        [P.FadeOutExplodingZebesCrust] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesCrust)),
            F(0, 20),
            F(1, 20),
            F(2, 20),
            F(3, 20),
            F(4, 20),
            F(5, 20),
            F(6, 20),
            F(7, 20),
        ],
        [P.FadeOutExplodingZebesGreyClouds] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesGreyClouds)),
            F(0, 14),
            F(1, 14),
            F(2, 14),
            F(3, 14),
            F(4, 14),
            F(5, 14),
            F(6, 14),
            F(7, 14),
        ],
        [P.GunshipEmergingFromZebesExplosion] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.GunshipEmergingFromExplosion)),
            F(0, 24),
            F(1, 24),
            F(2, 24),
            F(3, 24),
            F(4, 24),
            F(5, 24),
            F(6, 24),
            F(7, 24),
            F(8, 24),
            F(9, 24),
            F(10, 24),
            F(11, 24),
            F(12, 24),
            F(13, 24),
            F(14, 24),
            F(15, 24),
        ],
        [P.HyperBeam0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.HyperBeam)),
        ],
        [P.HyperBeam1] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
        ],
        [P.SamusLoadingPowerSuit0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
        ],
        [P.SamusLoadingPowerSuit1] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingPowerSuit2] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingPowerSuit3] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingPowerSuit4] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 1),
        ],
        [P.SamusLoadingVairaSuit0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
        ],
        [P.SamusLoadingVairaSuit1] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingVairaSuit2] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingVairaSuit3] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingVairaSuit4] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 1),
        ],
        [P.SamusLoadingGravitySuit0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
        ],
        [P.SamusLoadingGravitySuit1] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingGravitySuit2] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingGravitySuit3] =
        [
            F(0, 3),
            F(1, 3),
        ],
        [P.SamusLoadingGravitySuit4] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 1),
        ],
        [P.PostCreditsSuperMetroidIconGlare] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.PostCreditsSuperMetroidIcon)),
            F(0, 1),
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 1),
            F(6, 1),
            F(7, 1),
            F(8, 1),
            F(9, 1),
            F(10, 1),
            F(11, 1),
            F(12, 1),
            F(13, 1),
        ],
        [P.Nothing] =
        [
        ],
        [P.GreyOutTourianStatueDraygon] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.DraygonStatue)),
        ],
        [P.GreyOutTourianStatueKraid] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.KraidStatue)),
        ],
        [P.GreyOutTourianStatueRidley] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.RidleyStatue)),
        ],
        [P.GreyOutTourianStatuePhantoon] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.PhantoonStatue)),
        ],
        [P.CommonGreyOutTourianStatue] =
        [
            F(0, 8),
            F(1, 8),
            F(2, 8),
            F(3, 8),
            F(4, 8),
            F(5, 8),
            F(6, 8),
            F(7, 8),
        ],
        [P.BombTorizoBelly0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TorizoBelly)),
            E(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenTorizoDies.Instance)),
        ],
        [P.BombTorizoBelly1] =
        [
            F(0, 10),
            F(1, 8),
            F(2, 8),
            F(3, 10),
            F(4, 8),
            F(5, 8),
        ],
        [P.GoldenTorizoBelly0] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TorizoBelly)),
            E(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenTorizoDies.Instance)),
        ],
        [P.GoldenTorizoBelly1] =
        [
            F(0, 10),
            F(1, 8),
            F(2, 8),
            F(3, 10),
            F(4, 8),
            F(5, 8),
        ],
        [P.SamusInHeatPowerSuit] =
        [
            E(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            E(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
        ],
        [P.SamusInHeatPowerSuitPulse] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 3),
        ],
        [P.SamusInHeatVariaSuit] =
        [
            E(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            E(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
        ],
        [P.SamusInHeatVariaSuitPulse] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 16),
        ],
        [P.SamusInHeatGravitySuit] =
        [
            E(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            E(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
        ],
        [P.SamusInHeatGravitySuitPulse] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 16),
        ],
        [P.WreckedShipGreenLights] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.WreckedShipGreenLights)),
        ],
        [P.WreckedShipGreenLightsCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
        ],
        [P.CrateriaLightning] =
        [
            E(new PaletteFxCue.Watch(new ColourEffectCondition.RestartsWhileSamusIsHigh(CrateriaLightningDefinitions.SamusHighOnSurfaceHeight, P.CrateriaLightningCycle))),
            E(new PaletteFxCue.SetColours(PaletteFxColours.LandingSiteLightning)),
        ],
        [P.CrateriaLightningCycle] =
        [
            F(0, 240),
        ],
        [P.CrateriaLightningRepeatedFlash] =
        [
            F(0, 2),
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 1),
            F(6, 2),
            F(7, 240),
        ],
        [P.CrateriaLightningClosingFlash] =
        [
            F(0, 1),
            F(1, 1),
            F(2, 1),
            F(3, 2),
        ],
        [P.BrinstarBlueBackgroundSpores] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarBlueSpores)),
        ],
        [P.BrinstarBlueBackgroundSporesCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
            F(11, 10),
            F(12, 10),
            F(13, 10),
        ],
        [P.SporeSpawnBlueBackgroundSpores] =
        [
            E(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenAreaMiniBossDies.Instance)),
            E(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarBlueSpores)),
        ],
        [P.SporeSpawnBlueBackgroundSporesCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
            F(11, 10),
            F(12, 10),
            F(13, 10),
        ],
        [P.BrinstarRedBackgroundGlow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarRedGlow)),
        ],
        [P.BrinstarRedBackgroundGlowCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
            F(11, 10),
            F(12, 10),
            F(13, 10),
        ],
        [P.BeaconFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.Beacon)),
        ],
        [P.BeaconFlashingCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            E(new PaletteFxCue.QueueSound(Sound: EnemySound.Beacon)),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
        ],
        [P.NorfairForegroundAndHeatPhase] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NorfairHeatForeground)),
        ],
        [P.NorfairForegroundAndHeatPhaseCycle] =
        [
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(0))),
            F(0, 16),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(1))),
            F(1, 4),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(2))),
            F(2, 4),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(3))),
            F(3, 5),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(4))),
            F(4, 6),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(5))),
            F(5, 7),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(6))),
            F(6, 8),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(7))),
            F(7, 8),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(8))),
            F(8, 8),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(9))),
            F(9, 8),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(10))),
            F(10, 7),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(11))),
            F(11, 6),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(12))),
            F(12, 5),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(13))),
            F(13, 4),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(14))),
            F(14, 4),
            E(new PaletteFxCue.PublishHeatPhase(new HeatPhase(15))),
            F(15, 16),
        ],
        [P.NorfairGreenForegroundHeatGlow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NorfairGreenForeground)),
        ],
        [P.NorfairGreenForegroundHeatGlowCycle] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 16),
        ],
        [P.NorfairPurpleForegroundHeatGlow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NorfairPurpleForeground)),
        ],
        [P.NorfairPurpleForegroundHeatGlowCycle] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 16),
        ],
        [P.NorfairDarkForegroundHeatGlow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.NorfairDarkForeground)),
        ],
        [P.NorfairDarkForegroundHeatGlowCycle] =
        [
            F(0, 16),
            F(1, 4),
            F(2, 4),
            F(3, 5),
            F(4, 6),
            F(5, 7),
            F(6, 8),
            F(7, 8),
            F(8, 8),
            F(9, 8),
            F(10, 7),
            F(11, 6),
            F(12, 5),
            F(13, 4),
            F(14, 4),
            F(15, 16),
        ],
        [P.MaridiaSandPits] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandPits)),
        ],
        [P.MaridiaSandPitsCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
        ],
        [P.MaridiaSandFalls] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandFalls)),
        ],
        [P.MaridiaSandFallsCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
        ],
        [P.MaridiaBackgroundWaterfalls] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaBackgroundWaterfalls)),
        ],
        [P.MaridiaBackgroundWaterfallsCycle] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
        ],
        [P.TourianGlowingArkanoidBlocksAndRedOrbs] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TourianArkanoidBlocksAndOrbs)),
        ],
        [P.CommonTourianGlowCycle] =
        [
            F(0, 10),
            F(1, 10),
            F(2, 10),
            F(3, 10),
            F(4, 10),
            F(5, 10),
            F(6, 10),
            F(7, 10),
            F(8, 10),
            F(9, 10),
            F(10, 10),
        ],
        [P.TourianShutterRedFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TourianShutters)),
        ],
        [P.TourianShutterRedFlashingCycle] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
            F(10, 2),
            F(11, 2),
            F(12, 2),
            F(13, 2),
        ],
        [P.TourianBackgroundRedFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TourianBackground)),
        ],
        [P.TourianBackgroundRedFlashingCycle] =
        [
            F(0, 4),
            F(1, 4),
            F(2, 4),
            F(3, 4),
            F(4, 4),
            F(5, 4),
            F(6, 4),
            F(7, 4),
            F(8, 4),
            F(9, 4),
            F(10, 4),
            F(11, 4),
            F(12, 4),
            F(13, 4),
        ],
        [P.TourianGeneralLevelRedFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TourianLevel)),
        ],
        [P.TourianRedFlashingArkanoidBlocksAndRedOrbs] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.TourianArkanoidBlocksAndOrbs)),
        ],
        [P.CommonTourianEscapeRedFlashingCycle] =
        [
            F(0, 2),
            F(1, 2),
            F(2, 2),
            F(3, 2),
            F(4, 2),
            F(5, 2),
            F(6, 2),
            F(7, 2),
            F(8, 2),
            F(9, 2),
            F(10, 2),
            F(11, 2),
            F(12, 2),
            F(13, 2),
        ],
        [P.OldTourianRedFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianLevel)),
        ],
        [P.OldTourianRedFlashingCycle] =
        [
            F(0, 3),
            F(1, 3),
            F(2, 3),
            F(3, 3),
            F(4, 3),
            F(5, 3),
            F(6, 3),
            F(7, 3),
            F(8, 3),
            F(9, 3),
            F(10, 3),
            F(11, 3),
            F(12, 3),
            F(13, 3),
        ],
        [P.OldTourianBackgroundRailingsFlashYellow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianRailings)),
        ],
        [P.OldTourianBackgroundRailingsFlashYellowCycle] =
        [
            F(0, 16),
            F(1, 1),
            F(2, 1),
            F(3, 2),
            F(4, 1),
            F(5, 2),
            F(6, 1),
            F(7, 1),
            F(8, 1),
            F(9, 1),
            F(10, 32),
            F(11, 2),
            F(12, 1),
            F(13, 1),
            F(14, 1),
        ],
        [P.OldTourianBackgroundPanelsFlashYellow] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianPanels)),
        ],
        [P.OldTourianBackgroundPanelsFlashYellowCycle] =
        [
            F(0, 16),
            F(1, 1),
            F(2, 1),
            F(3, 2),
            F(4, 1),
            F(5, 2),
            F(6, 1),
            F(7, 1),
            F(8, 1),
            F(9, 1),
            F(10, 32),
            F(11, 2),
            F(12, 1),
            F(13, 1),
            F(14, 1),
        ],
        [P.UpperCrateriaRedFlashing] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.UpperCrateriaLevel)),
        ],
        [P.UpperCrateriaRedFlashingCycle] =
        [
            F(0, 8),
            F(1, 7),
            F(2, 6),
            F(3, 5),
            F(4, 4),
            F(5, 3),
            F(6, 2),
            F(7, 1),
            F(8, 2),
            F(9, 3),
            F(10, 4),
            F(11, 5),
            F(12, 6),
            F(13, 7),
        ],
        [P.CrateriaEscapeYellowLightning] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.CrateriaEscapeLightning)),
        ],
        [P.CrateriaEscapeYellowLightningCycle] =
        [
            F(0, 49),
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 17),
            F(6, 1),
            F(7, 24),
            F(8, 1),
            F(9, 1),
            F(10, 1),
        ],
        [P.ParlorEscapeSkyFlicker] =
        [
            E(new PaletteFxCue.SetColours(PaletteFxColours.ParlorEscapeSky)),
        ],
        [P.ParlorEscapeSkyFlickerCycle] =
        [
            F(0, 49),
            F(1, 1),
            F(2, 1),
            F(3, 1),
            F(4, 1),
            F(5, 17),
            F(6, 1),
            F(7, 24),
            F(8, 1),
            F(9, 1),
            F(10, 1),
        ],
    });

    /// <summary>Shows frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<int, PaletteFxCue> F(int frame, int duration) => new AnimationBeat<int, PaletteFxCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/> for the running effect.</summary>
    private static AnimationBeat<int, PaletteFxCue> E(PaletteFxCue cue) => new AnimationBeat<int, PaletteFxCue>.Event(cue);
}
