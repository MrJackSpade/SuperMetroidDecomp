using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Fx.PaletteFx.PaletteFxProgram;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour effects' programs (#1298, #1554, #1598): each effect's named steps, read by the effect that runs them.
/// A show step shows one of its program's own frames for its duration, the frame named by its place among the frames the
/// program shows (a <see cref="PaletteFxFrame"/> of that program); a <see cref="PaletteFxCue"/> names the colours, a
/// condition, a heat phase or a sound, and a counted repeat counts down the running effect's own repeats.
/// </summary>
public static class PaletteFxAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PaletteFxProgram, int, PaletteFxCue> Programs { get; } = new(
        new Dictionary<PaletteFxProgram, AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>[]>
    {
        [P.FadeInSuperMetroidTitleLogo] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TitleLogo)),
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Stop,
        ],
        [P.FadeInNintendoBootLogo] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NintendoBootLogo)),
            FallInto(P.CommonFadeInLogo),
        ],
        [P.CommonFadeInLogo] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Stop,
        ],
        [P.FadeInNintendoCopyright] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NintendoCopyright)),
            GoTo(P.CommonFadeInLogo),
        ],
        [P.TitleScreenBabyMetroidTubeLight0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.BabyMetroidTubeLight)),
            FallInto(P.TitleScreenBabyMetroidTubeLight1),
        ],
        [P.TitleScreenBabyMetroidTubeLight1] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            GoTo(P.TitleScreenBabyMetroidTubeLight1),
        ],
        [P.TitleScreenFlickeringDisplays0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TitleScreenDisplays)),
            FallInto(P.TitleScreenFlickeringDisplays1),
        ],
        [P.TitleScreenFlickeringDisplays1] =
        [
            Show(0, 1),
            Show(1, 1),
            GoTo(P.TitleScreenFlickeringDisplays1),
        ],
        [P.CutsceneGunshipEngineFlicker0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CutsceneGunshipEngine)),
            FallInto(P.CutsceneGunshipEngineFlicker1),
        ],
        [P.CutsceneGunshipEngineFlicker1] =
        [
            Show(0, 1),
            Show(1, 1),
            GoTo(P.CutsceneGunshipEngineFlicker1),
        ],
        [P.CutsceneCeresNavigationLightSprite] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CeresNavigationLightsSprite)),
            FallInto(P.CommonCeresNavigationLights),
        ],
        [P.CommonCeresNavigationLights] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            Show(3, 4),
            Show(4, 4),
            Show(5, 4),
            Show(6, 4),
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
            Show(11, 4),
            Show(12, 4),
            Show(13, 4),
            GoTo(P.CommonCeresNavigationLights),
        ],
        [P.CutsceneCeresNavigationLightsBG] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CeresNavigationLightsBackground)),
            GoTo(P.CommonCeresNavigationLights),
        ],
        [P.FadeInPlanetZebesText] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PlanetZebesText)),
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Stop,
        ],
        [P.FadeOutPlanetZebesText] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PlanetZebesText)),
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Stop,
        ],
        [P.OldMotherBrainFightBGLights0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldMotherBrainBackgroundLights)),
            FallInto(P.OldMotherBrainFightBGLights1),
        ],
        [P.OldMotherBrainFightBGLights1] =
        [
            Show(0, 6),
            Show(1, 6),
            Show(2, 6),
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Show(6, 6),
            Show(7, 6),
            Show(8, 6),
            Show(9, 6),
            Show(10, 6),
            Show(11, 6),
            Show(12, 6),
            Show(13, 6),
            GoTo(P.OldMotherBrainFightBGLights1),
        ],
        [P.GunshipGlow0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.GunshipGlow)),
            FallInto(P.GunshipGlow1),
        ],
        [P.GunshipGlow1] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            GoTo(P.GunshipGlow1),
        ],
        [P.FadeOutZoomedOutExplodingZebes] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZoomedOutExplodingZebes)),
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Stop,
        ],
        [P.WidePartOfZebesExplosionForeground] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.WideZebesExplosionForeground)),
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            Show(3, 60),
            Show(4, 6),
            Show(5, 6),
            Show(6, 6),
            Show(7, 6),
            Show(8, 6),
            Show(9, 6),
            Show(10, 6),
            Show(11, 6),
            Show(12, 6),
            Show(13, 6),
            Show(14, 6),
            Show(15, 6),
            Stop,
        ],
        [P.ZebesExplosionFinale] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZebesExplosionFinale)),
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(10, 2),
            Show(11, 2),
            Show(12, 2),
            Show(13, 2),
            Show(14, 2),
            Show(15, 9),
            Show(16, 9),
            Show(17, 9),
            Show(18, 9),
            Show(19, 9),
            Show(20, 9),
            Show(21, 9),
            Show(22, 9),
            Show(23, 9),
            Show(24, 9),
            Show(25, 9),
            Show(26, 9),
            Show(27, 9),
            Show(28, 9),
            Show(29, 9),
            Show(30, 9),
            Show(31, 9),
            Show(32, 9),
            Show(33, 9),
            Show(34, 9),
            Show(35, 9),
            Show(36, 9),
            Show(37, 9),
            Show(38, 9),
            Show(39, 9),
            Show(40, 9),
            Show(41, 9),
            Show(42, 9),
            Show(43, 9),
            Show(44, 9),
            Stop,
        ],
        [P.WidePartOfZebesExplosionBackground] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.WideZebesExplosionBackground)),
            GoTo(P.CommonZebesExplosion),
        ],
        [P.WhiteOutSpaceDuringZebesExplosion] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.SpaceDuringZebesExplosion)),
            FallInto(P.CommonZebesExplosion),
        ],
        [P.CommonZebesExplosion] =
        [
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
            Show(3, 14),
            Show(4, 14),
            Show(5, 14),
            Show(6, 14),
            Show(7, 14),
            Show(8, 14),
            Show(9, 14),
            Show(10, 14),
            Show(11, 14),
            Show(12, 14),
            Show(13, 14),
            Show(14, 14),
            Stop,
        ],
        [P.ZebesExplosionPlanetAfterGlow0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZebesPlanetAfterglow)),
            FallInto(P.ZebesExplosionPlanetAfterGlow1),
        ],
        [P.ZebesExplosionPlanetAfterGlow1] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(3, 16),
            Show(4, 16),
            Show(5, 16),
            GoTo(P.ZebesExplosionPlanetAfterGlow1),
        ],
        [P.ExplodingZebesLava0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesLava)),
            FallInto(P.ExplodingZebesLava1),
        ],
        [P.ExplodingZebesLava1] =
        [
            Show(0, 9),
            Show(1, 8),
            Show(2, 7),
            Show(3, 6),
            Show(4, 5),
            Show(5, 5),
            Show(6, 6),
            Show(7, 7),
            Show(8, 8),
            Show(9, 9),
            GoTo(P.ExplodingZebesLava1),
        ],
        [P.FadeOutExplodingZebesCrust] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesCrust)),
            Show(0, 20),
            Show(1, 20),
            Show(2, 20),
            Show(3, 20),
            Show(4, 20),
            Show(5, 20),
            Show(6, 20),
            Show(7, 20),
            Stop,
        ],
        [P.FadeOutExplodingZebesGreyClouds] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesGreyClouds)),
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
            Show(3, 14),
            Show(4, 14),
            Show(5, 14),
            Show(6, 14),
            Show(7, 14),
            Stop,
        ],
        [P.GunshipEmergingFromZebesExplosion] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.GunshipEmergingFromExplosion)),
            Show(0, 24),
            Show(1, 24),
            Show(2, 24),
            Show(3, 24),
            Show(4, 24),
            Show(5, 24),
            Show(6, 24),
            Show(7, 24),
            Show(8, 24),
            Show(9, 24),
            Show(10, 24),
            Show(11, 24),
            Show(12, 24),
            Show(13, 24),
            Show(14, 24),
            Show(15, 24),
            Stop,
        ],
        [P.HyperBeam0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HyperBeam)),
            FallInto(P.HyperBeam1),
        ],
        [P.HyperBeam1] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            GoTo(P.HyperBeam1),
        ],
        [P.UnusedCinematicFade] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.UnusedCinematicFade)),
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(10, 2),
            Stop,
        ],
        [P.SamusLoadingPowerSuit0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
            SetCounter(36),
            FallInto(P.SamusLoadingPowerSuit1),
        ],
        [P.SamusLoadingPowerSuit1] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingPowerSuit2),
        ],
        [P.SamusLoadingPowerSuit2] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingPowerSuit3),
        ],
        [P.SamusLoadingPowerSuit3] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingPowerSuit4),
        ],
        [P.SamusLoadingPowerSuit4] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit4),
            Show(2, 1),
            Stop,
        ],
        [P.SamusLoadingVairaSuit0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
            SetCounter(36),
            FallInto(P.SamusLoadingVairaSuit1),
        ],
        [P.SamusLoadingVairaSuit1] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingVairaSuit2),
        ],
        [P.SamusLoadingVairaSuit2] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingVairaSuit3),
        ],
        [P.SamusLoadingVairaSuit3] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingVairaSuit4),
        ],
        [P.SamusLoadingVairaSuit4] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit4),
            Show(2, 1),
            Stop,
        ],
        [P.SamusLoadingGravitySuit0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.SamusLoadingSuit)),
            SetCounter(36),
            FallInto(P.SamusLoadingGravitySuit1),
        ],
        [P.SamusLoadingGravitySuit1] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingGravitySuit2),
        ],
        [P.SamusLoadingGravitySuit2] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingGravitySuit3),
        ],
        [P.SamusLoadingGravitySuit3] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingGravitySuit4),
        ],
        [P.SamusLoadingGravitySuit4] =
        [
            Show(0, 3),
            Show(1, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit4),
            Show(2, 1),
            Stop,
        ],
        [P.PostCreditsSuperMetroidIconGlare] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PostCreditsSuperMetroidIcon)),
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            Show(11, 1),
            Show(12, 1),
            Show(13, 1),
            Stop,
        ],
        [P.Nothing] =
        [
            Stop,
        ],
        [P.GreyOutTourianStatueDraygon] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.DraygonStatue)),
            GoTo(P.CommonGreyOutTourianStatue),
        ],
        [P.GreyOutTourianStatueKraid] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.KraidStatue)),
            GoTo(P.CommonGreyOutTourianStatue),
        ],
        [P.GreyOutTourianStatueRidley] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.RidleyStatue)),
            GoTo(P.CommonGreyOutTourianStatue),
        ],
        [P.GreyOutTourianStatuePhantoon] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PhantoonStatue)),
            FallInto(P.CommonGreyOutTourianStatue),
        ],
        [P.CommonGreyOutTourianStatue] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 8),
            Stop,
        ],
        [P.BombTorizoBelly0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TorizoBelly)),
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenTorizoDies.Instance)),
            FallInto(P.BombTorizoBelly1),
        ],
        [P.BombTorizoBelly1] =
        [
            Show(0, 10),
            Show(1, 8),
            Show(2, 8),
            Show(3, 10),
            Show(4, 8),
            Show(5, 8),
            GoTo(P.BombTorizoBelly1),
        ],
        [P.GoldenTorizoBelly0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TorizoBelly)),
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenTorizoDies.Instance)),
            FallInto(P.GoldenTorizoBelly1),
        ],
        [P.GoldenTorizoBelly1] =
        [
            Show(0, 10),
            Show(1, 8),
            Show(2, 8),
            Show(3, 10),
            Show(4, 8),
            Show(5, 8),
            GoTo(P.GoldenTorizoBelly1),
        ],
        [P.SamusInHeatPowerSuit] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatPowerSuitPulse),
        ],
        [P.SamusInHeatPowerSuitPulse] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 3),
            GoTo(P.SamusInHeatPowerSuitPulse),
        ],
        [P.SamusInHeatVariaSuit] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatVariaSuitPulse),
        ],
        [P.SamusInHeatVariaSuitPulse] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 16),
            GoTo(P.SamusInHeatVariaSuitPulse),
        ],
        [P.SamusInHeatGravitySuit] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatGravitySuitPulse),
        ],
        [P.SamusInHeatGravitySuitPulse] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 16),
            GoTo(P.SamusInHeatGravitySuitPulse),
        ],
        [P.WreckedShipGreenLights] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.WreckedShipGreenLights)),
            FallInto(P.WreckedShipGreenLightsCycle),
        ],
        [P.WreckedShipGreenLightsCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            GoTo(P.WreckedShipGreenLightsCycle),
        ],
        [P.CrateriaLightning] =
        [
            Cue(new PaletteFxCue.Watch(new ColourEffectCondition.RestartsWhileSamusIsHigh(CrateriaLightningDefinitions.SamusHighOnSurfaceHeight, P.CrateriaLightningCycle))),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.LandingSiteLightning)),
            FallInto(P.CrateriaLightningCycle),
        ],
        [P.CrateriaLightningCycle] =
        [
            Show(0, 240),
            SetCounter(2),
            FallInto(P.CrateriaLightningRepeatedFlash),
        ],
        [P.CrateriaLightningRepeatedFlash] =
        [
            Show(0, 2),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 2),
            RepeatWhileCounting(P.CrateriaLightningRepeatedFlash),
            Show(7, 240),
            SetCounter(1),
            FallInto(P.CrateriaLightningClosingFlash),
        ],
        [P.CrateriaLightningClosingFlash] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 2),
            RepeatWhileCounting(P.CrateriaLightningClosingFlash),
            GoTo(P.CrateriaLightningCycle),
        ],
        [P.UnusedDarkLightning] =
        [
            Cue(new PaletteFxCue.Watch(new ColourEffectCondition.RestartsWhileSamusIsHigh(CrateriaLightningDefinitions.SamusHighOnSurfaceHeight, P.UnusedDarkLightningCycle))),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.UnusedDarkLightning)),
            FallInto(P.UnusedDarkLightningCycle),
        ],
        [P.UnusedDarkLightningCycle] =
        [
            Show(0, 240),
            SetCounter(2),
            FallInto(P.UnusedDarkLightningRepeatedFlash),
        ],
        [P.UnusedDarkLightningRepeatedFlash] =
        [
            Show(0, 2),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 2),
            RepeatWhileCounting(P.UnusedDarkLightningRepeatedFlash),
            Show(7, 240),
            Show(8, 240),
            SetCounter(1),
            FallInto(P.UnusedDarkLightningClosingFlash),
        ],
        [P.UnusedDarkLightningClosingFlash] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 2),
            RepeatWhileCounting(P.UnusedDarkLightningClosingFlash),
            GoTo(P.UnusedDarkLightningCycle),
        ],
        [P.BrinstarBlueBackgroundSpores] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarBlueSpores)),
            FallInto(P.BrinstarBlueBackgroundSporesCycle),
        ],
        [P.BrinstarBlueBackgroundSporesCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            GoTo(P.BrinstarBlueBackgroundSporesCycle),
        ],
        [P.SporeSpawnBlueBackgroundSpores] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.EndsWhenAreaMiniBossDies.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarBlueSpores)),
            FallInto(P.SporeSpawnBlueBackgroundSporesCycle),
        ],
        [P.SporeSpawnBlueBackgroundSporesCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            GoTo(P.SporeSpawnBlueBackgroundSporesCycle),
        ],
        [P.BrinstarRedBackgroundGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarRedGlow)),
            FallInto(P.BrinstarRedBackgroundGlowCycle),
        ],
        [P.BrinstarRedBackgroundGlowCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 10),
            Show(12, 10),
            Show(13, 10),
            GoTo(P.BrinstarRedBackgroundGlowCycle),
        ],
        [P.BeaconFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.Beacon)),
            FallInto(P.BeaconFlashingCycle),
        ],
        [P.BeaconFlashingCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Cue(new PaletteFxCue.QueueSound(Sound: EnemySound.Beacon)),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            GoTo(P.BeaconFlashingCycle),
        ],
        [P.NorfairForegroundAndHeatPhase] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairHeatForeground)),
            FallInto(P.NorfairForegroundAndHeatPhaseCycle),
        ],
        [P.NorfairForegroundAndHeatPhaseCycle] =
        [
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(0))),
            Show(0, 16),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(1))),
            Show(1, 4),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(2))),
            Show(2, 4),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(3))),
            Show(3, 5),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(4))),
            Show(4, 6),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(5))),
            Show(5, 7),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(6))),
            Show(6, 8),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(7))),
            Show(7, 8),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(8))),
            Show(8, 8),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(9))),
            Show(9, 8),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(10))),
            Show(10, 7),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(11))),
            Show(11, 6),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(12))),
            Show(12, 5),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(13))),
            Show(13, 4),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(14))),
            Show(14, 4),
            Cue(new PaletteFxCue.PublishHeatPhase(new HeatPhase(15))),
            Show(15, 16),
            GoTo(P.NorfairForegroundAndHeatPhaseCycle),
        ],
        [P.NorfairGreenForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairGreenForeground)),
            FallInto(P.NorfairGreenForegroundHeatGlowCycle),
        ],
        [P.NorfairGreenForegroundHeatGlowCycle] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 16),
            GoTo(P.NorfairGreenForegroundHeatGlowCycle),
        ],
        [P.NorfairPurpleForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairPurpleForeground)),
            FallInto(P.NorfairPurpleForegroundHeatGlowCycle),
        ],
        [P.NorfairPurpleForegroundHeatGlowCycle] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 16),
            GoTo(P.NorfairPurpleForegroundHeatGlowCycle),
        ],
        [P.NorfairDarkForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairDarkForeground)),
            FallInto(P.NorfairDarkForegroundHeatGlowCycle),
        ],
        [P.NorfairDarkForegroundHeatGlowCycle] =
        [
            Show(0, 16),
            Show(1, 4),
            Show(2, 4),
            Show(3, 5),
            Show(4, 6),
            Show(5, 7),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Show(9, 8),
            Show(10, 7),
            Show(11, 6),
            Show(12, 5),
            Show(13, 4),
            Show(14, 4),
            Show(15, 16),
            GoTo(P.NorfairDarkForegroundHeatGlowCycle),
        ],
        [P.MaridiaSandPits] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandPits)),
            FallInto(P.MaridiaSandPitsCycle),
        ],
        [P.MaridiaSandPitsCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            GoTo(P.MaridiaSandPitsCycle),
        ],
        [P.MaridiaSandFalls] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandFalls)),
            FallInto(P.MaridiaSandFallsCycle),
        ],
        [P.MaridiaSandFallsCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            GoTo(P.MaridiaSandFallsCycle),
        ],
        [P.MaridiaBackgroundWaterfalls] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaBackgroundWaterfalls)),
            FallInto(P.MaridiaBackgroundWaterfallsCycle),
        ],
        [P.MaridiaBackgroundWaterfallsCycle] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            GoTo(P.MaridiaBackgroundWaterfallsCycle),
        ],
        [P.UnusedTourianGlowClone] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianArkanoidBlocksAndOrbs)),
            GoTo(P.CommonTourianGlowCycle),
        ],
        [P.TourianGlowingArkanoidBlocksAndRedOrbs] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianArkanoidBlocksAndOrbs)),
            FallInto(P.CommonTourianGlowCycle),
        ],
        [P.CommonTourianGlowCycle] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            GoTo(P.CommonTourianGlowCycle),
        ],
        [P.TourianShutterRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianShutters)),
            FallInto(P.TourianShutterRedFlashingCycle),
        ],
        [P.TourianShutterRedFlashingCycle] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(10, 2),
            Show(11, 2),
            Show(12, 2),
            Show(13, 2),
            GoTo(P.TourianShutterRedFlashingCycle),
        ],
        [P.TourianBackgroundRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianBackground)),
            FallInto(P.TourianBackgroundRedFlashingCycle),
        ],
        [P.TourianBackgroundRedFlashingCycle] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            Show(3, 4),
            Show(4, 4),
            Show(5, 4),
            Show(6, 4),
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
            Show(11, 4),
            Show(12, 4),
            Show(13, 4),
            GoTo(P.TourianBackgroundRedFlashingCycle),
        ],
        [P.TourianGeneralLevelRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianLevel)),
            GoTo(P.CommonTourianEscapeRedFlashingCycle),
        ],
        [P.TourianRedFlashingArkanoidBlocksAndRedOrbs] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianArkanoidBlocksAndOrbs)),
            FallInto(P.CommonTourianEscapeRedFlashingCycle),
        ],
        [P.CommonTourianEscapeRedFlashingCycle] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 2),
            Show(3, 2),
            Show(4, 2),
            Show(5, 2),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(9, 2),
            Show(10, 2),
            Show(11, 2),
            Show(12, 2),
            Show(13, 2),
            GoTo(P.CommonTourianEscapeRedFlashingCycle),
        ],
        [P.OldTourianRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianLevel)),
            FallInto(P.OldTourianRedFlashingCycle),
        ],
        [P.OldTourianRedFlashingCycle] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
            Show(9, 3),
            Show(10, 3),
            Show(11, 3),
            Show(12, 3),
            Show(13, 3),
            GoTo(P.OldTourianRedFlashingCycle),
        ],
        [P.OldTourianBackgroundRailingsFlashYellow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianRailings)),
            FallInto(P.OldTourianBackgroundRailingsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundRailingsFlashYellowCycle] =
        [
            Show(0, 16),
            Show(1, 1),
            Show(2, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 32),
            Show(11, 2),
            Show(12, 1),
            Show(13, 1),
            Show(14, 1),
            GoTo(P.OldTourianBackgroundRailingsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundPanelsFlashYellow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianPanels)),
            FallInto(P.OldTourianBackgroundPanelsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundPanelsFlashYellowCycle] =
        [
            Show(0, 16),
            Show(1, 1),
            Show(2, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 32),
            Show(11, 2),
            Show(12, 1),
            Show(13, 1),
            Show(14, 1),
            GoTo(P.OldTourianBackgroundPanelsFlashYellowCycle),
        ],
        [P.UpperCrateriaRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.UpperCrateriaLevel)),
            FallInto(P.UpperCrateriaRedFlashingCycle),
        ],
        [P.UpperCrateriaRedFlashingCycle] =
        [
            Show(0, 8),
            Show(1, 7),
            Show(2, 6),
            Show(3, 5),
            Show(4, 4),
            Show(5, 3),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(9, 3),
            Show(10, 4),
            Show(11, 5),
            Show(12, 6),
            Show(13, 7),
            GoTo(P.UpperCrateriaRedFlashingCycle),
        ],
        [P.CrateriaEscapeYellowLightning] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CrateriaEscapeLightning)),
            FallInto(P.CrateriaEscapeYellowLightningCycle),
        ],
        [P.CrateriaEscapeYellowLightningCycle] =
        [
            Show(0, 49),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 17),
            Show(6, 1),
            Show(7, 24),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            GoTo(P.CrateriaEscapeYellowLightningCycle),
        ],
        [P.ParlorEscapeSkyFlicker] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ParlorEscapeSky)),
            FallInto(P.ParlorEscapeSkyFlickerCycle),
        ],
        [P.ParlorEscapeSkyFlickerCycle] =
        [
            Show(0, 49),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 17),
            Show(6, 1),
            Show(7, 24),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            GoTo(P.ParlorEscapeSkyFlickerCycle),
        ],
    });

    /// <summary>Shows frame <paramref name="frame"/> of the program's own frames for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> Show(int frame, int duration) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> Cue(PaletteFxCue cue) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.Publish(cue);

    /// <summary>Sets the loop counter.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> SetCounter(int count) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.SetCounter(count);

    /// <summary>Repeats <paramref name="program"/> until the loop counter runs out.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> RepeatWhileCounting(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> GoTo(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> FallInto(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.FallInto(program);

    /// <summary>Deletes the object.</summary>
    private static AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue> Stop => AnimationProgramStep<PaletteFxProgram, int, PaletteFxCue>.Stop;
}
