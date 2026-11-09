using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Fx.PaletteFx.PaletteFxProgram;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The colour effects' programs (#1298, #1554): each effect's named steps, read by the effect that runs them.
/// A timed colour record shows a <see cref="PaletteFxFrame"/> for its duration, a <see cref="PaletteFxCue"/> names the
/// colours, a condition, a heat phase or a sound, and a counted repeat counts down the running effect's own repeats.
/// </summary>
public static class PaletteFxAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> Programs { get; } = new(
        new Dictionary<PaletteFxProgram, AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>[]>
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
            Show(8, 3),
            Show(9, 3),
            Show(10, 3),
            Show(11, 3),
            Show(12, 3),
            Show(13, 3),
            Show(14, 3),
            Show(15, 3),
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
            Show(16, 10),
            Show(17, 10),
            Show(18, 10),
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(22, 10),
            Show(23, 10),
            GoTo(P.TitleScreenBabyMetroidTubeLight1),
        ],
        [P.TitleScreenFlickeringDisplays0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TitleScreenDisplays)),
            FallInto(P.TitleScreenFlickeringDisplays1),
        ],
        [P.TitleScreenFlickeringDisplays1] =
        [
            Show(24, 1),
            Show(25, 1),
            GoTo(P.TitleScreenFlickeringDisplays1),
        ],
        [P.CutsceneGunshipEngineFlicker0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CutsceneGunshipEngine)),
            FallInto(P.CutsceneGunshipEngineFlicker1),
        ],
        [P.CutsceneGunshipEngineFlicker1] =
        [
            Show(26, 1),
            Show(27, 1),
            GoTo(P.CutsceneGunshipEngineFlicker1),
        ],
        [P.CutsceneCeresNavigationLightSprite] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CeresNavigationLightsSprite)),
            FallInto(P.CommonCeresNavigationLights),
        ],
        [P.CommonCeresNavigationLights] =
        [
            Show(28, 4),
            Show(29, 4),
            Show(30, 4),
            Show(31, 4),
            Show(32, 4),
            Show(33, 4),
            Show(34, 4),
            Show(35, 4),
            Show(36, 4),
            Show(37, 4),
            Show(38, 4),
            Show(39, 4),
            Show(40, 4),
            Show(41, 4),
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
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            Show(46, 3),
            Show(47, 3),
            Show(48, 3),
            Show(49, 3),
            Stop,
        ],
        [P.FadeOutPlanetZebesText] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PlanetZebesText)),
            Show(50, 3),
            Show(51, 3),
            Show(52, 3),
            Show(53, 3),
            Show(54, 3),
            Show(55, 3),
            Show(56, 3),
            Show(57, 3),
            Stop,
        ],
        [P.OldMotherBrainFightBGLights0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldMotherBrainBackgroundLights)),
            FallInto(P.OldMotherBrainFightBGLights1),
        ],
        [P.OldMotherBrainFightBGLights1] =
        [
            Show(58, 6),
            Show(59, 6),
            Show(60, 6),
            Show(61, 6),
            Show(62, 6),
            Show(63, 6),
            Show(64, 6),
            Show(65, 6),
            Show(66, 6),
            Show(67, 6),
            Show(68, 6),
            Show(69, 6),
            Show(70, 6),
            Show(71, 6),
            GoTo(P.OldMotherBrainFightBGLights1),
        ],
        [P.GunshipGlow0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.GunshipGlow)),
            FallInto(P.GunshipGlow1),
        ],
        [P.GunshipGlow1] =
        [
            Show(72, 5),
            Show(73, 5),
            Show(74, 5),
            Show(75, 5),
            Show(76, 5),
            Show(77, 5),
            Show(78, 5),
            Show(79, 5),
            Show(80, 5),
            Show(81, 5),
            Show(82, 5),
            Show(83, 5),
            Show(84, 5),
            Show(85, 5),
            GoTo(P.GunshipGlow1),
        ],
        [P.FadeOutZoomedOutExplodingZebes] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZoomedOutExplodingZebes)),
            Show(86, 8),
            Show(87, 8),
            Show(88, 8),
            Show(89, 8),
            Show(90, 8),
            Show(91, 8),
            Show(92, 8),
            Stop,
        ],
        [P.WidePartOfZebesExplosionForeground] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.WideZebesExplosionForeground)),
            Show(93, 4),
            Show(94, 4),
            Show(95, 4),
            Show(96, 60),
            Show(97, 6),
            Show(98, 6),
            Show(99, 6),
            Show(100, 6),
            Show(101, 6),
            Show(102, 6),
            Show(103, 6),
            Show(104, 6),
            Show(105, 6),
            Show(106, 6),
            Show(107, 6),
            Show(108, 6),
            Stop,
        ],
        [P.ZebesExplosionFinale] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZebesExplosionFinale)),
            Show(109, 2),
            Show(110, 2),
            Show(111, 2),
            Show(112, 2),
            Show(113, 2),
            Show(114, 2),
            Show(115, 2),
            Show(116, 2),
            Show(117, 2),
            Show(118, 2),
            Show(119, 2),
            Show(120, 2),
            Show(121, 2),
            Show(122, 2),
            Show(123, 2),
            Show(124, 9),
            Show(125, 9),
            Show(126, 9),
            Show(127, 9),
            Show(128, 9),
            Show(129, 9),
            Show(130, 9),
            Show(131, 9),
            Show(132, 9),
            Show(133, 9),
            Show(134, 9),
            Show(135, 9),
            Show(136, 9),
            Show(137, 9),
            Show(138, 9),
            Show(139, 9),
            Show(140, 9),
            Show(141, 9),
            Show(142, 9),
            Show(143, 9),
            Show(144, 9),
            Show(145, 9),
            Show(146, 9),
            Show(147, 9),
            Show(148, 9),
            Show(149, 9),
            Show(150, 9),
            Show(151, 9),
            Show(152, 9),
            Show(153, 9),
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
            Show(154, 14),
            Show(155, 14),
            Show(156, 14),
            Show(157, 14),
            Show(158, 14),
            Show(159, 14),
            Show(160, 14),
            Show(161, 14),
            Show(162, 14),
            Show(163, 14),
            Show(164, 14),
            Show(165, 14),
            Show(166, 14),
            Show(167, 14),
            Show(168, 14),
            Stop,
        ],
        [P.ZebesExplosionPlanetAfterGlow0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ZebesPlanetAfterglow)),
            FallInto(P.ZebesExplosionPlanetAfterGlow1),
        ],
        [P.ZebesExplosionPlanetAfterGlow1] =
        [
            Show(169, 16),
            Show(170, 16),
            Show(171, 16),
            Show(172, 16),
            Show(173, 16),
            Show(174, 16),
            GoTo(P.ZebesExplosionPlanetAfterGlow1),
        ],
        [P.ExplodingZebesLava0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesLava)),
            FallInto(P.ExplodingZebesLava1),
        ],
        [P.ExplodingZebesLava1] =
        [
            Show(175, 9),
            Show(176, 8),
            Show(177, 7),
            Show(178, 6),
            Show(179, 5),
            Show(180, 5),
            Show(181, 6),
            Show(182, 7),
            Show(183, 8),
            Show(184, 9),
            GoTo(P.ExplodingZebesLava1),
        ],
        [P.FadeOutExplodingZebesCrust] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesCrust)),
            Show(185, 20),
            Show(186, 20),
            Show(187, 20),
            Show(188, 20),
            Show(189, 20),
            Show(190, 20),
            Show(191, 20),
            Show(192, 20),
            Stop,
        ],
        [P.FadeOutExplodingZebesGreyClouds] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ExplodingZebesGreyClouds)),
            Show(193, 14),
            Show(194, 14),
            Show(195, 14),
            Show(196, 14),
            Show(197, 14),
            Show(198, 14),
            Show(199, 14),
            Show(200, 14),
            Stop,
        ],
        [P.GunshipEmergingFromZebesExplosion] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.GunshipEmergingFromExplosion)),
            Show(201, 24),
            Show(202, 24),
            Show(203, 24),
            Show(204, 24),
            Show(205, 24),
            Show(206, 24),
            Show(207, 24),
            Show(208, 24),
            Show(209, 24),
            Show(210, 24),
            Show(211, 24),
            Show(212, 24),
            Show(213, 24),
            Show(214, 24),
            Show(215, 24),
            Show(216, 24),
            Stop,
        ],
        [P.HyperBeam0] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HyperBeam)),
            FallInto(P.HyperBeam1),
        ],
        [P.HyperBeam1] =
        [
            Show(217, 2),
            Show(218, 2),
            Show(219, 2),
            Show(220, 2),
            Show(221, 2),
            Show(222, 2),
            Show(223, 2),
            Show(224, 2),
            Show(225, 2),
            Show(226, 2),
            GoTo(P.HyperBeam1),
        ],
        [P.UnusedCinematicFade] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.UnusedCinematicFade)),
            Show(227, 2),
            Show(228, 2),
            Show(229, 2),
            Show(230, 2),
            Show(231, 2),
            Show(232, 2),
            Show(233, 2),
            Show(234, 2),
            Show(235, 2),
            Show(236, 2),
            Show(237, 2),
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
            Show(238, 3),
            Show(239, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingPowerSuit2),
        ],
        [P.SamusLoadingPowerSuit2] =
        [
            Show(240, 3),
            Show(241, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingPowerSuit3),
        ],
        [P.SamusLoadingPowerSuit3] =
        [
            Show(242, 3),
            Show(243, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingPowerSuit4),
        ],
        [P.SamusLoadingPowerSuit4] =
        [
            Show(244, 3),
            Show(245, 3),
            RepeatWhileCounting(P.SamusLoadingPowerSuit4),
            Show(246, 1),
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
            Show(247, 3),
            Show(248, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingVairaSuit2),
        ],
        [P.SamusLoadingVairaSuit2] =
        [
            Show(249, 3),
            Show(250, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingVairaSuit3),
        ],
        [P.SamusLoadingVairaSuit3] =
        [
            Show(251, 3),
            Show(252, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingVairaSuit4),
        ],
        [P.SamusLoadingVairaSuit4] =
        [
            Show(253, 3),
            Show(254, 3),
            RepeatWhileCounting(P.SamusLoadingVairaSuit4),
            Show(255, 1),
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
            Show(256, 3),
            Show(257, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit1),
            SetCounter(3),
            FallInto(P.SamusLoadingGravitySuit2),
        ],
        [P.SamusLoadingGravitySuit2] =
        [
            Show(258, 3),
            Show(259, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit2),
            SetCounter(3),
            FallInto(P.SamusLoadingGravitySuit3),
        ],
        [P.SamusLoadingGravitySuit3] =
        [
            Show(260, 3),
            Show(261, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit3),
            SetCounter(2),
            FallInto(P.SamusLoadingGravitySuit4),
        ],
        [P.SamusLoadingGravitySuit4] =
        [
            Show(262, 3),
            Show(263, 3),
            RepeatWhileCounting(P.SamusLoadingGravitySuit4),
            Show(264, 1),
            Stop,
        ],
        [P.PostCreditsSuperMetroidIconGlare] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.PostCreditsSuperMetroidIcon)),
            Show(265, 1),
            Show(266, 1),
            Show(267, 1),
            Show(268, 1),
            Show(269, 1),
            Show(270, 1),
            Show(271, 1),
            Show(272, 1),
            Show(273, 1),
            Show(274, 1),
            Show(275, 1),
            Show(276, 1),
            Show(277, 1),
            Show(278, 1),
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
            Show(279, 8),
            Show(280, 8),
            Show(281, 8),
            Show(282, 8),
            Show(283, 8),
            Show(284, 8),
            Show(285, 8),
            Show(286, 8),
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
            Show(287, 10),
            Show(288, 8),
            Show(289, 8),
            Show(290, 10),
            Show(291, 8),
            Show(292, 8),
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
            Show(293, 10),
            Show(294, 8),
            Show(295, 8),
            Show(296, 10),
            Show(297, 8),
            Show(298, 8),
            GoTo(P.GoldenTorizoBelly1),
        ],
        [P.SamusInHeatPowerSuitStep0] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatPowerSuitStep1),
        ],
        [P.SamusInHeatPowerSuitStep1] =
        [
            Show(299, 16),
            FallInto(P.SamusInHeatPowerSuitStep2),
        ],
        [P.SamusInHeatPowerSuitStep2] =
        [
            Show(300, 4),
            FallInto(P.SamusInHeatPowerSuitStep3),
        ],
        [P.SamusInHeatPowerSuitStep3] =
        [
            Show(301, 4),
            FallInto(P.SamusInHeatPowerSuitStep4),
        ],
        [P.SamusInHeatPowerSuitStep4] =
        [
            Show(302, 5),
            FallInto(P.SamusInHeatPowerSuitStep5),
        ],
        [P.SamusInHeatPowerSuitStep5] =
        [
            Show(303, 6),
            FallInto(P.SamusInHeatPowerSuitStep6),
        ],
        [P.SamusInHeatPowerSuitStep6] =
        [
            Show(304, 7),
            FallInto(P.SamusInHeatPowerSuitStep7),
        ],
        [P.SamusInHeatPowerSuitStep7] =
        [
            Show(305, 8),
            FallInto(P.SamusInHeatPowerSuitStep8),
        ],
        [P.SamusInHeatPowerSuitStep8] =
        [
            Show(306, 8),
            FallInto(P.SamusInHeatPowerSuitStep9),
        ],
        [P.SamusInHeatPowerSuitStep9] =
        [
            Show(307, 8),
            FallInto(P.SamusInHeatPowerSuitStep10),
        ],
        [P.SamusInHeatPowerSuitStep10] =
        [
            Show(308, 8),
            FallInto(P.SamusInHeatPowerSuitStep11),
        ],
        [P.SamusInHeatPowerSuitStep11] =
        [
            Show(309, 7),
            FallInto(P.SamusInHeatPowerSuitStep12),
        ],
        [P.SamusInHeatPowerSuitStep12] =
        [
            Show(310, 6),
            FallInto(P.SamusInHeatPowerSuitStep13),
        ],
        [P.SamusInHeatPowerSuitStep13] =
        [
            Show(311, 5),
            FallInto(P.SamusInHeatPowerSuitStep14),
        ],
        [P.SamusInHeatPowerSuitStep14] =
        [
            Show(312, 4),
            FallInto(P.SamusInHeatPowerSuitStep15),
        ],
        [P.SamusInHeatPowerSuitStep15] =
        [
            Show(313, 4),
            FallInto(P.SamusInHeatPowerSuit10),
        ],
        [P.SamusInHeatPowerSuit10] =
        [
            Show(314, 3),
            GoTo(P.SamusInHeatPowerSuitStep1),
        ],
        [P.SamusInHeatVariaSuitStep0] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatVariaSuitStep2),
        ],
        [P.SamusInHeatVariaSuitStep2] =
        [
            Show(315, 16),
            FallInto(P.SamusInHeatVariaSuitStep3),
        ],
        [P.SamusInHeatVariaSuitStep3] =
        [
            Show(316, 4),
            FallInto(P.SamusInHeatVariaSuitStep4),
        ],
        [P.SamusInHeatVariaSuitStep4] =
        [
            Show(317, 4),
            FallInto(P.SamusInHeatVariaSuitStep5),
        ],
        [P.SamusInHeatVariaSuitStep5] =
        [
            Show(318, 5),
            FallInto(P.SamusInHeatVariaSuitStep6),
        ],
        [P.SamusInHeatVariaSuitStep6] =
        [
            Show(319, 6),
            FallInto(P.SamusInHeatVariaSuitStep7),
        ],
        [P.SamusInHeatVariaSuitStep7] =
        [
            Show(320, 7),
            FallInto(P.SamusInHeatVariaSuitStep8),
        ],
        [P.SamusInHeatVariaSuitStep8] =
        [
            Show(321, 8),
            FallInto(P.SamusInHeatVariaSuitStep9),
        ],
        [P.SamusInHeatVariaSuitStep9] =
        [
            Show(322, 8),
            FallInto(P.SamusInHeatVariaSuitStep10),
        ],
        [P.SamusInHeatVariaSuitStep10] =
        [
            Show(323, 8),
            FallInto(P.SamusInHeatVariaSuitStep11),
        ],
        [P.SamusInHeatVariaSuitStep11] =
        [
            Show(324, 8),
            FallInto(P.SamusInHeatVariaSuitStep12),
        ],
        [P.SamusInHeatVariaSuitStep12] =
        [
            Show(325, 7),
            FallInto(P.SamusInHeatVariaSuitStep13),
        ],
        [P.SamusInHeatVariaSuitStep13] =
        [
            Show(326, 6),
            FallInto(P.SamusInHeatVariaSuitStep14),
        ],
        [P.SamusInHeatVariaSuitStep14] =
        [
            Show(327, 5),
            FallInto(P.SamusInHeatVariaSuitStep15),
        ],
        [P.SamusInHeatVariaSuitStep15] =
        [
            Show(328, 4),
            FallInto(P.SamusInHeatVariaSuit10),
        ],
        [P.SamusInHeatVariaSuit10] =
        [
            Show(329, 4),
            FallInto(P.SamusInHeatVariaSuit11),
        ],
        [P.SamusInHeatVariaSuit11] =
        [
            Show(330, 16),
            GoTo(P.SamusInHeatVariaSuitStep2),
        ],
        [P.SamusInHeatGravitySuitStep0] =
        [
            Cue(new PaletteFxCue.Watch(ColourEffectCondition.BurnsSamusInHeat.Instance)),
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.HeatSuitGlow)),
            FallInto(P.SamusInHeatGravitySuitStep2),
        ],
        [P.SamusInHeatGravitySuitStep2] =
        [
            Show(331, 16),
            FallInto(P.SamusInHeatGravitySuitStep3),
        ],
        [P.SamusInHeatGravitySuitStep3] =
        [
            Show(332, 4),
            FallInto(P.SamusInHeatGravitySuitStep4),
        ],
        [P.SamusInHeatGravitySuitStep4] =
        [
            Show(333, 4),
            FallInto(P.SamusInHeatGravitySuitStep5),
        ],
        [P.SamusInHeatGravitySuitStep5] =
        [
            Show(334, 5),
            FallInto(P.SamusInHeatGravitySuitStep6),
        ],
        [P.SamusInHeatGravitySuitStep6] =
        [
            Show(335, 6),
            FallInto(P.SamusInHeatGravitySuitStep7),
        ],
        [P.SamusInHeatGravitySuitStep7] =
        [
            Show(336, 7),
            FallInto(P.SamusInHeatGravitySuitStep8),
        ],
        [P.SamusInHeatGravitySuitStep8] =
        [
            Show(337, 8),
            FallInto(P.SamusInHeatGravitySuitStep9),
        ],
        [P.SamusInHeatGravitySuitStep9] =
        [
            Show(338, 8),
            FallInto(P.SamusInHeatGravitySuitStep10),
        ],
        [P.SamusInHeatGravitySuitStep10] =
        [
            Show(339, 8),
            FallInto(P.SamusInHeatGravitySuitStep11),
        ],
        [P.SamusInHeatGravitySuitStep11] =
        [
            Show(340, 8),
            FallInto(P.SamusInHeatGravitySuitStep12),
        ],
        [P.SamusInHeatGravitySuitStep12] =
        [
            Show(341, 7),
            FallInto(P.SamusInHeatGravitySuitStep13),
        ],
        [P.SamusInHeatGravitySuitStep13] =
        [
            Show(342, 6),
            FallInto(P.SamusInHeatGravitySuitStep14),
        ],
        [P.SamusInHeatGravitySuitStep14] =
        [
            Show(343, 5),
            FallInto(P.SamusInHeatGravitySuitStep15),
        ],
        [P.SamusInHeatGravitySuitStep15] =
        [
            Show(344, 4),
            FallInto(P.SamusInHeatGravitySuit10),
        ],
        [P.SamusInHeatGravitySuit10] =
        [
            Show(345, 4),
            FallInto(P.SamusInHeatGravitySuit11),
        ],
        [P.SamusInHeatGravitySuit11] =
        [
            Show(346, 16),
            GoTo(P.SamusInHeatGravitySuitStep2),
        ],
        [P.WreckedShipGreenLights] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.WreckedShipGreenLights)),
            FallInto(P.WreckedShipGreenLightsCycle),
        ],
        [P.WreckedShipGreenLightsCycle] =
        [
            Show(347, 10),
            Show(348, 10),
            Show(349, 10),
            Show(350, 10),
            Show(351, 10),
            Show(352, 10),
            Show(353, 10),
            Show(354, 10),
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
            Show(355, 240),
            SetCounter(2),
            FallInto(P.CrateriaLightningRepeatedFlash),
        ],
        [P.CrateriaLightningRepeatedFlash] =
        [
            Show(356, 2),
            Show(357, 1),
            Show(358, 1),
            Show(359, 1),
            Show(360, 1),
            Show(361, 1),
            Show(362, 2),
            RepeatWhileCounting(P.CrateriaLightningRepeatedFlash),
            Show(363, 240),
            SetCounter(1),
            FallInto(P.CrateriaLightningClosingFlash),
        ],
        [P.CrateriaLightningClosingFlash] =
        [
            Show(364, 1),
            Show(365, 1),
            Show(366, 1),
            Show(367, 2),
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
            Show(368, 240),
            SetCounter(2),
            FallInto(P.UnusedDarkLightningRepeatedFlash),
        ],
        [P.UnusedDarkLightningRepeatedFlash] =
        [
            Show(369, 2),
            Show(370, 1),
            Show(371, 1),
            Show(372, 1),
            Show(373, 1),
            Show(374, 1),
            Show(375, 2),
            RepeatWhileCounting(P.UnusedDarkLightningRepeatedFlash),
            Show(376, 240),
            Show(377, 240),
            SetCounter(1),
            FallInto(P.UnusedDarkLightningClosingFlash),
        ],
        [P.UnusedDarkLightningClosingFlash] =
        [
            Show(378, 1),
            Show(379, 1),
            Show(380, 1),
            Show(381, 2),
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
            Show(382, 10),
            Show(383, 10),
            Show(384, 10),
            Show(385, 10),
            Show(386, 10),
            Show(387, 10),
            Show(388, 10),
            Show(389, 10),
            Show(390, 10),
            Show(391, 10),
            Show(392, 10),
            Show(393, 10),
            Show(394, 10),
            Show(395, 10),
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
            Show(396, 10),
            Show(397, 10),
            Show(398, 10),
            Show(399, 10),
            Show(400, 10),
            Show(401, 10),
            Show(402, 10),
            Show(403, 10),
            Show(404, 10),
            Show(405, 10),
            Show(406, 10),
            Show(407, 10),
            Show(408, 10),
            Show(409, 10),
            GoTo(P.SporeSpawnBlueBackgroundSporesCycle),
        ],
        [P.BrinstarRedBackgroundGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.BrinstarRedGlow)),
            FallInto(P.BrinstarRedBackgroundGlowCycle),
        ],
        [P.BrinstarRedBackgroundGlowCycle] =
        [
            Show(410, 10),
            Show(411, 10),
            Show(412, 10),
            Show(413, 10),
            Show(414, 10),
            Show(415, 10),
            Show(416, 10),
            Show(417, 10),
            Show(418, 10),
            Show(419, 10),
            Show(420, 10),
            Show(421, 10),
            Show(422, 10),
            Show(423, 10),
            GoTo(P.BrinstarRedBackgroundGlowCycle),
        ],
        [P.BeaconFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.Beacon)),
            FallInto(P.BeaconFlashingCycle),
        ],
        [P.BeaconFlashingCycle] =
        [
            Show(424, 10),
            Show(425, 10),
            Show(426, 10),
            Show(427, 10),
            Show(428, 10),
            Show(429, 10),
            Cue(new PaletteFxCue.QueueSound(Sound: EnemySound.Beacon)),
            Show(430, 10),
            Show(431, 10),
            Show(432, 10),
            Show(433, 10),
            GoTo(P.BeaconFlashingCycle),
        ],
        [P.NorfairForegroundAndHeatPhase] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairHeatForeground)),
            FallInto(P.NorfairForegroundAndHeatPhaseCycle),
        ],
        [P.NorfairForegroundAndHeatPhaseCycle] =
        [
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 0)),
            Show(434, 16),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 1)),
            Show(435, 4),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 2)),
            Show(436, 4),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 3)),
            Show(437, 5),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 4)),
            Show(438, 6),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 5)),
            Show(439, 7),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 6)),
            Show(440, 8),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 7)),
            Show(441, 8),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 8)),
            Show(442, 8),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 9)),
            Show(443, 8),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 10)),
            Show(444, 7),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 11)),
            Show(445, 6),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 12)),
            Show(446, 5),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 13)),
            Show(447, 4),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 14)),
            Show(448, 4),
            Cue(new PaletteFxCue.SetHeatPaletteIndex(HeatPhase: 15)),
            Show(449, 16),
            GoTo(P.NorfairForegroundAndHeatPhaseCycle),
        ],
        [P.NorfairGreenForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairGreenForeground)),
            FallInto(P.NorfairGreenForegroundHeatGlowCycle),
        ],
        [P.NorfairGreenForegroundHeatGlowCycle] =
        [
            Show(450, 16),
            Show(451, 4),
            Show(452, 4),
            Show(453, 5),
            Show(454, 6),
            Show(455, 7),
            Show(456, 8),
            Show(457, 8),
            Show(458, 8),
            Show(459, 8),
            Show(460, 7),
            Show(461, 6),
            Show(462, 5),
            Show(463, 4),
            Show(464, 4),
            Show(465, 16),
            GoTo(P.NorfairGreenForegroundHeatGlowCycle),
        ],
        [P.NorfairPurpleForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairPurpleForeground)),
            FallInto(P.NorfairPurpleForegroundHeatGlowCycle),
        ],
        [P.NorfairPurpleForegroundHeatGlowCycle] =
        [
            Show(466, 16),
            Show(467, 4),
            Show(468, 4),
            Show(469, 5),
            Show(470, 6),
            Show(471, 7),
            Show(472, 8),
            Show(473, 8),
            Show(474, 8),
            Show(475, 8),
            Show(476, 7),
            Show(477, 6),
            Show(478, 5),
            Show(479, 4),
            Show(480, 4),
            Show(481, 16),
            GoTo(P.NorfairPurpleForegroundHeatGlowCycle),
        ],
        [P.NorfairDarkForegroundHeatGlow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.NorfairDarkForeground)),
            FallInto(P.NorfairDarkForegroundHeatGlowCycle),
        ],
        [P.NorfairDarkForegroundHeatGlowCycle] =
        [
            Show(482, 16),
            Show(483, 4),
            Show(484, 4),
            Show(485, 5),
            Show(486, 6),
            Show(487, 7),
            Show(488, 8),
            Show(489, 8),
            Show(490, 8),
            Show(491, 8),
            Show(492, 7),
            Show(493, 6),
            Show(494, 5),
            Show(495, 4),
            Show(496, 4),
            Show(497, 16),
            GoTo(P.NorfairDarkForegroundHeatGlowCycle),
        ],
        [P.MaridiaSandPits] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandPits)),
            FallInto(P.MaridiaSandPitsCycle),
        ],
        [P.MaridiaSandPitsCycle] =
        [
            Show(498, 10),
            Show(499, 10),
            Show(500, 10),
            Show(501, 10),
            GoTo(P.MaridiaSandPitsCycle),
        ],
        [P.MaridiaSandFalls] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaSandFalls)),
            FallInto(P.MaridiaSandFallsCycle),
        ],
        [P.MaridiaSandFallsCycle] =
        [
            Show(502, 10),
            Show(503, 10),
            Show(504, 10),
            Show(505, 10),
            GoTo(P.MaridiaSandFallsCycle),
        ],
        [P.MaridiaBackgroundWaterfalls] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.MaridiaBackgroundWaterfalls)),
            FallInto(P.MaridiaBackgroundWaterfallsCycle),
        ],
        [P.MaridiaBackgroundWaterfallsCycle] =
        [
            Show(506, 2),
            Show(507, 2),
            Show(508, 2),
            Show(509, 2),
            Show(510, 2),
            Show(511, 2),
            Show(512, 2),
            Show(513, 2),
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
            Show(514, 10),
            Show(515, 10),
            Show(516, 10),
            Show(517, 10),
            Show(518, 10),
            Show(519, 10),
            Show(520, 10),
            Show(521, 10),
            Show(522, 10),
            Show(523, 10),
            Show(524, 10),
            GoTo(P.CommonTourianGlowCycle),
        ],
        [P.TourianShutterRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianShutters)),
            FallInto(P.TourianShutterRedFlashingCycle),
        ],
        [P.TourianShutterRedFlashingCycle] =
        [
            Show(525, 2),
            Show(526, 2),
            Show(527, 2),
            Show(528, 2),
            Show(529, 2),
            Show(530, 2),
            Show(531, 2),
            Show(532, 2),
            Show(533, 2),
            Show(534, 2),
            Show(535, 2),
            Show(536, 2),
            Show(537, 2),
            Show(538, 2),
            GoTo(P.TourianShutterRedFlashingCycle),
        ],
        [P.TourianBackgroundRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.TourianBackground)),
            FallInto(P.TourianBackgroundRedFlashingCycle),
        ],
        [P.TourianBackgroundRedFlashingCycle] =
        [
            Show(539, 4),
            Show(540, 4),
            Show(541, 4),
            Show(542, 4),
            Show(543, 4),
            Show(544, 4),
            Show(545, 4),
            Show(546, 4),
            Show(547, 4),
            Show(548, 4),
            Show(549, 4),
            Show(550, 4),
            Show(551, 4),
            Show(552, 4),
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
            Show(553, 2),
            Show(554, 2),
            Show(555, 2),
            Show(556, 2),
            Show(557, 2),
            Show(558, 2),
            Show(559, 2),
            Show(560, 2),
            Show(561, 2),
            Show(562, 2),
            Show(563, 2),
            Show(564, 2),
            Show(565, 2),
            Show(566, 2),
            GoTo(P.CommonTourianEscapeRedFlashingCycle),
        ],
        [P.OldTourianRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianLevel)),
            FallInto(P.OldTourianRedFlashingCycle),
        ],
        [P.OldTourianRedFlashingCycle] =
        [
            Show(567, 3),
            Show(568, 3),
            Show(569, 3),
            Show(570, 3),
            Show(571, 3),
            Show(572, 3),
            Show(573, 3),
            Show(574, 3),
            Show(575, 3),
            Show(576, 3),
            Show(577, 3),
            Show(578, 3),
            Show(579, 3),
            Show(580, 3),
            GoTo(P.OldTourianRedFlashingCycle),
        ],
        [P.OldTourianBackgroundRailingsFlashYellow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianRailings)),
            FallInto(P.OldTourianBackgroundRailingsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundRailingsFlashYellowCycle] =
        [
            Show(581, 16),
            Show(582, 1),
            Show(583, 1),
            Show(584, 2),
            Show(585, 1),
            Show(586, 2),
            Show(587, 1),
            Show(588, 1),
            Show(589, 1),
            Show(590, 1),
            Show(591, 32),
            Show(592, 2),
            Show(593, 1),
            Show(594, 1),
            Show(595, 1),
            GoTo(P.OldTourianBackgroundRailingsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundPanelsFlashYellow] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.OldTourianPanels)),
            FallInto(P.OldTourianBackgroundPanelsFlashYellowCycle),
        ],
        [P.OldTourianBackgroundPanelsFlashYellowCycle] =
        [
            Show(596, 16),
            Show(597, 1),
            Show(598, 1),
            Show(599, 2),
            Show(600, 1),
            Show(601, 2),
            Show(602, 1),
            Show(603, 1),
            Show(604, 1),
            Show(605, 1),
            Show(606, 32),
            Show(607, 2),
            Show(608, 1),
            Show(609, 1),
            Show(610, 1),
            GoTo(P.OldTourianBackgroundPanelsFlashYellowCycle),
        ],
        [P.UpperCrateriaRedFlashing] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.UpperCrateriaLevel)),
            FallInto(P.UpperCrateriaRedFlashingCycle),
        ],
        [P.UpperCrateriaRedFlashingCycle] =
        [
            Show(611, 8),
            Show(612, 7),
            Show(613, 6),
            Show(614, 5),
            Show(615, 4),
            Show(616, 3),
            Show(617, 2),
            Show(618, 1),
            Show(619, 2),
            Show(620, 3),
            Show(621, 4),
            Show(622, 5),
            Show(623, 6),
            Show(624, 7),
            GoTo(P.UpperCrateriaRedFlashingCycle),
        ],
        [P.CrateriaEscapeYellowLightning] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.CrateriaEscapeLightning)),
            FallInto(P.CrateriaEscapeYellowLightningCycle),
        ],
        [P.CrateriaEscapeYellowLightningCycle] =
        [
            Show(625, 49),
            Show(626, 1),
            Show(627, 1),
            Show(628, 1),
            Show(629, 1),
            Show(630, 17),
            Show(631, 1),
            Show(632, 24),
            Show(633, 1),
            Show(634, 1),
            Show(635, 1),
            GoTo(P.CrateriaEscapeYellowLightningCycle),
        ],
        [P.ParlorEscapeSkyFlicker] =
        [
            Cue(new PaletteFxCue.SetColours(PaletteFxColours.ParlorEscapeSky)),
            FallInto(P.ParlorEscapeSkyFlickerCycle),
        ],
        [P.ParlorEscapeSkyFlickerCycle] =
        [
            Show(636, 49),
            Show(637, 1),
            Show(638, 1),
            Show(639, 1),
            Show(640, 1),
            Show(641, 17),
            Show(642, 1),
            Show(643, 24),
            Show(644, 1),
            Show(645, 1),
            Show(646, 1),
            GoTo(P.ParlorEscapeSkyFlickerCycle),
        ],
    });

    /// <summary>Shows record <paramref name="record"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> Show(int record, int duration) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.Show(new(record), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> Cue(PaletteFxCue cue) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.Publish(cue);

    /// <summary>Sets the loop counter.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> SetCounter(int count) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounter(count);

    /// <summary>Repeats <paramref name="program"/> until the loop counter runs out.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> RepeatWhileCounting(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> GoTo(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> FallInto(PaletteFxProgram program) => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallInto(program);

    /// <summary>Deletes the object.</summary>
    private static AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> Stop => AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.Stop;
}
