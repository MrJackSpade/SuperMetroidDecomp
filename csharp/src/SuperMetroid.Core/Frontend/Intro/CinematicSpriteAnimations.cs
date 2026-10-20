using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Frontend.Intro.CinematicSpriteCueKind;
using P = SuperMetroid.Core.Frontend.Intro.CinematicSpriteProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Every bank-$8B cinematic sprite list as a typed program (#641), generated from the verified native lists: each frame
/// shows a <see cref="CinematicSpritePose"/>, each owner instruction becomes a <see cref="CinematicSpriteCue"/>, and the
/// shared delete stops the program.
/// </summary>
internal static class CinematicSpriteAnimations
{
    /// <summary>Every cinematic sprite program.</summary>
    internal static AnimationProgramSet<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> Programs { get; } = new(
        new Dictionary<CinematicSpriteProgram, AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>[]>
    {
        [P.IntroMotherBrain] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
            GoTo(P.IntroMotherBrain),
        ],
        [P.IntroMotherBrainStartPage2] =
        [
            Cue(new(K.StartIntroPage2)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.IntroMotherBrainCrossFading)),
            FallInto(P.IntroMotherBrainPageTwo),
        ],
        [P.IntroMotherBrainPageTwo] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
            GoTo(P.IntroMotherBrainPageTwo),
        ],
        [P.MetroidEgg] =
        [
            Show(3, 5),
            GoTo(P.MetroidEgg),
        ],
        [P.MetroidEggHatching] =
        [
            Show(3, 32),
            SetCounter(4),
            FallInto(P.MetroidEggShaking),
        ],
        [P.MetroidEggShaking] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(3, 5),
            Show(5, 5),
            RepeatWhileCounting(P.MetroidEggShaking),
            Show(3, 10),
            Show(6, 10),
            Show(7, 10),
            Show(8, 10),
            Show(9, 10),
            Show(10, 10),
            Show(11, 80),
            Cue(new(K.SpawnParticles)),
            Show(12, 10),
            FallInto(P.MetroidEggHatchedFrame2),
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
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.MetroidEggDeleteAfterCrossFade)),
            FallInto(P.MetroidEggHatched),
        ],
        [P.MetroidEggHatched] =
        [
            Show(18, 80),
            GoTo(P.MetroidEggHatched),
        ],
        [P.BabyMetroidBeingDelivered] =
        [
            SetCounter(10),
            FallInto(P.BabyMetroidBeingDeliveredLoop),
        ],
        [P.BabyMetroidBeingDeliveredLoop] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(20, 10),
            RepeatWhileCounting(P.BabyMetroidBeingDeliveredLoop),
            Cue(new(K.StartIntroPage4)),
            FallInto(P.BabyMetroidDelivered),
        ],
        [P.BabyMetroidDelivered] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(20, 10),
            GoTo(P.BabyMetroidDelivered),
        ],
        [P.BabyMetroidBeingExamined] =
        [
            SetCounter(10),
            FallInto(P.BabyMetroidBeingExaminedLoop),
        ],
        [P.BabyMetroidBeingExaminedLoop] =
        [
            Show(22, 10),
            Show(23, 10),
            Show(24, 10),
            Show(23, 10),
            RepeatWhileCounting(P.BabyMetroidBeingExaminedLoop),
            Cue(new(K.StartIntroPage5)),
            FallInto(P.BabyMetroidExamined),
        ],
        [P.BabyMetroidExamined] =
        [
            Show(22, 10),
            Show(23, 10),
            Show(24, 10),
            Show(23, 10),
            GoTo(P.BabyMetroidExamined),
        ],
        [P.IntroTextCaret] =
        [
            Show(25, 5),
            GoTo(P.IntroTextCaret),
        ],
        [P.IntroTextCaretBlink] =
        [
            Show(25, 5),
            Show(26, 5),
            GoTo(P.IntroTextCaretBlink),
        ],
        [P.IntroScientistArrow] =
        [
            Show(27, 10),
            Show(28, 10),
            Show(29, 10),
            Show(28, 10),
            GoTo(P.IntroScientistArrow),
        ],
        [P.IntroScientistArrowDelayed] =
        [
            Show(26, 60),
            GoTo(P.IntroScientistArrow),
        ],
        [P.ConfusedBabyMetroid] =
        [
            Show(30, 10),
            Show(31, 10),
            Show(32, 10),
            Show(31, 10),
            GoTo(P.ConfusedBabyMetroid),
        ],
        [P.CeresDestructionLargeAsteroid] =
        [
            Show(33, 10),
            GoTo(P.CeresDestructionLargeAsteroid),
        ],
        [P.CeresUnderAttack] =
        [
            Show(34, 10),
            GoTo(P.CeresUnderAttack),
        ],
        [P.CeresSmallAsteroid] =
        [
            Show(35, 10),
            GoTo(P.CeresSmallAsteroid),
        ],
        [P.CeresVortex] =
        [
            Show(36, 1),
            Show(37, 1),
            GoTo(P.CeresVortex),
        ],
        [P.ZebesPlanet] =
        [
            Show(38, 10),
            GoTo(P.ZebesPlanet),
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
            Stop,
        ],
        [P.ZebesJapaneseTitle] =
        [
            Show(40, 192),
            Stop,
        ],
        [P.CeresSmallExplosion] =
        [
            Show(41, 3),
            Show(42, 3),
            Show(43, 3),
            Show(44, 3),
            Show(45, 3),
            Show(46, 3),
            Stop,
        ],
        [P.CeresRepeatingSmallExplosion] =
        [
            SetCounter(6),
            FallInto(P.CeresRepeatingSmallExplosionLoop),
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
            RepeatWhileCounting(P.CeresRepeatingSmallExplosionLoop),
            Stop,
        ],
        [P.CeresLargeExplosion] =
        [
            SetCounter(7),
            FallInto(P.CeresLargeExplosionLoop),
        ],
        [P.CeresLargeExplosionLoop] =
        [
            Show(47, 5),
            Show(48, 5),
            Show(49, 5),
            Show(50, 5),
            Show(26, 8),
            RepeatWhileCounting(P.CeresLargeExplosionLoop),
            Stop,
        ],
        [P.MetroidEggParticle1] =
        [
            Show(51, 1),
            GoTo(P.MetroidEggParticle1),
        ],
        [P.MetroidEggParticle2] =
        [
            Show(52, 1),
            GoTo(P.MetroidEggParticle2),
        ],
        [P.MetroidEggParticle3] =
        [
            Show(53, 1),
            GoTo(P.MetroidEggParticle3),
        ],
        [P.MetroidEggParticle4] =
        [
            Show(54, 1),
            GoTo(P.MetroidEggParticle4),
        ],
        [P.MetroidEggParticle5] =
        [
            Show(55, 1),
            GoTo(P.MetroidEggParticle5),
        ],
        [P.MetroidEggParticle6] =
        [
            Show(56, 1),
            GoTo(P.MetroidEggParticle6),
        ],
        [P.MetroidEggParticle7] =
        [
            Show(57, 1),
            GoTo(P.MetroidEggParticle7),
        ],
        [P.MetroidEggParticleHitGround] =
        [
            Show(58, 10),
            Show(59, 10),
            Show(60, 10),
            Show(61, 10),
            Stop,
        ],
        [P.ZebesStarSheet0] =
        [
            Show(62, 10),
            GoTo(P.ZebesStarSheet0),
        ],
        [P.ZebesStarSheet1] =
        [
            Show(63, 10),
            GoTo(P.ZebesStarSheet1),
        ],
        [P.ZebesStarSheet2] =
        [
            Show(64, 10),
            GoTo(P.ZebesStarSheet2),
        ],
        [P.ZebesStarSheet3] =
        [
            Show(65, 10),
            GoTo(P.ZebesStarSheet3),
        ],
        [P.CeresStars] =
        [
            Show(66, 10),
            GoTo(P.CeresStars),
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
            GoTo(P.IntroMotherBrainExplosionBig),
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
            GoTo(P.IntroMotherBrainExplosionSmall),
        ],
        [P.IntroRinka] =
        [
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
            Cue(new(K.StartRinkaMoving)),
            FallInto(P.IntroRinkaMoving),
        ],
        [P.IntroRinkaMoving] =
        [
            Show(74, 10),
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
            GoTo(P.IntroRinkaMoving),
        ],
        [P.IntroRinkaSpawner] =
        [
            Show(26, 74),
            Cue(new(K.SpawnRinkas0And1)),
            Show(26, 128),
            Cue(new(K.SpawnRinkas2And3)),
            Stop,
        ],
        [P.CeresStationBlast] =
        [
            Show(76, 5),
            Show(77, 5),
            Show(78, 5),
            Show(79, 5),
            Show(80, 5),
            Show(81, 5),
            Stop,
        ],
        [P.CeresExplosionSpawner] =
        [
            Show(26, 128),
            Cue(new(K.SpawnInitialExplosionWave)),
            Show(26, 80),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.CeresExplosionSpawnerRepeating)),
            Show(26, 64),
            Cue(new(K.SpawnFinalExplosionWave)),
            Stop,
        ],
        [P.CeresLargeAsteroid] =
        [
            Show(82, 10),
            GoTo(P.CeresLargeAsteroid),
        ],
        [P.Delete] =
        [
            Stop,
        ],
        [P.ZebesExplosionPlanet] =
        [
            SetCounter(5),
            FallInto(P.ZebesExplosionPlanetLoop),
        ],
        [P.ZebesExplosionPlanetLoop] =
        [
            Show(83, 13),
            Show(84, 13),
            Show(85, 13),
            Show(86, 13),
            RepeatWhileCounting(P.ZebesExplosionPlanetLoop),
            Cue(new(K.FadeExplosionPalette)),
            Show(87, 32),
            Show(88, 32),
            Show(89, 32),
            Show(90, 32),
            Cue(new(K.SpawnExplosionSilhouette)),
            Stop,
        ],
        [P.ZebesExplosionGlow] =
        [
            Show(91, 16),
            Show(92, 16),
            Show(93, 16),
            Show(92, 16),
            GoTo(P.ZebesExplosionGlow),
        ],
        [P.ZebesExplosionStars] =
        [
            Show(94, 16),
            GoTo(P.ZebesExplosionStars),
        ],
        [P.ZebesExplosionLava] =
        [
            Show(26, 156),
            FallInto(P.ZebesExplosionLavaLoop),
        ],
        [P.ZebesExplosionLavaLoop] =
        [
            Show(95, 10),
            Show(96, 10),
            GoTo(P.ZebesExplosionLavaLoop),
        ],
        [P.ZebesExplosionSilhouette] =
        [
            Show(97, 8),
            Cue(new(K.StartZebesExplosion)),
            Stop,
        ],
        [P.ZebesExplosionRightStars] =
        [
            Show(94, 144),
            Cue(new(K.ExplosionFinale)),
            Show(94, 332),
            Cue(new(K.EndZebesExplosion)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway)),
            FallInto(P.ZebesExplosionLeftStars),
        ],
        [P.ZebesExplosionLeftStars] =
        [
            Show(94, 16),
            GoTo(P.ZebesExplosionLeftStars),
        ],
        [P.ZebesExplosionAfterglow] =
        [
            Show(98, 16),
            GoTo(P.ZebesExplosionAfterglow),
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
            FallInto(P.OperationWasTextHold),
        ],
        [P.OperationWasTextHold] =
        [
            Show(113, 15),
            GoTo(P.OperationWasTextHold),
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
            FallInto(P.CompletedSuccessfullyTextHold),
        ],
        [P.CompletedSuccessfullyTextHold] =
        [
            Show(134, 8),
            GoTo(P.CompletedSuccessfullyTextHold),
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
            FallInto(P.ClearTimeTextHold),
        ],
        [P.ClearTimeTextHold] =
        [
            Show(143, 15),
            GoTo(P.ClearTimeTextHold),
        ],
        [P.ClearTimeDigit0] =
        [
            Show(144, 8),
            GoTo(P.ClearTimeDigit0),
        ],
        [P.ClearTimeDigit1] =
        [
            Show(145, 8),
            GoTo(P.ClearTimeDigit1),
        ],
        [P.ClearTimeDigit2] =
        [
            Show(146, 8),
            GoTo(P.ClearTimeDigit2),
        ],
        [P.ClearTimeDigit3] =
        [
            Show(147, 8),
            GoTo(P.ClearTimeDigit3),
        ],
        [P.ClearTimeDigit4] =
        [
            Show(148, 8),
            GoTo(P.ClearTimeDigit4),
        ],
        [P.ClearTimeDigit5] =
        [
            Show(149, 8),
            GoTo(P.ClearTimeDigit5),
        ],
        [P.ClearTimeDigit6] =
        [
            Show(150, 8),
            GoTo(P.ClearTimeDigit6),
        ],
        [P.ClearTimeDigit7] =
        [
            Show(151, 8),
            GoTo(P.ClearTimeDigit7),
        ],
        [P.ClearTimeDigit8] =
        [
            Show(152, 8),
            GoTo(P.ClearTimeDigit8),
        ],
        [P.ClearTimeDigit9] =
        [
            Show(153, 8),
            GoTo(P.ClearTimeDigit9),
        ],
        [P.ClearTimeColon] =
        [
            Show(154, 8),
            GoTo(P.ClearTimeColon),
        ],
        [P.EndingAnimalEscape] =
        [
            Show(155, 1),
            Show(156, 1),
            Show(157, 1),
            Show(158, 1),
            GoTo(P.EndingAnimalEscape),
        ],
        [P.EndingCloud0] =
        [
            Show(159, 1),
            GoTo(P.EndingCloud0),
        ],
        [P.EndingCloud1] =
        [
            Show(160, 1),
            GoTo(P.EndingCloud1),
        ],
        [P.EndingCloud2] =
        [
            Show(161, 1),
            GoTo(P.EndingCloud2),
        ],
        [P.EndingCloud3] =
        [
            Show(162, 1),
            GoTo(P.EndingCloud3),
        ],
        [P.EndingCloud4] =
        [
            Show(163, 1),
            GoTo(P.EndingCloud4),
        ],
        [P.EndingCloud5] =
        [
            Show(164, 1),
            GoTo(P.EndingCloud5),
        ],
        [P.SuitlessIdle] =
        [
            Show(165, 128),
            GoTo(P.SuitlessIdle),
        ],
        [P.SuitlessLegsIdle] =
        [
            Show(166, 128),
            GoTo(P.SuitlessLegsIdle),
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
            Stop,
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
            Stop,
        ],
        [P.SuitlessJump] =
        [
            Show(175, 48),
            Show(176, 10),
            Cue(new(K.Launch)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardBodyFlight)),
            FallInto(P.SuitlessJumpFlight),
        ],
        [P.SuitlessJumpFlight] =
        [
            Show(177, 48),
            GoTo(P.SuitlessJumpFlight),
        ],
        [P.Falling] =
        [
            Show(178, 10),
            GoTo(P.Falling),
        ],
        [P.Landing] =
        [
            Show(179, 10),
            Show(180, 16),
            Show(181, 48),
            Cue(new(K.Shoot)),
            Show(181, 128),
            Stop,
        ],
        [P.SuitedIdle] =
        [
            Show(182, 10),
            GoTo(P.SuitedIdle),
        ],
        [P.HelmetIdle] =
        [
            Show(183, 10),
            GoTo(P.HelmetIdle),
        ],
        [P.HeadIdle] =
        [
            Show(184, 10),
            GoTo(P.HeadIdle),
        ],
        [P.GestureBody] =
        [
            Show(182, 64),
            Show(185, 264),
            Stop,
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
            Stop,
        ],
        [P.GestureHelmet] =
        [
            Show(183, 328),
            Stop,
        ],
        [P.GestureHead] =
        [
            Show(184, 128),
            Show(194, 5),
            Show(195, 5),
            Show(196, 190),
            Stop,
        ],
        [P.SuitedJump] =
        [
            Cue(new(K.PrepareHead)),
            Show(197, 10),
            Cue(new(K.LaunchHead)),
            Cue(new(K.Launch)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardBodyFlight)),
            FallInto(P.SuitedJumpFlight),
        ],
        [P.SuitedJumpFlight] =
        [
            Show(198, 5),
            GoTo(P.SuitedJumpFlight),
        ],
        [P.HelmetJump] =
        [
            Show(199, 10),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardHeadFlight)),
            FallInto(P.HelmetFlight),
        ],
        [P.HelmetFlight] =
        [
            Show(200, 5),
            GoTo(P.HelmetFlight),
        ],
        [P.HeadJump] =
        [
            Show(201, 10),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardHeadFlight)),
            FallInto(P.HeadFlight),
        ],
        [P.HeadFlight] =
        [
            Show(201, 5),
            GoTo(P.HeadFlight),
        ],
        [P.LogoUpperS] =
        [
            Show(202, 10),
            GoTo(P.LogoUpperS),
        ],
        [P.LogoLowerS] =
        [
            Show(203, 10),
            GoTo(P.LogoLowerS),
        ],
        [P.LogoUpperCircle] =
        [
            Show(26, 96),
            Show(204, 5),
            Show(205, 5),
            Show(206, 64),
            Cue(new(K.GreyOut)),
            FallInto(P.LogoUpperCircleLoop),
        ],
        [P.LogoUpperCircleLoop] =
        [
            Show(206, 5),
            GoTo(P.LogoUpperCircleLoop),
        ],
        [P.LogoLowerCircle] =
        [
            Show(26, 96),
            Show(207, 5),
            Show(208, 5),
            FallInto(P.LogoLowerCircleLoop),
        ],
        [P.LogoLowerCircleLoop] =
        [
            Show(209, 5),
            GoTo(P.LogoLowerCircleLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> Show(int pose, int duration) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> Cue(CinematicSpriteCue cue) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.Publish(cue);

    /// <summary>Loads the actor's loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> SetCounter(int count) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> RepeatWhileCounting(CinematicSpriteProgram program) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> GoTo(CinematicSpriteProgram program) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> FallInto(CinematicSpriteProgram program) => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.FallInto(program);

    /// <summary>Deletes the actor.</summary>
    private static AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> Stop => AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.Stop;
}
