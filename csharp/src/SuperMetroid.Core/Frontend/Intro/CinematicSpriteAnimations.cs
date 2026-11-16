using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Frontend.Intro.CinematicSpriteCueKind;
using P = SuperMetroid.Core.Frontend.Intro.CinematicSpriteSegment;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Every cinematic sprite animation as typed segments and animations (#641, #1600):
/// each frame shows a <see cref="CinematicSpritePose"/> and each owner cue becomes a
/// <see cref="CinematicSpriteCue"/>. Each animation lists the whole run of segments it plays, every segment it plays on
/// into named as its next phase, and a counted animation counts its own segment (#1978).
/// </summary>
internal static class CinematicSpriteAnimations
{
    /// <summary>Every cinematic sprite animation.</summary>
    internal static AnimationSegments<CinematicSpriteSegment, CinematicSpritePose, CinematicSpriteCue> Segments { get; } = new(
        new Dictionary<CinematicSpriteSegment, AnimationBeat<CinematicSpritePose, CinematicSpriteCue>[]>
    {
        [P.IntroMotherBrain] =
        [
            Show(S.MotherBrainPulseLow, 16),
            Show(S.MotherBrainPulseMiddle, 16),
            Show(S.MotherBrainPulseHigh, 16),
            Show(S.MotherBrainPulseMiddle, 16),
        ],
        [P.IntroMotherBrainStartPage2] =
        [
            Cue(new(K.StartIntroPage2)),
            Cue(new(K.StartMotherBrainCrossFade)),
        ],
        [P.IntroMotherBrainPageTwo] =
        [
            Show(S.MotherBrainPulseLow, 16),
            Show(S.MotherBrainPulseMiddle, 16),
            Show(S.MotherBrainPulseHigh, 16),
            Show(S.MotherBrainPulseMiddle, 16),
        ],
        [P.MetroidEgg] =
        [
            Show(S.EggIntact, 5),
        ],
        [P.MetroidEggHatching] =
        [
            Show(S.EggIntact, 32),
        ],
        [P.MetroidEggShaking] =
        [
            Show(S.EggIntact, 5),
            Show(S.EggRockingLeft, 5),
            Show(S.EggIntact, 5),
            Show(S.EggRockingRight, 5),
            Show(S.EggIntact, 10),
            Show(S.EggCrackingUpperLeft, 10),
            Show(S.EggCrackingTop, 10),
            Show(S.EggCrackingLeft, 10),
            Show(S.EggCrackingUpperRight, 10),
            Show(S.EggCrackingRight, 10),
            Show(S.EggHatched, 80),
            Cue(new(K.SpawnParticles)),
            Show(S.EggRemnantDomeSagging, 10),
        ],
        [P.MetroidEggRemnantsSettling] =
        [
            Show(S.EggRemnantDomeFalling, 10),
            Show(S.EggRemnantDomeCrumbling, 10),
            Show(S.EggRemnantRibsBared, 10),
            Show(S.EggRemnantRibsShifting, 10),
            Show(S.EggRemnantRibsSettling, 10),
            Show(S.EggRemnantSettled, 320),
            Cue(new(K.StartIntroPage3)),
            Cue(new(K.DeleteEggAfterCrossFade)),
        ],
        [P.MetroidEggHatched] =
        [
            Show(S.EggRemnantSettled, 80),
        ],
        [P.BabyMetroidBeingDelivered] =
        [
            Show(S.DeliveredBabyPulseLow, 10),
            Show(S.DeliveredBabyPulseMiddle, 10),
            Show(S.DeliveredBabyPulseHigh, 10),
            Show(S.DeliveredBabyPulseMiddle, 10),
            Cue(new(K.StartIntroPage4)),
        ],
        [P.BabyMetroidDelivered] =
        [
            Show(S.DeliveredBabyPulseLow, 10),
            Show(S.DeliveredBabyPulseMiddle, 10),
            Show(S.DeliveredBabyPulseHigh, 10),
            Show(S.DeliveredBabyPulseMiddle, 10),
        ],
        [P.BabyMetroidBeingExamined] =
        [
            Show(S.ExaminedBabyWide, 10),
            Show(S.ExaminedBabyNarrowing, 10),
            Show(S.ExaminedBabyNarrow, 10),
            Show(S.ExaminedBabyNarrowing, 10),
            Cue(new(K.StartIntroPage5)),
        ],
        [P.BabyMetroidExamined] =
        [
            Show(S.ExaminedBabyWide, 10),
            Show(S.ExaminedBabyNarrowing, 10),
            Show(S.ExaminedBabyNarrow, 10),
            Show(S.ExaminedBabyNarrowing, 10),
        ],
        [P.IntroTextCaret] =
        [
            Show(S.CaretVisible, 5),
        ],
        [P.IntroTextCaretBlink] =
        [
            Show(S.CaretVisible, 5),
            Show(S.Blank, 5),
        ],
        [P.IntroNextPageArrow] =
        [
            Show(S.NextPageArrowBright, 10),
            Show(S.NextPageArrowFading, 10),
            Show(S.NextPageArrowFaint, 10),
            Show(S.NextPageArrowFading, 10),
        ],
        [P.IntroNextPageArrowDelayed] =
        [
            Show(S.Blank, 60),
        ],
        [P.ConfusedBabyMetroid] =
        [
            Show(S.ConfusedBabyStretched, 10),
            Show(S.ConfusedBabyRound, 10),
            Show(S.ConfusedBabySquashed, 10),
            Show(S.ConfusedBabyRound, 10),
        ],
        [P.CeresDestructionLargeAsteroid] =
        [
            Show(S.StationUnderAttackLargeAsteroid, 10),
        ],
        [P.CeresUnderAttack] =
        [
            Show(S.StationUnderAttack, 10),
        ],
        [P.CeresSmallAsteroid] =
        [
            Show(S.SmallAsteroid, 10),
        ],
        [P.CeresVortex] =
        [
            Show(S.VortexFirst, 1),
            Show(S.VortexSecond, 1),
        ],
        [P.ZebesPlanet] =
        [
            Show(S.PlanetZebes, 10),
        ],
        [P.ZebesTitle] =
        [
            Show(S.Blank, 64),
            Cue(new(K.FadeInZebesText)),
            Show(S.PlanetZebesTitle, 32),
            Cue(new(K.SpawnJapaneseZebesTextIfNeeded)),
            Show(S.PlanetZebesTitle, 192),
            Cue(new(K.FadeOutZebesText)),
            Show(S.PlanetZebesTitle, 96),
            Cue(new(K.StartFlyingToZebes)),
        ],
        [P.ZebesJapaneseTitle] =
        [
            Show(S.ZebesJapaneseTitle, 192),
        ],
        [P.CeresSmallExplosion] =
        [
            Show(S.SmallExplosionSpark, 3),
            Show(S.SmallExplosionFlare, 3),
            Show(S.SmallExplosionFireball, 3),
            Show(S.SmallExplosionHollowing, 3),
            Show(S.SmallExplosionRing, 3),
            Show(S.SmallExplosionFadingRing, 3),
        ],
        [P.CeresRepeatingSmallExplosion] =
        [
            Show(S.SmallExplosionSpark, 3),
            Show(S.SmallExplosionFlare, 3),
            Show(S.SmallExplosionFireball, 3),
            Show(S.SmallExplosionHollowing, 3),
            Show(S.SmallExplosionRing, 3),
            Show(S.SmallExplosionFadingRing, 3),
            Show(S.Blank, 16),
        ],
        [P.CeresLargeExplosion] =
        [
            Show(S.LargeBlastFireballCluster, 5),
            Show(S.LargeBlastBreakingCluster, 5),
            Show(S.LargeBlastScatteringEmbers, 5),
            Show(S.LargeBlastFadingEmbers, 5),
            Show(S.Blank, 8),
        ],
        [P.MetroidEggShellUpperLeft] =
        [
            Show(S.EggShellUpperLeft, 1),
        ],
        [P.MetroidEggShellUpperRight] =
        [
            Show(S.EggShellUpperRight, 1),
        ],
        [P.MetroidEggShellLeft] =
        [
            Show(S.EggShellLeft, 1),
        ],
        [P.MetroidEggShellCentre] =
        [
            Show(S.EggShellCentre, 1),
        ],
        [P.MetroidEggShellRight] =
        [
            Show(S.EggShellRight, 1),
        ],
        [P.MetroidEggShellLower] =
        [
            Show(S.EggShellLower, 1),
        ],
        [P.MetroidEggSlimeDrop] =
        [
            Show(S.SlimeMoving, 1),
        ],
        [P.MetroidEggSlimeImpact] =
        [
            Show(S.SlimeSplashCrown, 10),
            Show(S.SlimeSplashPeak, 10),
            Show(S.SlimeSplashSinking, 10),
            Show(S.SlimePuddle, 10),
        ],
        [P.ZebesStarsUpperLeft] =
        [
            Show(S.ZebesStarsUpperLeft, 10),
        ],
        [P.ZebesStarsUpperRight] =
        [
            Show(S.ZebesStarsUpperRight, 10),
        ],
        [P.ZebesStarsLowerLeft] =
        [
            Show(S.ZebesStarsLowerLeft, 10),
        ],
        [P.ZebesStarsLowerRight] =
        [
            Show(S.ZebesStarsLowerRight, 10),
        ],
        [P.CeresStars] =
        [
            Show(S.FlightStars, 10),
        ],
        [P.IntroMotherBrainExplosionBig] =
        [
            Show(S.BigExplosionSpark, 6),
            Show(S.BigExplosionFlare, 6),
            Show(S.BigExplosionFireball, 6),
            Show(S.BigExplosionBursting, 6),
            Show(S.BigExplosionSpikedRing, 6),
            Show(S.BigExplosionScatteredSmoke, 6),
            Show(S.Blank, 16),
        ],
        [P.IntroMotherBrainExplosionSmall] =
        [
            Show(S.SmallExplosionSpark, 3),
            Show(S.SmallExplosionFlare, 3),
            Show(S.SmallExplosionFireball, 3),
            Show(S.SmallExplosionHollowing, 3),
            Show(S.SmallExplosionRing, 3),
            Show(S.SmallExplosionFadingRing, 3),
            Show(S.Blank, 16),
        ],
        [P.IntroRinka] =
        [
            Show(S.RinkaDim, 10),
            Show(S.RinkaGlowing, 10),
            Show(S.RinkaBright, 10),
            Cue(new(K.StartRinkaMoving)),
        ],
        [P.IntroRinkaMoving] =
        [
            Show(S.RinkaGlowing, 10),
            Show(S.RinkaDim, 10),
            Show(S.RinkaGlowing, 10),
            Show(S.RinkaBright, 10),
        ],
        [P.IntroRinkaSpawner] =
        [
            Show(S.Blank, 74),
            Cue(new(K.SpawnStrikingRinkaPair)),
            Show(S.Blank, 128),
            Cue(new(K.SpawnMissingRinkaPair)),
        ],
        [P.CeresStationBlast] =
        [
            Show(S.StationBlastFlare, 5),
            Show(S.StationBlastFireball, 5),
            Show(S.StationBlastBursting, 5),
            Show(S.StationBlastRingedFireball, 5),
            Show(S.StationBlastRing, 5),
            Show(S.StationBlastFadingRing, 5),
        ],
        [P.CeresExplosionSpawner] =
        [
            Show(S.Blank, 128),
            Cue(new(K.SpawnInitialExplosionWave)),
            Show(S.Blank, 80),
            Cue(new(K.StartRepeatingExplosions)),
            Show(S.Blank, 64),
            Cue(new(K.SpawnFinalExplosionWave)),
        ],
        [P.CeresLargeAsteroid] =
        [
            Show(S.LargeAsteroid, 10),
        ],
        [P.ZebesExplosionPlanet] =
        [
            Show(S.PlanetRumbleDeepShadow, 13),
            Show(S.PlanetRumbleShadow, 13),
            Show(S.PlanetRumbleLit, 13),
            Show(S.PlanetRumbleBright, 13),
            Cue(new(K.FadeExplosionColours)),
            Show(S.PlanetCracking, 32),
            Show(S.PlanetFracturing, 32),
            Show(S.PlanetBreakingApart, 32),
            Show(S.PlanetCrumbling, 32),
            Cue(new(K.SpawnExplosionSilhouette)),
        ],
        [P.ZebesExplosionGlow] =
        [
            Show(S.PlanetGlowTightHalo, 16),
            Show(S.PlanetGlowSpreadingHalo, 16),
            Show(S.PlanetGlowWideHalo, 16),
            Show(S.PlanetGlowSpreadingHalo, 16),
        ],
        [P.ZebesExplosionStars] =
        [
            Show(S.EndingStarfield, 16),
        ],
        [P.ZebesExplosionLava] =
        [
            Show(S.Blank, 156),
        ],
        [P.ZebesExplosionLavaLoop] =
        [
            Show(S.PlanetLavaDark, 10),
            Show(S.PlanetLavaBright, 10),
        ],
        [P.ZebesExplosionSilhouette] =
        [
            Show(S.PlanetSilhouette, 8),
            Cue(new(K.StartZebesExplosion)),
        ],
        [P.ZebesExplosionRightStars] =
        [
            Show(S.EndingStarfield, 144),
            Cue(new(K.ExplosionFinale)),
            Show(S.EndingStarfield, 332),
            Cue(new(K.EndZebesExplosion)),
            Cue(new(K.WaitForFlyaway)),
        ],
        [P.ZebesExplosionLeftStars] =
        [
            Show(S.EndingStarfield, 16),
        ],
        [P.ZebesExplosionAfterglow] =
        [
            Show(S.PlanetAfterglow, 16),
        ],
        [P.OperationWasText] =
        [
            Show(S.OperationTypedT, 8),
            Show(S.OperationTypedTh, 8),
            Show(S.OperationTypedThe, 15),
            Show(S.OperationTypedTheO, 8),
            Show(S.OperationTypedTheOp, 8),
            Show(S.OperationTypedTheOpe, 8),
            Show(S.OperationTypedTheOper, 8),
            Show(S.OperationTypedTheOpera, 8),
            Show(S.OperationTypedTheOperat, 8),
            Show(S.OperationTypedTheOperati, 8),
            Show(S.OperationTypedTheOperatio, 8),
            Show(S.OperationTypedTheOperation, 15),
            Show(S.OperationTypedTheOperationW, 8),
            Show(S.OperationTypedTheOperationWa, 8),
            Show(S.OperationTypedTheOperationWas, 15),
            Cue(new(K.SpawnCompletedText)),
        ],
        [P.OperationWasTextHold] =
        [
            Show(S.OperationTypedTheOperationWas, 15),
        ],
        [P.CompletedSuccessfullyText] =
        [
            Show(S.CompletedTypedC, 8),
            Show(S.CompletedTypedCo, 8),
            Show(S.CompletedTypedCom, 8),
            Show(S.CompletedTypedComp, 8),
            Show(S.CompletedTypedCompl, 8),
            Show(S.CompletedTypedComple, 8),
            Show(S.CompletedTypedComplet, 8),
            Show(S.CompletedTypedComplete, 8),
            Show(S.CompletedTypedCompleted, 15),
            Show(S.CompletedTypedCompletedS, 8),
            Show(S.CompletedTypedCompletedSu, 8),
            Show(S.CompletedTypedCompletedSuc, 8),
            Show(S.CompletedTypedCompletedSucc, 8),
            Show(S.CompletedTypedCompletedSucce, 8),
            Show(S.CompletedTypedCompletedSucces, 8),
            Show(S.CompletedTypedCompletedSuccess, 8),
            Show(S.CompletedTypedCompletedSuccessf, 8),
            Show(S.CompletedTypedCompletedSuccessfu, 8),
            Show(S.CompletedTypedCompletedSuccessful, 8),
            Show(S.CompletedTypedCompletedSuccessfull, 8),
            Show(S.CompletedTypedCompletedSuccessfully, 15),
            Cue(new(K.SpawnClearTime)),
        ],
        [P.CompletedSuccessfullyTextHold] =
        [
            Show(S.CompletedTypedCompletedSuccessfully, 8),
        ],
        [P.ClearTimeText] =
        [
            Show(S.ClearTimeTypedC, 8),
            Show(S.ClearTimeTypedCl, 8),
            Show(S.ClearTimeTypedCle, 8),
            Show(S.ClearTimeTypedClea, 8),
            Show(S.ClearTimeTypedClear, 15),
            Show(S.ClearTimeTypedClearT, 8),
            Show(S.ClearTimeTypedClearTi, 8),
            Show(S.ClearTimeTypedClearTim, 8),
            Show(S.ClearTimeTypedClearTime, 15),
            Cue(new(K.SpawnHoursTens)),
            Show(S.ClearTimeTypedClearTime, 8),
            Cue(new(K.SpawnHoursUnits)),
            Show(S.ClearTimeTypedClearTime, 8),
            Cue(new(K.SpawnColon)),
            Show(S.ClearTimeTypedClearTime, 8),
            Cue(new(K.SpawnMinutesTens)),
            Show(S.ClearTimeTypedClearTime, 8),
            Cue(new(K.SpawnMinutesUnits)),
            Show(S.ClearTimeTypedClearTime, 128),
            Cue(new(K.TransitionToCredits)),
        ],
        [P.ClearTimeTextHold] =
        [
            Show(S.ClearTimeTypedClearTime, 15),
        ],
        [P.ClearTimeDigit0] =
        [
            Show(S.ClearTimeDigit0, 8),
        ],
        [P.ClearTimeDigit1] =
        [
            Show(S.ClearTimeDigit1, 8),
        ],
        [P.ClearTimeDigit2] =
        [
            Show(S.ClearTimeDigit2, 8),
        ],
        [P.ClearTimeDigit3] =
        [
            Show(S.ClearTimeDigit3, 8),
        ],
        [P.ClearTimeDigit4] =
        [
            Show(S.ClearTimeDigit4, 8),
        ],
        [P.ClearTimeDigit5] =
        [
            Show(S.ClearTimeDigit5, 8),
        ],
        [P.ClearTimeDigit6] =
        [
            Show(S.ClearTimeDigit6, 8),
        ],
        [P.ClearTimeDigit7] =
        [
            Show(S.ClearTimeDigit7, 8),
        ],
        [P.ClearTimeDigit8] =
        [
            Show(S.ClearTimeDigit8, 8),
        ],
        [P.ClearTimeDigit9] =
        [
            Show(S.ClearTimeDigit9, 8),
        ],
        [P.ClearTimeColon] =
        [
            Show(S.ClearTimeColon, 8),
        ],
        [P.EndingAnimalEscape] =
        [
            Show(S.EscapePodShortTrail, 1),
            Show(S.EscapePodLongTrail, 1),
            Show(S.EscapePodLongTrail, 1),
            Show(S.EscapePodLongTrail, 1),
        ],
        [P.EndingTopCloudPattern] =
        [
            Show(S.TopCloudPattern, 1),
        ],
        [P.EndingTopCloudEdge] =
        [
            Show(S.TopCloudEdge, 1),
        ],
        [P.EndingBottomCloudEdge] =
        [
            Show(S.BottomCloudEdge, 1),
        ],
        [P.EndingBottomCloudPattern] =
        [
            Show(S.BottomCloudPattern, 1),
        ],
        [P.EndingRightSideCloud] =
        [
            Show(S.RightSideCloud, 1),
        ],
        [P.EndingLeftSideCloud] =
        [
            Show(S.LeftSideCloud, 1),
        ],
        [P.SuitlessIdle] =
        [
            Show(S.SuitlessIdleUpper, 128),
        ],
        [P.SuitlessLegsIdle] =
        [
            Show(S.SuitlessLower, 128),
        ],
        [P.HairRelease] =
        [
            Show(S.SuitlessIdleUpper, 90),
            Show(S.SuitlessPonytailTied, 8),
            Show(S.SuitlessHandsToPonytail, 10),
            Show(S.SuitlessUntyingPonytail, 10),
            Show(S.SuitlessPonytailUntied, 32),
            Show(S.SuitlessHairFallingLoose, 10),
            Show(S.SuitlessHairSwinging, 9),
            Show(S.SuitlessHairSweptBack, 16),
            Show(S.SuitlessHairFlowing, 10),
            Show(S.SuitlessStanding, 48),
            Cue(new(K.SpawnSuitlessJump)),
        ],
        [P.HairReleaseLegs] =
        [
            Show(S.SuitlessLower, 90),
            Show(S.SuitlessLower, 8),
            Show(S.SuitlessLower, 10),
            Show(S.SuitlessLower, 10),
            Show(S.SuitlessLower, 32),
            Show(S.SuitlessLower, 10),
            Show(S.SuitlessLower, 9),
            Show(S.SuitlessLower, 16),
            Show(S.SuitlessLower, 10),
        ],
        [P.SuitlessJump] =
        [
            Show(S.SuitlessStanding, 48),
            Show(S.SuitlessPrepareJump, 10),
            Cue(new(K.Launch)),
            Cue(new(K.StartBodyFlight)),
        ],
        [P.SuitlessJumpFlight] =
        [
            Show(S.SuitlessJumping, 48),
        ],
        [P.Falling] =
        [
            Show(S.SamusFalling, 10),
        ],
        [P.Landing] =
        [
            Show(S.SamusLanding, 10),
            Show(S.SamusLanded, 16),
            Show(S.SamusShooting, 48),
            Cue(new(K.Shoot)),
            Show(S.SamusShooting, 128),
        ],
        [P.SuitedIdle] =
        [
            Show(S.SuitedIdleBody, 10),
        ],
        [P.HelmetIdle] =
        [
            Show(S.SuitedHelmetHead, 10),
        ],
        [P.HeadIdle] =
        [
            Show(S.HelmetlessHeadResting, 10),
        ],
        [P.GestureBody] =
        [
            Show(S.SuitedIdleBody, 64),
            Show(S.SuitedHeadlessBody, 264),
        ],
        [P.GestureArm] =
        [
            Show(S.Blank, 64),
            Show(S.SuitedArmLowered, 8),
            Show(S.SuitedArmSwingingUp, 8),
            Show(S.SuitedArmFistForward, 5),
            Show(S.SuitedArmFistRaised, 4),
            Show(S.SuitedArmFistDrawingIn, 3),
            Show(S.SuitedArmFistAtChest, 32),
            Show(S.SuitedArmFistTurning, 8),
            Show(S.SuitedArmFistTurned, 64),
            Show(S.SuitedArmFistAtChest, 5),
            Show(S.SuitedArmFistDrawingIn, 5),
            Show(S.SuitedArmFistRaised, 5),
            Show(S.SuitedArmFistForward, 5),
            Show(S.SuitedArmSwingingUp, 112),
            Cue(new(K.SpawnSuitedJump)),
        ],
        [P.GestureHelmet] =
        [
            Show(S.SuitedHelmetHead, 328),
        ],
        [P.GestureHead] =
        [
            Show(S.HelmetlessHeadResting, 128),
            Show(S.HelmetlessHeadSmileStarting, 5),
            Show(S.HelmetlessHeadSmileWidening, 5),
            Show(S.HelmetlessHeadSmiling, 190),
        ],
        [P.SuitedJump] =
        [
            Cue(new(K.PrepareHead)),
            Show(S.SuitedPrepareJump, 10),
            Cue(new(K.LaunchHead)),
            Cue(new(K.Launch)),
            Cue(new(K.StartBodyFlight)),
        ],
        [P.SuitedJumpFlight] =
        [
            Show(S.SuitedJumping, 5),
        ],
        [P.HelmetJump] =
        [
            Show(S.SuitedHelmetJumpLevel, 10),
            Cue(new(K.StartHeadFlight)),
        ],
        [P.HelmetFlight] =
        [
            Show(S.SuitedHelmetJumpTilted, 5),
        ],
        [P.HeadJump] =
        [
            Show(S.HelmetlessJumpHead, 10),
            Cue(new(K.StartHeadFlight)),
        ],
        [P.HeadFlight] =
        [
            Show(S.HelmetlessJumpHead, 5),
        ],
        [P.LogoUpperS] =
        [
            Show(S.LogoSUpper, 10),
        ],
        [P.LogoLowerS] =
        [
            Show(S.LogoSLower, 10),
        ],
        [P.LogoUpperCircle] =
        [
            Show(S.Blank, 96),
            Show(S.LogoRightCircleTip, 5),
            Show(S.LogoRightCircleTipAndBlade, 5),
            Show(S.LogoRightCircleWhole, 64),
            Cue(new(K.GreyOut)),
        ],
        [P.LogoUpperCircleLoop] =
        [
            Show(S.LogoRightCircleWhole, 5),
        ],
        [P.LogoLowerCircle] =
        [
            Show(S.Blank, 96),
            Show(S.LogoLeftCircleTip, 5),
            Show(S.LogoLeftCircleTipAndBlade, 5),
        ],
        [P.LogoLowerCircleLoop] =
        [
            Show(S.LogoLeftCircleWhole, 5),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: each names the segments it plays on
    /// into as its later phases (#1978).
    /// </summary>
    internal static IReadOnlyDictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>> StartingAt { get; } = new Dictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>>
    {
        [P.IntroMotherBrain] = new([new(P.IntroMotherBrain, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainStartPage2] = new([new(P.IntroMotherBrainStartPage2, 0, 2, 1), new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IntroMotherBrainPageTwo] = new([new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEgg] = new([new(P.MetroidEgg, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggHatching] = new([new(P.MetroidEggHatching, 0, 1, 1), new(P.MetroidEggShaking, 0, 4, 4), new(P.MetroidEggShaking, 4, 9, 1), new(P.MetroidEggRemnantsSettling, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(4)),
        [P.MetroidEggRemnantsSettling] = new([new(P.MetroidEggRemnantsSettling, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidEggHatched] = new([new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BabyMetroidBeingDelivered] = new([new(P.BabyMetroidBeingDelivered, 0, 4, 10), new(P.BabyMetroidBeingDelivered, 4, 1, 1), new(P.BabyMetroidDelivered, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
        [P.BabyMetroidDelivered] = new([new(P.BabyMetroidDelivered, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BabyMetroidBeingExamined] = new([new(P.BabyMetroidBeingExamined, 0, 4, 10), new(P.BabyMetroidBeingExamined, 4, 1, 1), new(P.BabyMetroidExamined, 0, 4, 1)], AnimationEnd.LoopsFrom(2)),
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
        [P.CeresRepeatingSmallExplosion] = new([new(P.CeresRepeatingSmallExplosion, 0, 7, 6)], AnimationEnd.Stops),
        [P.CeresLargeExplosion] = new([new(P.CeresLargeExplosion, 0, 5, 7)], AnimationEnd.Stops),
        [P.MetroidEggShellUpperLeft] = new([new(P.MetroidEggShellUpperLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggShellUpperRight] = new([new(P.MetroidEggShellUpperRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggShellLeft] = new([new(P.MetroidEggShellLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggShellCentre] = new([new(P.MetroidEggShellCentre, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggShellRight] = new([new(P.MetroidEggShellRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggShellLower] = new([new(P.MetroidEggShellLower, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggSlimeDrop] = new([new(P.MetroidEggSlimeDrop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggSlimeImpact] = new([new(P.MetroidEggSlimeImpact, 0, 4, 1)], AnimationEnd.Stops),
        [P.ZebesStarsUpperLeft] = new([new(P.ZebesStarsUpperLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarsUpperRight] = new([new(P.ZebesStarsUpperRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarsLowerLeft] = new([new(P.ZebesStarsLowerLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesStarsLowerRight] = new([new(P.ZebesStarsLowerRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CeresStars] = new([new(P.CeresStars, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainExplosionBig] = new([new(P.IntroMotherBrainExplosionBig, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainExplosionSmall] = new([new(P.IntroMotherBrainExplosionSmall, 0, 7, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroRinka] = new([new(P.IntroRinka, 0, 4, 1), new(P.IntroRinkaMoving, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IntroRinkaMoving] = new([new(P.IntroRinkaMoving, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroRinkaSpawner] = new([new(P.IntroRinkaSpawner, 0, 4, 1)], AnimationEnd.Stops),
        [P.CeresStationBlast] = new([new(P.CeresStationBlast, 0, 6, 1)], AnimationEnd.Stops),
        [P.CeresExplosionSpawner] = new([new(P.CeresExplosionSpawner, 0, 6, 1)], AnimationEnd.Stops),
        [P.CeresLargeAsteroid] = new([new(P.CeresLargeAsteroid, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ZebesExplosionPlanet] = new([new(P.ZebesExplosionPlanet, 0, 4, 5), new(P.ZebesExplosionPlanet, 4, 6, 1)], AnimationEnd.Stops),
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
        [P.EndingTopCloudPattern] = new([new(P.EndingTopCloudPattern, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingTopCloudEdge] = new([new(P.EndingTopCloudEdge, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingBottomCloudEdge] = new([new(P.EndingBottomCloudEdge, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingBottomCloudPattern] = new([new(P.EndingBottomCloudPattern, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingRightSideCloud] = new([new(P.EndingRightSideCloud, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EndingLeftSideCloud] = new([new(P.EndingLeftSideCloud, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Show(CinematicSpritePose pose, int duration) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame(pose, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Cue(CinematicSpriteCue cue) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event(cue);
}
