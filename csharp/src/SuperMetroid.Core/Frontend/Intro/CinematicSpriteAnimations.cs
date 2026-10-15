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
        // $8B:CB05
        [P.IntroMotherBrain] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
            GoTo(P.IntroMotherBrain),
        ],
        // $8B:CB19
        [P.IntroMotherBrainStartPage2] =
        [
            Cue(new(K.StartIntroPage2)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.IntroMotherBrainCrossFading)),
            FallInto(P.IntroMotherBrainPageTwo),
        ],
        // $8B:CB1F
        [P.IntroMotherBrainPageTwo] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
            Show(1, 16),
            GoTo(P.IntroMotherBrainPageTwo),
        ],
        // $8B:CB33
        [P.MetroidEgg] =
        [
            Show(3, 5),
            GoTo(P.MetroidEgg),
        ],
        // $8B:CB3B
        [P.MetroidEggHatching] =
        [
            Show(3, 32),
            SetCounter(4),
            FallInto(P.MetroidEggShaking),
        ],
        // $8B:CB43
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
        // $8B:CB79
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
        // $8B:CB97
        [P.MetroidEggHatched] =
        [
            Show(18, 80),
            GoTo(P.MetroidEggHatched),
        ],
        // $8B:CB9F
        [P.BabyMetroidBeingDelivered] =
        [
            SetCounter(10),
            FallInto(P.BabyMetroidBeingDeliveredLoop),
        ],
        // $8B:CBA3
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
        // $8B:CBB9
        [P.BabyMetroidDelivered] =
        [
            Show(19, 10),
            Show(20, 10),
            Show(21, 10),
            Show(20, 10),
            GoTo(P.BabyMetroidDelivered),
        ],
        // $8B:CBCD
        [P.BabyMetroidBeingExamined] =
        [
            SetCounter(10),
            FallInto(P.BabyMetroidBeingExaminedLoop),
        ],
        // $8B:CBD1
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
        // $8B:CBE7
        [P.BabyMetroidExamined] =
        [
            Show(22, 10),
            Show(23, 10),
            Show(24, 10),
            Show(23, 10),
            GoTo(P.BabyMetroidExamined),
        ],
        // $8B:CBFB
        [P.IntroTextCaret] =
        [
            Show(25, 5),
            GoTo(P.IntroTextCaret),
        ],
        // $8B:CC03
        [P.IntroTextCaretBlink] =
        [
            Show(25, 5),
            Show(26, 5),
            GoTo(P.IntroTextCaretBlink),
        ],
        // $8B:CC0F
        [P.IntroScientistArrow] =
        [
            Show(27, 10),
            Show(28, 10),
            Show(29, 10),
            Show(28, 10),
            GoTo(P.IntroScientistArrow),
        ],
        // $8B:CC23
        [P.IntroScientistArrowDelayed] =
        [
            Show(26, 60),
            GoTo(P.IntroScientistArrow),
        ],
        // $8B:CC2B
        [P.ConfusedBabyMetroid] =
        [
            Show(30, 10),
            Show(31, 10),
            Show(32, 10),
            Show(31, 10),
            GoTo(P.ConfusedBabyMetroid),
        ],
        // $8B:CC3F
        [P.CeresDestructionLargeAsteroid] =
        [
            Show(33, 10),
            GoTo(P.CeresDestructionLargeAsteroid),
        ],
        // $8B:CC47
        [P.CeresUnderAttack] =
        [
            Show(34, 10),
            GoTo(P.CeresUnderAttack),
        ],
        // $8B:CC4F
        [P.CeresSmallAsteroid] =
        [
            Show(35, 10),
            GoTo(P.CeresSmallAsteroid),
        ],
        // $8B:CC57
        [P.CeresVortex] =
        [
            Show(36, 1),
            Show(37, 1),
            GoTo(P.CeresVortex),
        ],
        // $8B:CCAB
        [P.ZebesPlanet] =
        [
            Show(38, 10),
            GoTo(P.ZebesPlanet),
        ],
        // $8B:CCBB
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
        // $8B:CCD5
        [P.ZebesJapaneseTitle] =
        [
            Show(40, 192),
            Stop,
        ],
        // $8B:CCDB
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
        // $8B:CCF5
        [P.CeresRepeatingSmallExplosion] =
        [
            SetCounter(6),
            FallInto(P.CeresRepeatingSmallExplosionLoop),
        ],
        // $8B:CCF9
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
        // $8B:CD1B
        [P.CeresLargeExplosion] =
        [
            SetCounter(7),
            FallInto(P.CeresLargeExplosionLoop),
        ],
        // $8B:CD1F
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
        // $8B:CD39
        [P.MetroidEggParticle1] =
        [
            Show(51, 1),
            GoTo(P.MetroidEggParticle1),
        ],
        // $8B:CD41
        [P.MetroidEggParticle2] =
        [
            Show(52, 1),
            GoTo(P.MetroidEggParticle2),
        ],
        // $8B:CD49
        [P.MetroidEggParticle3] =
        [
            Show(53, 1),
            GoTo(P.MetroidEggParticle3),
        ],
        // $8B:CD51
        [P.MetroidEggParticle4] =
        [
            Show(54, 1),
            GoTo(P.MetroidEggParticle4),
        ],
        // $8B:CD59
        [P.MetroidEggParticle5] =
        [
            Show(55, 1),
            GoTo(P.MetroidEggParticle5),
        ],
        // $8B:CD61
        [P.MetroidEggParticle6] =
        [
            Show(56, 1),
            GoTo(P.MetroidEggParticle6),
        ],
        // $8B:CD69
        [P.MetroidEggParticle7] =
        [
            Show(57, 1),
            GoTo(P.MetroidEggParticle7),
        ],
        // $8B:CD71
        [P.MetroidEggParticleHitGround] =
        [
            Show(58, 10),
            Show(59, 10),
            Show(60, 10),
            Show(61, 10),
            Stop,
        ],
        // $8B:CD83
        [P.ZebesStarSheet0] =
        [
            Show(62, 10),
            GoTo(P.ZebesStarSheet0),
        ],
        // $8B:CD8B
        [P.ZebesStarSheet1] =
        [
            Show(63, 10),
            GoTo(P.ZebesStarSheet1),
        ],
        // $8B:CD93
        [P.ZebesStarSheet2] =
        [
            Show(64, 10),
            GoTo(P.ZebesStarSheet2),
        ],
        // $8B:CD9B
        [P.ZebesStarSheet3] =
        [
            Show(65, 10),
            GoTo(P.ZebesStarSheet3),
        ],
        // $8B:CDA3
        [P.CeresStars] =
        [
            Show(66, 10),
            GoTo(P.CeresStars),
        ],
        // $8B:CDAB
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
        // $8B:CDCB
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
        // $8B:CDEB
        [P.IntroRinka] =
        [
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
            Cue(new(K.StartRinkaMoving)),
            FallInto(P.IntroRinkaMoving),
        ],
        // $8B:CDF9
        [P.IntroRinkaMoving] =
        [
            Show(74, 10),
            Show(73, 10),
            Show(74, 10),
            Show(75, 10),
            GoTo(P.IntroRinkaMoving),
        ],
        // $8B:CE0D
        [P.IntroRinkaSpawner] =
        [
            Show(26, 74),
            Cue(new(K.SpawnRinkas0And1)),
            Show(26, 128),
            Cue(new(K.SpawnRinkas2And3)),
            Stop,
        ],
        // $8B:CE1B
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
        // $8B:CE35
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
        // $8B:CE4B
        [P.CeresLargeAsteroid] =
        [
            Show(82, 10),
            GoTo(P.CeresLargeAsteroid),
        ],
        // $8B:CE53
        [P.Delete] =
        [
            Stop,
        ],
        // $8B:EB0F
        [P.ZebesExplosionPlanet] =
        [
            SetCounter(5),
            FallInto(P.ZebesExplosionPlanetLoop),
        ],
        // $8B:EB13
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
        // $8B:EB3D
        [P.ZebesExplosionGlow] =
        [
            Show(91, 16),
            Show(92, 16),
            Show(93, 16),
            Show(92, 16),
            GoTo(P.ZebesExplosionGlow),
        ],
        // $8B:EB51
        [P.ZebesExplosionStars] =
        [
            Show(94, 16),
            GoTo(P.ZebesExplosionStars),
        ],
        // $8B:EB59
        [P.ZebesExplosionLava] =
        [
            Show(26, 156),
            FallInto(P.ZebesExplosionLavaLoop),
        ],
        // $8B:EB5D
        [P.ZebesExplosionLavaLoop] =
        [
            Show(95, 10),
            Show(96, 10),
            GoTo(P.ZebesExplosionLavaLoop),
        ],
        // $8B:EB69
        [P.ZebesExplosionSilhouette] =
        [
            Show(97, 8),
            Cue(new(K.StartZebesExplosion)),
            Stop,
        ],
        // $8B:EB71
        [P.ZebesExplosionRightStars] =
        [
            Show(94, 144),
            Cue(new(K.ExplosionFinale)),
            Show(94, 332),
            Cue(new(K.EndZebesExplosion)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway)),
            FallInto(P.ZebesExplosionLeftStars),
        ],
        // $8B:EB81
        [P.ZebesExplosionLeftStars] =
        [
            Show(94, 16),
            GoTo(P.ZebesExplosionLeftStars),
        ],
        // $8B:EB89
        [P.ZebesExplosionAfterglow] =
        [
            Show(98, 16),
            GoTo(P.ZebesExplosionAfterglow),
        ],
        // $8B:EB91
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
        // $8B:EBCF
        [P.OperationWasTextHold] =
        [
            Show(113, 15),
            GoTo(P.OperationWasTextHold),
        ],
        // $8B:EBD7
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
        // $8B:EC2D
        [P.CompletedSuccessfullyTextHold] =
        [
            Show(134, 8),
            GoTo(P.CompletedSuccessfullyTextHold),
        ],
        // $8B:EC35
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
        // $8B:EC79
        [P.ClearTimeTextHold] =
        [
            Show(143, 15),
            GoTo(P.ClearTimeTextHold),
        ],
        // $8B:EC81
        [P.ClearTimeDigit0] =
        [
            Show(144, 8),
            GoTo(P.ClearTimeDigit0),
        ],
        // $8B:EC89
        [P.ClearTimeDigit1] =
        [
            Show(145, 8),
            GoTo(P.ClearTimeDigit1),
        ],
        // $8B:EC91
        [P.ClearTimeDigit2] =
        [
            Show(146, 8),
            GoTo(P.ClearTimeDigit2),
        ],
        // $8B:EC99
        [P.ClearTimeDigit3] =
        [
            Show(147, 8),
            GoTo(P.ClearTimeDigit3),
        ],
        // $8B:ECA1
        [P.ClearTimeDigit4] =
        [
            Show(148, 8),
            GoTo(P.ClearTimeDigit4),
        ],
        // $8B:ECA9
        [P.ClearTimeDigit5] =
        [
            Show(149, 8),
            GoTo(P.ClearTimeDigit5),
        ],
        // $8B:ECB1
        [P.ClearTimeDigit6] =
        [
            Show(150, 8),
            GoTo(P.ClearTimeDigit6),
        ],
        // $8B:ECB9
        [P.ClearTimeDigit7] =
        [
            Show(151, 8),
            GoTo(P.ClearTimeDigit7),
        ],
        // $8B:ECC1
        [P.ClearTimeDigit8] =
        [
            Show(152, 8),
            GoTo(P.ClearTimeDigit8),
        ],
        // $8B:ECC9
        [P.ClearTimeDigit9] =
        [
            Show(153, 8),
            GoTo(P.ClearTimeDigit9),
        ],
        // $8B:ECD1
        [P.ClearTimeColon] =
        [
            Show(154, 8),
            GoTo(P.ClearTimeColon),
        ],
        // $8B:ECD9
        [P.EndingAnimalEscape] =
        [
            Show(155, 1),
            Show(156, 1),
            Show(157, 1),
            Show(158, 1),
            GoTo(P.EndingAnimalEscape),
        ],
        // $8B:ECED
        [P.EndingCloud0] =
        [
            Show(159, 1),
            GoTo(P.EndingCloud0),
        ],
        // $8B:ECF5
        [P.EndingCloud1] =
        [
            Show(160, 1),
            GoTo(P.EndingCloud1),
        ],
        // $8B:ECFD
        [P.EndingCloud2] =
        [
            Show(161, 1),
            GoTo(P.EndingCloud2),
        ],
        // $8B:ED05
        [P.EndingCloud3] =
        [
            Show(162, 1),
            GoTo(P.EndingCloud3),
        ],
        // $8B:ED0D
        [P.EndingCloud4] =
        [
            Show(163, 1),
            GoTo(P.EndingCloud4),
        ],
        // $8B:ED15
        [P.EndingCloud5] =
        [
            Show(164, 1),
            GoTo(P.EndingCloud5),
        ],
        // $8B:ED1D
        [P.SuitlessIdle] =
        [
            Show(165, 128),
            GoTo(P.SuitlessIdle),
        ],
        // $8B:ED25
        [P.SuitlessLegsIdle] =
        [
            Show(166, 128),
            GoTo(P.SuitlessLegsIdle),
        ],
        // $8B:ED2D
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
        // $8B:ED59
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
        // $8B:ED7F
        [P.SuitlessJump] =
        [
            Show(175, 48),
            Show(176, 10),
            Cue(new(K.Launch)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardBodyFlight)),
            FallInto(P.SuitlessJumpFlight),
        ],
        // $8B:ED8D
        [P.SuitlessJumpFlight] =
        [
            Show(177, 48),
            GoTo(P.SuitlessJumpFlight),
        ],
        // $8B:ED95
        [P.Falling] =
        [
            Show(178, 10),
            GoTo(P.Falling),
        ],
        // $8B:ED9D
        [P.Landing] =
        [
            Show(179, 10),
            Show(180, 16),
            Show(181, 48),
            Cue(new(K.Shoot)),
            Show(181, 128),
            Stop,
        ],
        // $8B:EDB1
        [P.SuitedIdle] =
        [
            Show(182, 10),
            GoTo(P.SuitedIdle),
        ],
        // $8B:EDB9
        [P.HelmetIdle] =
        [
            Show(183, 10),
            GoTo(P.HelmetIdle),
        ],
        // $8B:EDC1
        [P.HeadIdle] =
        [
            Show(184, 10),
            GoTo(P.HeadIdle),
        ],
        // $8B:EDC9
        [P.GestureBody] =
        [
            Show(182, 64),
            Show(185, 264),
            Stop,
        ],
        // $8B:EDD3
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
        // $8B:EE0F
        [P.GestureHelmet] =
        [
            Show(183, 328),
            Stop,
        ],
        // $8B:EE15
        [P.GestureHead] =
        [
            Show(184, 128),
            Show(194, 5),
            Show(195, 5),
            Show(196, 190),
            Stop,
        ],
        // $8B:EE27
        [P.SuitedJump] =
        [
            Cue(new(K.PrepareHead)),
            Show(197, 10),
            Cue(new(K.LaunchHead)),
            Cue(new(K.Launch)),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardBodyFlight)),
            FallInto(P.SuitedJumpFlight),
        ],
        // $8B:EE35
        [P.SuitedJumpFlight] =
        [
            Show(198, 5),
            GoTo(P.SuitedJumpFlight),
        ],
        // $8B:EE3D
        [P.HelmetJump] =
        [
            Show(199, 10),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardHeadFlight)),
            FallInto(P.HelmetFlight),
        ],
        // $8B:EE45
        [P.HelmetFlight] =
        [
            Show(200, 5),
            GoTo(P.HelmetFlight),
        ],
        // $8B:EE4D
        [P.HeadJump] =
        [
            Show(201, 10),
            Cue(new(K.SetPreInstruction, CinematicSpritePreInstruction.RewardHeadFlight)),
            FallInto(P.HeadFlight),
        ],
        // $8B:EE55
        [P.HeadFlight] =
        [
            Show(201, 5),
            GoTo(P.HeadFlight),
        ],
        // $8B:EE5D
        [P.LogoUpperS] =
        [
            Show(202, 10),
            GoTo(P.LogoUpperS),
        ],
        // $8B:EE65
        [P.LogoLowerS] =
        [
            Show(203, 10),
            GoTo(P.LogoLowerS),
        ],
        // $8B:EE6D
        [P.LogoUpperCircle] =
        [
            Show(26, 96),
            Show(204, 5),
            Show(205, 5),
            Show(206, 64),
            Cue(new(K.GreyOut)),
            FallInto(P.LogoUpperCircleLoop),
        ],
        // $8B:EE7F
        [P.LogoUpperCircleLoop] =
        [
            Show(206, 5),
            GoTo(P.LogoUpperCircleLoop),
        ],
        // $8B:EE87
        [P.LogoLowerCircle] =
        [
            Show(26, 96),
            Show(207, 5),
            Show(208, 5),
            FallInto(P.LogoLowerCircleLoop),
        ],
        // $8B:EE93
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
