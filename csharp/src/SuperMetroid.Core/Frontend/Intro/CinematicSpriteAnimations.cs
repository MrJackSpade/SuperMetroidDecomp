using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Frontend.Intro.CinematicSpriteCueKind;
using P = SuperMetroid.Core.Frontend.Intro.CinematicSpriteSegment;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Every cinematic sprite animation script as typed segments and animations (#641, #1600), generated from the verified
/// animation scripts: each frame shows a <see cref="CinematicSpritePose"/>, each owner cue becomes a
/// <see cref="CinematicSpriteCue"/>, and the shared delete ends the animation.
/// </summary>
internal static class CinematicSpriteAnimations
{
    /// <summary>Every cinematic sprite program.</summary>
    internal static AnimationSegments<CinematicSpriteSegment, CinematicSpritePose, CinematicSpriteCue> Segments { get; } = new(
        new Dictionary<CinematicSpriteSegment, AnimationBeat<CinematicSpritePose, CinematicSpriteCue>[]>
    {
        [P.IntroMotherBrain] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
        ],
        [P.IntroMotherBrainStartPage2] =
        [
            Cue(new(K.StartIntroPage2)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading)),
        ],
        [P.IntroMotherBrainPageTwo] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
        ],
        [P.MetroidEgg] =
        [
            Show(3, 5),
        ],
        [P.MetroidEggHatching] =
        [
            Show(3, 32),
        ],
        [P.MetroidEggShaking] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(3, 5),
            Show(5, 5),
            Show(3, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 80),
            Cue(new(K.SpawnParticles)),
            Show(12, 10),
        ],
        [P.MetroidEggHatchedFrame2] =
        [
            Show(13, 10),
            Show(14, 10),
            Show(15, 10),
            Show(16, 10),
            Show(17, 10),
            Show(18, 320),
            Cue(new(K.StartIntroPage3)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade)),
        ],
        [P.MetroidEggHatched] =
        [
            Show(18, 80),
        ],
        [P.BabyMetroidBeingDelivered] =
        [
        ],
        [P.BabyMetroidBeingDeliveredLoop] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(20, 10),
            Cue(new(K.StartIntroPage4)),
        ],
        [P.BabyMetroidDelivered] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(20, 10),
        ],
        [P.BabyMetroidBeingExamined] =
        [
        ],
        [P.BabyMetroidBeingExaminedLoop] =
        [
            Show(22, 10),
            Show(23, 10),
            Show(24, 10),
            Show(23, 10),
            Cue(new(K.StartIntroPage5)),
        ],
        [P.BabyMetroidExamined] =
        [
            Show(22, 10),
            Show(23, 10),
            Show(24, 10),
            Show(23, 10),
        ],
        [P.IntroTextCaret] =
        [
            Show(25, 5),
        ],
        [P.IntroTextCaretBlink] =
        [
            Show(25, 5),
            Show(26, 5),
        ],
        [P.IntroNextPageArrow] =
        [
            Show(27, 10),
            Show(28, 10),
            Show(29, 10),
            Show(28, 10),
        ],
        [P.IntroNextPageArrowDelayed] =
        [
            Show(26, 60),
        ],
        [P.ConfusedBabyMetroid] =
        [
            Show(30, 10),
            Show(31, 10),
            Show(32, 10),
            Show(31, 10),
        ],
        [P.CeresDestructionLargeAsteroid] =
        [
            Show(33, 10),
        ],
        [P.CeresUnderAttack] =
        [
            Show(34, 10),
        ],
        [P.CeresSmallAsteroid] =
        [
            Show(35, 10),
        ],
        [P.CeresVortex] =
        [
            Show(36, 1),
            Show(37, 1),
        ],
        [P.ZebesPlanet] =
        [
            Show(38, 10),
        ],
        [P.ZebesTitle] =
        [
            Show(26, 64),
            Cue(new(K.FadeInZebesText)),
            Show(39, 32),
            Cue(new(K.SpawnJapaneseZebesTextIfNeeded)),
            Show(39, 192),
            Cue(new(K.FadeOutZebesText)),
            Show(39, 96),
            Cue(new(K.StartFlyingToZebes)),
        ],
        [P.ZebesJapaneseTitle] =
        [
            Show(40, 192),
        ],
        [P.CeresSmallExplosion] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            Show(46, 3),
        ],
        [P.CeresRepeatingSmallExplosion] =
        [
        ],
        [P.CeresRepeatingSmallExplosionLoop] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            Show(46, 3),
            Show(26, 16),
        ],
        [P.CeresLargeExplosion] =
        [
        ],
        [P.CeresLargeExplosionLoop] =
        [
            Show(47, 5),
            Show(48, 5),
            Show(49, 5),
            Show(50, 5),
            Show(26, 8),
        ],
        [P.MetroidEggParticle1] =
        [
            Show(51, 1),
        ],
        [P.MetroidEggParticle2] =
        [
            Show(52, 1),
        ],
        [P.MetroidEggParticle3] =
        [
            Show(53, 1),
        ],
        [P.MetroidEggParticle4] =
        [
            Show(54, 1),
        ],
        [P.MetroidEggParticle5] =
        [
            Show(55, 1),
        ],
        [P.MetroidEggParticle6] =
        [
            Show(56, 1),
        ],
        [P.MetroidEggParticle7] =
        [
            Show(57, 1),
        ],
        [P.MetroidEggParticleHitGround] =
        [
            Show(58, 10),
            Show(59, 10),
            Show(60, 10),
            Show(61, 10),
        ],
        [P.ZebesStarSheet0] =
        [
            Show(62, 10),
        ],
        [P.ZebesStarSheet1] =
        [
            Show(63, 10),
        ],
        [P.ZebesStarSheet2] =
        [
            Show(64, 10),
        ],
        [P.ZebesStarSheet3] =
        [
            Show(65, 10),
        ],
        [P.CeresStars] =
        [
            Show(66, 10),
        ],
        [P.IntroMotherBrainExplosionBig] =
        [
            Show(67, 6),
            Show(68, 6),
            Show(69, 6),
            Show(70, 6),
            Show(71, 6),
            Show(72, 6),
            Show(26, 16),
        ],
        [P.IntroMotherBrainExplosionSmall] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            Show(46, 3),
            Show(26, 16),
        ],
        [P.IntroRinka] =
        [
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
            Cue(new(K.StartRinkaMoving)),
        ],
        [P.IntroRinkaMoving] =
        [
            Show(74, 10),
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
        ],
        [P.IntroRinkaSpawner] =
        [
            Show(26, 74),
            Cue(new(K.SpawnRinkas0And1)),
            Show(26, 128),
            Cue(new(K.SpawnRinkas2And3)),
        ],
        [P.CeresStationBlast] =
        [
            Show(76, 5),
            Show(77, 5),
            Show(78, 5),
            Show(79, 5),
            Show(80, 5),
            Show(81, 5),
        ],
        [P.CeresExplosionSpawner] =
        [
            Show(26, 128),
            Cue(new(K.SpawnInitialExplosionWave)),
            Show(26, 80),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating)),
            Show(26, 64),
            Cue(new(K.SpawnFinalExplosionWave)),
        ],
        [P.CeresLargeAsteroid] =
        [
            Show(82, 10),
        ],
        [P.Delete] =
        [
        ],
        [P.ZebesExplosionPlanet] =
        [
        ],
        [P.ZebesExplosionPlanetLoop] =
        [
            Show(83, 13),
            Show(84, 13),
            Show(85, 13),
            Show(86, 13),
            Cue(new(K.FadeExplosionPalette)),
            Show(87, 32),
            Show(88, 32),
            Show(89, 32),
            Show(90, 32),
            Cue(new(K.SpawnExplosionSilhouette)),
        ],
        [P.ZebesExplosionGlow] =
        [
            Show(91, 16),
            Show(92, 16),
            Show(93, 16),
            Show(92, 16),
        ],
        [P.ZebesExplosionStars] =
        [
            Show(94, 16),
        ],
        [P.ZebesExplosionLava] =
        [
            Show(26, 156),
        ],
        [P.ZebesExplosionLavaLoop] =
        [
            Show(95, 10),
            Show(96, 10),
        ],
        [P.ZebesExplosionSilhouette] =
        [
            Show(97, 8),
            Cue(new(K.StartZebesExplosion)),
        ],
        [P.ZebesExplosionRightStars] =
        [
            Show(94, 144),
            Cue(new(K.ExplosionFinale)),
            Show(94, 332),
            Cue(new(K.EndZebesExplosion)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway)),
        ],
        [P.ZebesExplosionLeftStars] =
        [
            Show(94, 16),
        ],
        [P.ZebesExplosionAfterglow] =
        [
            Show(98, 16),
        ],
        [P.OperationWasText] =
        [
            Show(99, 8),
            Show(100, 8),
            Show(101, 15),
            Show(102, 8),
            Show(103, 8),
            Show(104, 8),
            Show(105, 8),
            Show(106, 8),
            Show(107, 8),
            Show(108, 8),
            Show(109, 8),
            Show(110, 15),
            Show(111, 8),
            Show(112, 8),
            Show(113, 15),
            Cue(new(K.SpawnCompletedText)),
        ],
        [P.OperationWasTextHold] =
        [
            Show(113, 15),
        ],
        [P.CompletedSuccessfullyText] =
        [
            Show(114, 8),
            Show(115, 8),
            Show(116, 8),
            Show(117, 8),
            Show(118, 8),
            Show(119, 8),
            Show(120, 8),
            Show(121, 8),
            Show(122, 15),
            Show(123, 8),
            Show(124, 8),
            Show(125, 8),
            Show(126, 8),
            Show(127, 8),
            Show(128, 8),
            Show(129, 8),
            Show(130, 8),
            Show(131, 8),
            Show(132, 8),
            Show(133, 8),
            Show(134, 15),
            Cue(new(K.SpawnClearTime)),
        ],
        [P.CompletedSuccessfullyTextHold] =
        [
            Show(134, 8),
        ],
        [P.ClearTimeText] =
        [
            Show(135, 8),
            Show(136, 8),
            Show(137, 8),
            Show(138, 8),
            Show(139, 15),
            Show(140, 8),
            Show(141, 8),
            Show(142, 8),
            Show(143, 15),
            Cue(new(K.SpawnHoursTens)),
            Show(143, 8),
            Cue(new(K.SpawnHoursUnits)),
            Show(143, 8),
            Cue(new(K.SpawnColon)),
            Show(143, 8),
            Cue(new(K.SpawnMinutesTens)),
            Show(143, 8),
            Cue(new(K.SpawnMinutesUnits)),
            Show(143, 128),
            Cue(new(K.TransitionToCredits)),
        ],
        [P.ClearTimeTextHold] =
        [
            Show(143, 15),
        ],
        [P.ClearTimeDigit0] =
        [
            Show(144, 8),
        ],
        [P.ClearTimeDigit1] =
        [
            Show(145, 8),
        ],
        [P.ClearTimeDigit2] =
        [
            Show(146, 8),
        ],
        [P.ClearTimeDigit3] =
        [
            Show(147, 8),
        ],
        [P.ClearTimeDigit4] =
        [
            Show(148, 8),
        ],
        [P.ClearTimeDigit5] =
        [
            Show(149, 8),
        ],
        [P.ClearTimeDigit6] =
        [
            Show(150, 8),
        ],
        [P.ClearTimeDigit7] =
        [
            Show(151, 8),
        ],
        [P.ClearTimeDigit8] =
        [
            Show(152, 8),
        ],
        [P.ClearTimeDigit9] =
        [
            Show(153, 8),
        ],
        [P.ClearTimeColon] =
        [
            Show(154, 8),
        ],
        [P.EndingAnimalEscape] =
        [
            Show(155, 1),
            Show(156, 1),
            Show(157, 1),
            Show(158, 1),
        ],
        [P.EndingCloud0] =
        [
            Show(159, 1),
        ],
        [P.EndingCloud1] =
        [
            Show(160, 1),
        ],
        [P.EndingCloud2] =
        [
            Show(161, 1),
        ],
        [P.EndingCloud3] =
        [
            Show(162, 1),
        ],
        [P.EndingCloud4] =
        [
            Show(163, 1),
        ],
        [P.EndingCloud5] =
        [
            Show(164, 1),
        ],
        [P.SuitlessIdle] =
        [
            Show(165, 128),
        ],
        [P.SuitlessLegsIdle] =
        [
            Show(166, 128),
        ],
        [P.HairRelease] =
        [
            Show(165, 90),
            Show(167, 8),
            Show(168, 10),
            Show(169, 10),
            Show(170, 32),
            Show(171, 10),
            Show(172, 9),
            Show(173, 16),
            Show(174, 10),
            Show(175, 48),
            Cue(new(K.SpawnSuitlessJump)),
        ],
        [P.HairReleaseLegs] =
        [
            Show(166, 90),
            Show(166, 8),
            Show(166, 10),
            Show(166, 10),
            Show(166, 32),
            Show(166, 10),
            Show(166, 9),
            Show(166, 16),
            Show(166, 10),
        ],
        [P.SuitlessJump] =
        [
            Show(175, 48),
            Show(176, 10),
            Cue(new(K.Launch)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardBodyFlight)),
        ],
        [P.SuitlessJumpFlight] =
        [
            Show(177, 48),
        ],
        [P.Falling] =
        [
            Show(178, 10),
        ],
        [P.Landing] =
        [
            Show(179, 10),
            Show(180, 16),
            Show(181, 48),
            Cue(new(K.Shoot)),
            Show(181, 128),
        ],
        [P.SuitedIdle] =
        [
            Show(182, 10),
        ],
        [P.HelmetIdle] =
        [
            Show(183, 10),
        ],
        [P.HeadIdle] =
        [
            Show(184, 10),
        ],
        [P.GestureBody] =
        [
            Show(182, 64),
            Show(185, 264),
        ],
        [P.GestureArm] =
        [
            Show(26, 64),
            Show(186, 8),
            Show(187, 8),
            Show(188, 5),
            Show(189, 4),
            Show(190, 3),
            Show(191, 32),
            Show(192, 8),
            Show(193, 64),
            Show(191, 5),
            Show(190, 5),
            Show(189, 5),
            Show(188, 5),
            Show(187, 112),
            Cue(new(K.SpawnSuitedJump)),
        ],
        [P.GestureHelmet] =
        [
            Show(183, 328),
        ],
        [P.GestureHead] =
        [
            Show(184, 128),
            Show(194, 5),
            Show(195, 5),
            Show(196, 190),
        ],
        [P.SuitedJump] =
        [
            Cue(new(K.PrepareHead)),
            Show(197, 10),
            Cue(new(K.LaunchHead)),
            Cue(new(K.Launch)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardBodyFlight)),
        ],
        [P.SuitedJumpFlight] =
        [
            Show(198, 5),
        ],
        [P.HelmetJump] =
        [
            Show(199, 10),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardHeadFlight)),
        ],
        [P.HelmetFlight] =
        [
            Show(200, 5),
        ],
        [P.HeadJump] =
        [
            Show(201, 10),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardHeadFlight)),
        ],
        [P.HeadFlight] =
        [
            Show(201, 5),
        ],
        [P.LogoUpperS] =
        [
            Show(202, 10),
        ],
        [P.LogoLowerS] =
        [
            Show(203, 10),
        ],
        [P.LogoUpperCircle] =
        [
            Show(26, 96),
            Show(204, 5),
            Show(205, 5),
            Show(206, 64),
            Cue(new(K.GreyOut)),
        ],
        [P.LogoUpperCircleLoop] =
        [
            Show(206, 5),
        ],
        [P.LogoLowerCircle] =
        [
            Show(26, 96),
            Show(207, 5),
            Show(208, 5),
        ],
        [P.LogoLowerCircleLoop] =
        [
            Show(209, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>> StartingAt { get; } = new Dictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>>
    {
        [P.IntroMotherBrain] = new([new(P.IntroMotherBrain, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainStartPage2] = new([new(P.IntroMotherBrainStartPage2, 0, 2, 1), new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IntroMotherBrainPageTwo] = new([new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEgg] = new([new(P.MetroidEgg, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggHatching] = new([new(P.MetroidEggHatching, 0, 1, 1), new(P.MetroidEggShaking, 0, 4, 4), new(P.MetroidEggShaking, 4, 9, 1), new(P.MetroidEggHatchedFrame2, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(4)),
        [P.MetroidEggHatchedFrame2] = new([new(P.MetroidEggHatchedFrame2, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidEggHatched] = new([new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BabyMetroidBeingDelivered] = new([new(P.BabyMetroidBeingDeliveredLoop, 0, 4, 10), new(P.BabyMetroidBeingDeliveredLoop, 4, 1, 1), new(P.BabyMetroidDelivered, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
        [P.BabyMetroidDelivered] = new([new(P.BabyMetroidDelivered, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BabyMetroidBeingExamined] = new([new(P.BabyMetroidBeingExaminedLoop, 0, 4, 10), new(P.BabyMetroidBeingExaminedLoop, 4, 1, 1), new(P.BabyMetroidExamined, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
        [P.BabyMetroidExamined] = new([new(P.BabyMetroidExamined, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroTextCaret] = new([new(P.IntroTextCaret, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroTextCaretBlink] = new([new(P.IntroTextCaretBlink, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroNextPageArrow] = new([new(P.IntroNextPageArrow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroNextPageArrowDelayed] = new([new(P.IntroNextPageArrowDelayed, 0, 1, 1), new(P.IntroNextPageArrow, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ConfusedBabyMetroid] = new([new(P.ConfusedBabyMetroid, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresDestructionLargeAsteroid] = new([new(P.CeresDestructionLargeAsteroid, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresUnderAttack] = new([new(P.CeresUnderAttack, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresSmallAsteroid] = new([new(P.CeresSmallAsteroid, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresVortex] = new([new(P.CeresVortex, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesPlanet] = new([new(P.ZebesPlanet, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesTitle] = new([new(P.ZebesTitle, 0, 8, 1)], AnimationEnd.Stops),
        [P.ZebesJapaneseTitle] = new([new(P.ZebesJapaneseTitle, 0, 1, 1)], AnimationEnd.Stops),
        [P.CeresSmallExplosion] = new([new(P.CeresSmallExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.CeresRepeatingSmallExplosion] = new([new(P.CeresRepeatingSmallExplosionLoop, 0, 7, 6)], AnimationEnd.Stops),
        [P.CeresLargeExplosion] = new([new(P.CeresLargeExplosionLoop, 0, 5, 7)], AnimationEnd.Stops),
        [P.MetroidEggParticle1] = new([new(P.MetroidEggParticle1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle2] = new([new(P.MetroidEggParticle2, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle3] = new([new(P.MetroidEggParticle3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle4] = new([new(P.MetroidEggParticle4, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle5] = new([new(P.MetroidEggParticle5, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle6] = new([new(P.MetroidEggParticle6, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticle7] = new([new(P.MetroidEggParticle7, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggParticleHitGround] = new([new(P.MetroidEggParticleHitGround, 0, 4, 1)], AnimationEnd.Stops),
        [P.ZebesStarSheet0] = new([new(P.ZebesStarSheet0, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarSheet1] = new([new(P.ZebesStarSheet1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarSheet2] = new([new(P.ZebesStarSheet2, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarSheet3] = new([new(P.ZebesStarSheet3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresStars] = new([new(P.CeresStars, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainExplosionBig] = new([new(P.IntroMotherBrainExplosionBig, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainExplosionSmall] = new([new(P.IntroMotherBrainExplosionSmall, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroRinka] = new([new(P.IntroRinka, 0, 4, 1), new(P.IntroRinkaMoving, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IntroRinkaMoving] = new([new(P.IntroRinkaMoving, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroRinkaSpawner] = new([new(P.IntroRinkaSpawner, 0, 4, 1)], AnimationEnd.Stops),
        [P.CeresStationBlast] = new([new(P.CeresStationBlast, 0, 6, 1)], AnimationEnd.Stops),
        [P.CeresExplosionSpawner] = new([new(P.CeresExplosionSpawner, 0, 6, 1)], AnimationEnd.Stops),
        [P.CeresLargeAsteroid] = new([new(P.CeresLargeAsteroid, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([], AnimationEnd.Stops),
        [P.ZebesExplosionPlanet] = new([new(P.ZebesExplosionPlanetLoop, 0, 4, 5), new(P.ZebesExplosionPlanetLoop, 4, 6, 1)], AnimationEnd.Stops),
        [P.ZebesExplosionGlow] = new([new(P.ZebesExplosionGlow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesExplosionStars] = new([new(P.ZebesExplosionStars, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesExplosionLava] = new([new(P.ZebesExplosionLava, 0, 1, 1), new(P.ZebesExplosionLavaLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZebesExplosionLavaLoop] = new([new(P.ZebesExplosionLavaLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesExplosionSilhouette] = new([new(P.ZebesExplosionSilhouette, 0, 2, 1)], AnimationEnd.Stops),
        [P.ZebesExplosionRightStars] = new([new(P.ZebesExplosionRightStars, 0, 5, 1), new(P.ZebesExplosionLeftStars, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ZebesExplosionLeftStars] = new([new(P.ZebesExplosionLeftStars, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesExplosionAfterglow] = new([new(P.ZebesExplosionAfterglow, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OperationWasText] = new([new(P.OperationWasText, 0, 16, 1), new(P.OperationWasTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OperationWasTextHold] = new([new(P.OperationWasTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CompletedSuccessfullyText] = new([new(P.CompletedSuccessfullyText, 0, 22, 1), new(P.CompletedSuccessfullyTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CompletedSuccessfullyTextHold] = new([new(P.CompletedSuccessfullyTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeText] = new([new(P.ClearTimeText, 0, 20, 1), new(P.ClearTimeTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ClearTimeTextHold] = new([new(P.ClearTimeTextHold, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit0] = new([new(P.ClearTimeDigit0, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit1] = new([new(P.ClearTimeDigit1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit2] = new([new(P.ClearTimeDigit2, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit3] = new([new(P.ClearTimeDigit3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit4] = new([new(P.ClearTimeDigit4, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit5] = new([new(P.ClearTimeDigit5, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit6] = new([new(P.ClearTimeDigit6, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit7] = new([new(P.ClearTimeDigit7, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit8] = new([new(P.ClearTimeDigit8, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeDigit9] = new([new(P.ClearTimeDigit9, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ClearTimeColon] = new([new(P.ClearTimeColon, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingAnimalEscape] = new([new(P.EndingAnimalEscape, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud0] = new([new(P.EndingCloud0, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud1] = new([new(P.EndingCloud1, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud2] = new([new(P.EndingCloud2, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud3] = new([new(P.EndingCloud3, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud4] = new([new(P.EndingCloud4, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingCloud5] = new([new(P.EndingCloud5, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuitlessIdle] = new([new(P.SuitlessIdle, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SuitlessLegsIdle] = new([new(P.SuitlessLegsIdle, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HairRelease] = new([new(P.HairRelease, 0, 11, 1)], AnimationEnd.Stops),
        [P.HairReleaseLegs] = new([new(P.HairReleaseLegs, 0, 9, 1)], AnimationEnd.Stops),
        [P.SuitlessJump] = new([new(P.SuitlessJump, 0, 4, 1), new(P.SuitlessJumpFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuitlessJumpFlight] = new([new(P.SuitlessJumpFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Falling] = new([new(P.Falling, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Landing] = new([new(P.Landing, 0, 5, 1)], AnimationEnd.Stops),
        [P.SuitedIdle] = new([new(P.SuitedIdle, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HelmetIdle] = new([new(P.HelmetIdle, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadIdle] = new([new(P.HeadIdle, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GestureBody] = new([new(P.GestureBody, 0, 2, 1)], AnimationEnd.Stops),
        [P.GestureArm] = new([new(P.GestureArm, 0, 15, 1)], AnimationEnd.Stops),
        [P.GestureHelmet] = new([new(P.GestureHelmet, 0, 1, 1)], AnimationEnd.Stops),
        [P.GestureHead] = new([new(P.GestureHead, 0, 4, 1)], AnimationEnd.Stops),
        [P.SuitedJump] = new([new(P.SuitedJump, 0, 5, 1), new(P.SuitedJumpFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SuitedJumpFlight] = new([new(P.SuitedJumpFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HelmetJump] = new([new(P.HelmetJump, 0, 2, 1), new(P.HelmetFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HelmetFlight] = new([new(P.HelmetFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HeadJump] = new([new(P.HeadJump, 0, 2, 1), new(P.HeadFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HeadFlight] = new([new(P.HeadFlight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LogoUpperS] = new([new(P.LogoUpperS, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LogoLowerS] = new([new(P.LogoLowerS, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LogoUpperCircle] = new([new(P.LogoUpperCircle, 0, 5, 1), new(P.LogoUpperCircleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LogoUpperCircleLoop] = new([new(P.LogoUpperCircleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LogoLowerCircle] = new([new(P.LogoLowerCircle, 0, 3, 1), new(P.LogoLowerCircleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LogoLowerCircleLoop] = new([new(P.LogoLowerCircleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CinematicSpriteSegment> StartingWith(CinematicSpriteSegment segment) => StartingAt[segment];

    /// <summary>
    /// The segment each list that runs on into the list authored next, directly after its last beat, continues into: a
    /// sprite resting after that beat already stands in the next.
    /// </summary>
    internal static IReadOnlyDictionary<CinematicSpriteSegment, CinematicSpriteSegment> RunsOnInto { get; } =
        new Dictionary<CinematicSpriteSegment, CinematicSpriteSegment>
    {
        [P.IntroMotherBrainStartPage2] = P.IntroMotherBrainPageTwo,
        [P.MetroidEggShaking] = P.MetroidEggHatchedFrame2,
        [P.MetroidEggHatchedFrame2] = P.MetroidEggHatched,
        [P.BabyMetroidBeingDeliveredLoop] = P.BabyMetroidDelivered,
        [P.BabyMetroidBeingExaminedLoop] = P.BabyMetroidExamined,
        [P.IntroRinka] = P.IntroRinkaMoving,
        [P.ZebesExplosionLava] = P.ZebesExplosionLavaLoop,
        [P.ZebesExplosionRightStars] = P.ZebesExplosionLeftStars,
        [P.OperationWasText] = P.OperationWasTextHold,
        [P.CompletedSuccessfullyText] = P.CompletedSuccessfullyTextHold,
        [P.ClearTimeText] = P.ClearTimeTextHold,
        [P.SuitlessJump] = P.SuitlessJumpFlight,
        [P.SuitedJump] = P.SuitedJumpFlight,
        [P.HelmetJump] = P.HelmetFlight,
        [P.HeadJump] = P.HeadFlight,
        [P.LogoUpperCircle] = P.LogoUpperCircleLoop,
        [P.LogoLowerCircle] = P.LogoLowerCircleLoop,
    };

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Show(int pose, int duration) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Cue(CinematicSpriteCue cue) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event(cue);
}
