using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Frontend.Intro.CinematicSpriteCueKind;
using P = SuperMetroid.Core.Frontend.Intro.CinematicSpriteSegment;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Every cinematic sprite animation as typed segments and animations (#641, #1600):
/// each frame shows a <see cref="CinematicSpritePose"/>, each owner cue becomes a
/// <see cref="CinematicSpriteCue"/>, and the shared delete ends the animation.
/// </summary>
internal static class CinematicSpriteAnimations
{
    /// <summary>Every cinematic sprite animation.</summary>
    internal static AnimationSegments<CinematicSpriteSegment, CinematicSpritePose, CinematicSpriteCue> Segments { get; } = new(
        new Dictionary<CinematicSpriteSegment, AnimationBeat<CinematicSpritePose, CinematicSpriteCue>[]>
    {
        [P.IntroMotherBrain] =
        [
            Show(S.MotherBrainFrame0, 16),
            Show(S.MotherBrainFrame1, 16),
            Show(S.MotherBrainFrame2, 16),
            Show(S.MotherBrainFrame1, 16),
        ],
        [P.IntroMotherBrainStartPage2] =
        [
            Cue(new(K.StartIntroPage2)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.IntroMotherBrainCrossFading)),
        ],
        [P.IntroMotherBrainPageTwo] =
        [
            Show(S.MotherBrainFrame0, 16),
            Show(S.MotherBrainFrame1, 16),
            Show(S.MotherBrainFrame2, 16),
            Show(S.MotherBrainFrame1, 16),
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
            Show(S.EggCrack1, 5),
            Show(S.EggIntact, 5),
            Show(S.EggCrack2, 5),
            Show(S.EggIntact, 10),
            Show(S.EggCrack3, 10),
            Show(S.EggCrack4, 10),
            Show(S.EggCrack5, 10),
            Show(S.EggCrack6, 10),
            Show(S.EggCrack7, 10),
            Show(S.EggHatched, 80),
            Cue(new(K.SpawnParticles)),
            Show(S.EggRemnant1, 10),
        ],
        [P.MetroidEggRemnantsSettling] =
        [
            Show(S.EggRemnant2, 10),
            Show(S.EggRemnant3, 10),
            Show(S.EggRemnant4, 10),
            Show(S.EggRemnant5, 10),
            Show(S.EggRemnant6, 10),
            Show(S.EggRemnant7, 320),
            Cue(new(K.StartIntroPage3)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.MetroidEggDeleteAfterCrossFade)),
        ],
        [P.MetroidEggHatched] =
        [
            Show(S.EggRemnant7, 80),
        ],
        [P.BabyMetroidBeingDelivered] =
        [
        ],
        [P.BabyMetroidBeingDeliveredLoop] =
        [
            Show(S.DeliveredBaby1, 10),
            Show(S.DeliveredBaby2, 10),
            Show(S.DeliveredBaby3, 10),
            Show(S.DeliveredBaby2, 10),
            Cue(new(K.StartIntroPage4)),
        ],
        [P.BabyMetroidDelivered] =
        [
            Show(S.DeliveredBaby1, 10),
            Show(S.DeliveredBaby2, 10),
            Show(S.DeliveredBaby3, 10),
            Show(S.DeliveredBaby2, 10),
        ],
        [P.BabyMetroidBeingExamined] =
        [
        ],
        [P.BabyMetroidBeingExaminedLoop] =
        [
            Show(S.ExaminedBaby1, 10),
            Show(S.ExaminedBaby2, 10),
            Show(S.ExaminedBaby3, 10),
            Show(S.ExaminedBaby2, 10),
            Cue(new(K.StartIntroPage5)),
        ],
        [P.BabyMetroidExamined] =
        [
            Show(S.ExaminedBaby1, 10),
            Show(S.ExaminedBaby2, 10),
            Show(S.ExaminedBaby3, 10),
            Show(S.ExaminedBaby2, 10),
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
            Show(S.NextPageArrow1, 10),
            Show(S.NextPageArrow2, 10),
            Show(S.NextPageArrow3, 10),
            Show(S.NextPageArrow2, 10),
        ],
        [P.IntroNextPageArrowDelayed] =
        [
            Show(S.Blank, 60),
        ],
        [P.ConfusedBabyMetroid] =
        [
            Show(S.ConfusedBaby1, 10),
            Show(S.ConfusedBaby2, 10),
            Show(S.ConfusedBaby3, 10),
            Show(S.ConfusedBaby2, 10),
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
            Show(S.VortexEven, 1),
            Show(S.VortexOdd, 1),
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
            Show(S.SmallExplosion0, 3),
            Show(S.SmallExplosion1, 3),
            Show(S.SmallExplosion2, 3),
            Show(S.SmallExplosion3, 3),
            Show(S.SmallExplosion4, 3),
            Show(S.SmallExplosion5, 3),
        ],
        [P.CeresRepeatingSmallExplosion] =
        [
        ],
        [P.CeresRepeatingSmallExplosionLoop] =
        [
            Show(S.SmallExplosion0, 3),
            Show(S.SmallExplosion1, 3),
            Show(S.SmallExplosion2, 3),
            Show(S.SmallExplosion3, 3),
            Show(S.SmallExplosion4, 3),
            Show(S.SmallExplosion5, 3),
            Show(S.Blank, 16),
        ],
        [P.CeresLargeExplosion] =
        [
        ],
        [P.CeresLargeExplosionLoop] =
        [
            Show(S.LargeBlast0, 5),
            Show(S.LargeBlast1, 5),
            Show(S.LargeBlast2, 5),
            Show(S.LargeBlast3, 5),
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
            Show(S.SlimeImpact0, 10),
            Show(S.SlimeImpact1, 10),
            Show(S.SlimeImpact2, 10),
            Show(S.SlimeImpact3, 10),
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
            Show(S.BigExplosion0, 6),
            Show(S.BigExplosion1, 6),
            Show(S.BigExplosion2, 6),
            Show(S.BigExplosion3, 6),
            Show(S.BigExplosion4, 6),
            Show(S.BigExplosion5, 6),
            Show(S.Blank, 16),
        ],
        [P.IntroMotherBrainExplosionSmall] =
        [
            Show(S.SmallExplosion0, 3),
            Show(S.SmallExplosion1, 3),
            Show(S.SmallExplosion2, 3),
            Show(S.SmallExplosion3, 3),
            Show(S.SmallExplosion4, 3),
            Show(S.SmallExplosion5, 3),
            Show(S.Blank, 16),
        ],
        [P.IntroRinka] =
        [
            Show(S.Rinka0, 10),
            Show(S.Rinka1, 10),
            Show(S.Rinka2, 10),
            Cue(new(K.StartRinkaMoving)),
        ],
        [P.IntroRinkaMoving] =
        [
            Show(S.Rinka1, 10),
            Show(S.Rinka0, 10),
            Show(S.Rinka1, 10),
            Show(S.Rinka2, 10),
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
            Show(S.StationBlast0, 5),
            Show(S.StationBlast1, 5),
            Show(S.StationBlast2, 5),
            Show(S.StationBlast3, 5),
            Show(S.StationBlast4, 5),
            Show(S.StationBlast5, 5),
        ],
        [P.CeresExplosionSpawner] =
        [
            Show(S.Blank, 128),
            Cue(new(K.SpawnInitialExplosionWave)),
            Show(S.Blank, 80),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.CeresExplosionSpawnerRepeating)),
            Show(S.Blank, 64),
            Cue(new(K.SpawnFinalExplosionWave)),
        ],
        [P.CeresLargeAsteroid] =
        [
            Show(S.LargeAsteroid, 10),
        ],
        [P.Delete] =
        [
        ],
        [P.ZebesExplosionPlanet] =
        [
        ],
        [P.ZebesExplosionPlanetLoop] =
        [
            Show(S.PlanetDamage0, 13),
            Show(S.PlanetDamage1, 13),
            Show(S.PlanetDamage2, 13),
            Show(S.PlanetDamage3, 13),
            Cue(new(K.FadeExplosionColours)),
            Show(S.PlanetFlash0, 32),
            Show(S.PlanetFlash1, 32),
            Show(S.PlanetFlash2, 32),
            Show(S.PlanetFlash3, 32),
            Cue(new(K.SpawnExplosionSilhouette)),
        ],
        [P.ZebesExplosionGlow] =
        [
            Show(S.PlanetGlow0, 16),
            Show(S.PlanetGlow1, 16),
            Show(S.PlanetGlow2, 16),
            Show(S.PlanetGlow1, 16),
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
            Show(S.PlanetLava0, 10),
            Show(S.PlanetLava1, 10),
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
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.ZebesExplosionStarsWaitForFlyaway)),
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
            Show(S.OperationText00, 8),
            Show(S.OperationText01, 8),
            Show(S.OperationText02, 15),
            Show(S.OperationText03, 8),
            Show(S.OperationText04, 8),
            Show(S.OperationText05, 8),
            Show(S.OperationText06, 8),
            Show(S.OperationText07, 8),
            Show(S.OperationText08, 8),
            Show(S.OperationText09, 8),
            Show(S.OperationText10, 8),
            Show(S.OperationText11, 15),
            Show(S.OperationText12, 8),
            Show(S.OperationText13, 8),
            Show(S.OperationText14, 15),
            Cue(new(K.SpawnCompletedText)),
        ],
        [P.OperationWasTextHold] =
        [
            Show(S.OperationText14, 15),
        ],
        [P.CompletedSuccessfullyText] =
        [
            Show(S.CompletedText00, 8),
            Show(S.CompletedText01, 8),
            Show(S.CompletedText02, 8),
            Show(S.CompletedText03, 8),
            Show(S.CompletedText04, 8),
            Show(S.CompletedText05, 8),
            Show(S.CompletedText06, 8),
            Show(S.CompletedText07, 8),
            Show(S.CompletedText08, 15),
            Show(S.CompletedText09, 8),
            Show(S.CompletedText10, 8),
            Show(S.CompletedText11, 8),
            Show(S.CompletedText12, 8),
            Show(S.CompletedText13, 8),
            Show(S.CompletedText14, 8),
            Show(S.CompletedText15, 8),
            Show(S.CompletedText16, 8),
            Show(S.CompletedText17, 8),
            Show(S.CompletedText18, 8),
            Show(S.CompletedText19, 8),
            Show(S.CompletedText20, 15),
            Cue(new(K.SpawnClearTime)),
        ],
        [P.CompletedSuccessfullyTextHold] =
        [
            Show(S.CompletedText20, 8),
        ],
        [P.ClearTimeText] =
        [
            Show(S.ClearTimeText00, 8),
            Show(S.ClearTimeText01, 8),
            Show(S.ClearTimeText02, 8),
            Show(S.ClearTimeText03, 8),
            Show(S.ClearTimeText04, 15),
            Show(S.ClearTimeText05, 8),
            Show(S.ClearTimeText06, 8),
            Show(S.ClearTimeText07, 8),
            Show(S.ClearTimeText08, 15),
            Cue(new(K.SpawnHoursTens)),
            Show(S.ClearTimeText08, 8),
            Cue(new(K.SpawnHoursUnits)),
            Show(S.ClearTimeText08, 8),
            Cue(new(K.SpawnColon)),
            Show(S.ClearTimeText08, 8),
            Cue(new(K.SpawnMinutesTens)),
            Show(S.ClearTimeText08, 8),
            Cue(new(K.SpawnMinutesUnits)),
            Show(S.ClearTimeText08, 128),
            Cue(new(K.TransitionToCredits)),
        ],
        [P.ClearTimeTextHold] =
        [
            Show(S.ClearTimeText08, 15),
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
            Show(S.EscapePod0, 1),
            Show(S.EscapePod1, 1),
            Show(S.EscapePod2, 1),
            Show(S.EscapePod3, 1),
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
            Show(S.SuitlessHair1, 8),
            Show(S.SuitlessHair2, 10),
            Show(S.SuitlessHair3, 10),
            Show(S.SuitlessHair4, 32),
            Show(S.SuitlessHair5, 10),
            Show(S.SuitlessHair6, 9),
            Show(S.SuitlessHair7, 16),
            Show(S.SuitlessHair8, 10),
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
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardBodyFlight)),
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
            Show(S.HelmetlessHead1, 10),
        ],
        [P.GestureBody] =
        [
            Show(S.SuitedIdleBody, 64),
            Show(S.SuitedHeadlessBody, 264),
        ],
        [P.GestureArm] =
        [
            Show(S.Blank, 64),
            Show(S.SuitedArm1, 8),
            Show(S.SuitedArm2, 8),
            Show(S.SuitedArm3, 5),
            Show(S.SuitedArm4, 4),
            Show(S.SuitedArm5, 3),
            Show(S.SuitedArm6, 32),
            Show(S.SuitedArm7, 8),
            Show(S.SuitedArm8, 64),
            Show(S.SuitedArm6, 5),
            Show(S.SuitedArm5, 5),
            Show(S.SuitedArm4, 5),
            Show(S.SuitedArm3, 5),
            Show(S.SuitedArm2, 112),
            Cue(new(K.SpawnSuitedJump)),
        ],
        [P.GestureHelmet] =
        [
            Show(S.SuitedHelmetHead, 328),
        ],
        [P.GestureHead] =
        [
            Show(S.HelmetlessHead1, 128),
            Show(S.HelmetlessHead2, 5),
            Show(S.HelmetlessHead3, 5),
            Show(S.HelmetlessHead4, 190),
        ],
        [P.SuitedJump] =
        [
            Cue(new(K.PrepareHead)),
            Show(S.SuitedPrepareJump, 10),
            Cue(new(K.LaunchHead)),
            Cue(new(K.Launch)),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardBodyFlight)),
        ],
        [P.SuitedJumpFlight] =
        [
            Show(S.SuitedJumping, 5),
        ],
        [P.HelmetJump] =
        [
            Show(S.SuitedHelmetJumpHead1, 10),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardHeadFlight)),
        ],
        [P.HelmetFlight] =
        [
            Show(S.SuitedHelmetJumpHead2, 5),
        ],
        [P.HeadJump] =
        [
            Show(S.HelmetlessJumpHead, 10),
            Cue(new(K.SetFrameBehavior, CinematicSpriteFrameBehavior.RewardHeadFlight)),
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
            Show(S.LogoCircleRight1, 5),
            Show(S.LogoCircleRight2, 5),
            Show(S.LogoCircleRight3, 64),
            Cue(new(K.GreyOut)),
        ],
        [P.LogoUpperCircleLoop] =
        [
            Show(S.LogoCircleRight3, 5),
        ],
        [P.LogoLowerCircle] =
        [
            Show(S.Blank, 96),
            Show(S.LogoCircleLeft1, 5),
            Show(S.LogoCircleLeft2, 5),
        ],
        [P.LogoLowerCircleLoop] =
        [
            Show(S.LogoCircleLeft3, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>> StartingAt { get; } = new Dictionary<CinematicSpriteSegment, Animation<CinematicSpriteSegment>>
    {
        [P.IntroMotherBrain] = new([new(P.IntroMotherBrain, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IntroMotherBrainStartPage2] = new([new(P.IntroMotherBrainStartPage2, 0, 2, 1), new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IntroMotherBrainPageTwo] = new([new(P.IntroMotherBrainPageTwo, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEgg] = new([new(P.MetroidEgg, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidEggHatching] = new([new(P.MetroidEggHatching, 0, 1, 1), new(P.MetroidEggShaking, 0, 4, 4), new(P.MetroidEggShaking, 4, 9, 1), new(P.MetroidEggRemnantsSettling, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(4)),
        [P.MetroidEggRemnantsSettling] = new([new(P.MetroidEggRemnantsSettling, 0, 8, 1), new(P.MetroidEggHatched, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
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

    /// <summary>
    /// The segment each segment that runs on into the one authored next, directly after its last beat, continues into: a
    /// sprite resting after that beat already stands in the next.
    /// </summary>
    internal static IReadOnlyDictionary<CinematicSpriteSegment, CinematicSpriteSegment> RunsOnInto { get; } =
        new Dictionary<CinematicSpriteSegment, CinematicSpriteSegment>
    {
        [P.IntroMotherBrainStartPage2] = P.IntroMotherBrainPageTwo,
        [P.MetroidEggShaking] = P.MetroidEggRemnantsSettling,
        [P.MetroidEggRemnantsSettling] = P.MetroidEggHatched,
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
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Show(CinematicSpritePose pose, int duration) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame(pose, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CinematicSpritePose, CinematicSpriteCue> Cue(CinematicSpriteCue cue) => new AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event(cue);
}
