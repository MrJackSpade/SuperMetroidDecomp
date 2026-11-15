using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using P = SuperMetroid.Core.Enemies.EffectActors.RoomEffectActorSegment;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// Every room effect-actor animation the named object kinds reach, as a typed animation: each timed beat shows a
/// <see cref="RoomEffectActorPicture"/>, an enemy-sheet frame or, for the hit burst, one of Samus's burst sprites
/// (#1817); a hold keeps the last frame showing, deletion stops the animation, and a jump continues at another
/// animation.
/// </summary>
internal static class RoomEffectActorAnimations
{
    /// <summary>Every room effect-actor animation.</summary>
    internal static AnimationSegments<RoomEffectActorSegment, RoomEffectActorPicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomEffectActorSegment, AnimationBeat<RoomEffectActorPicture, NoAnimationCue>[]>
    {
        [P.DraygonFoamingAtTheMouth] =
        [
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthFirst, 6),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthSecond, 6),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthThird, 7),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthFourth, 7),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthFifth, 8),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthSixth, 8),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthSeventh, 9),
            Show(EnemyFrameId.EffectActorDraygonFoamingAtTheMouthEighth, 9),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyFrameId.EffectActorSmallExplosionFirst, 4),
            Show(EnemyFrameId.EffectActorSmallExplosionSecond, 6),
            Show(EnemyFrameId.EffectActorSmallExplosionThird, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionFourth, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionFifth, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionSixth, 6),
        ],
        [P.DudShot] =
        [
            Show(EnemyFrameId.EffectActorDudShotFirst, 4),
            Show(EnemyFrameId.EffectActorDudShotSecond, 4),
            Show(EnemyFrameId.EffectActorDudShotThird, 4),
            Show(EnemyFrameId.EffectActorDudShotFourth, 4),
            Show(EnemyFrameId.EffectActorDudShotFifth, 4),
            Show(EnemyFrameId.EffectActorDudShotSixth, 4),
        ],
        [P.SmallDudShot] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotFirst, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSecond, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotThird, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotFourth, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotFifth, 5),
        ],
        [P.SpacePirateLandingDustCloud] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotFirst, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustCloudSecond, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustCloudThird, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustCloudFourth, 3),
        ],
        [P.Smoke] =
        [
            Show(EnemyFrameId.EffectActorSmokeFirst, 8),
            Show(EnemyFrameId.EffectActorSmokeSecond, 8),
            Show(EnemyFrameId.EffectActorSmokeThird, 8),
            Show(EnemyFrameId.EffectActorSmokeFourth, 8),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyFrameId.EffectActorBigExplosionFirst, 5),
            Show(EnemyFrameId.EffectActorBigExplosionSecond, 5),
            Show(EnemyFrameId.EffectActorBigExplosionThird, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFourth, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFifth, 5),
            Show(EnemyFrameId.EffectActorBigExplosionSixth, 5),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyFrameId.EffectActorShortBigDustCloudFirst, 2),
            Show(EnemyFrameId.EffectActorShortBigDustCloudSecond, 2),
            Show(EnemyFrameId.EffectActorShortBigDustCloudThird, 2),
            Show(EnemyFrameId.EffectActorShortBigDustCloudFourth, 2),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyFrameId.EffectActorShortBigDustCloudFirst, 5),
            Show(EnemyFrameId.EffectActorShortBigDustCloudSecond, 5),
            Show(EnemyFrameId.EffectActorShortBigDustCloudThird, 5),
            Show(EnemyFrameId.EffectActorShortBigDustCloudFourth, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudFifth, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudSixth, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudSeventh, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudEighth, 5),
        ],
        [P.ShortDraygonBreathBubbles] =
        [
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesFirst, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesSecond, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesThird, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesFourth, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesFifth, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesSixth, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesSeventh, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesEighth, 3),
            Show(EnemyFrameId.EffectActorShortDraygonBreathBubblesNinth, 3),
        ],
        [P.PuromiBody] =
        [
            Show(EnemyFrameId.EffectActorPuromiBodyFirst, 3),
            Show(EnemyFrameId.EffectActorPuromiBodySecond, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyThird, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyFourth, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyFifth, 3),
            Show(EnemyFrameId.EffectActorPuromiBodySixth, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyFirst, 3),
            Show(EnemyFrameId.EffectActorPuromiBodySecond, 3),
            Show(EnemyFrameId.EffectActorPuromiBodySeventh, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyFourth, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyFifth, 3),
            Show(EnemyFrameId.EffectActorPuromiBodyEighth, 3),
        ],
        [P.PuromiRightExplosion] =
        [
            Show(EnemyFrameId.EffectActorPuromiRightExplosionFirst, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionSecond, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionThird, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionFourth, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionFifth, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionSixth, 2),
            Show(EnemyFrameId.EffectActorPuromiRightExplosionSeventh, 2),
        ],
        [P.PuromiLeftExplosion] =
        [
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionFirst, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionSecond, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionThird, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionFourth, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionFifth, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionSixth, 2),
            Show(EnemyFrameId.EffectActorPuromiLeftExplosionSeventh, 2),
        ],
        [P.PuromiSplash] =
        [
            Show(EnemyFrameId.EffectActorPuromiSplashFirst, 2),
            Show(EnemyFrameId.EffectActorPuromiSplashFirst, 2),
            Show(EnemyFrameId.EffectActorPuromiSplashSecond, 2),
            Show(EnemyFrameId.EffectActorPuromiSplashThird, 2),
            Show(EnemyFrameId.EffectActorPuromiSplashFourth, 2),
        ],
        [P.FallingSparkTrail] =
        [
            Show(EnemyFrameId.EffectActorFallingSparkTrailFirst, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailSecond, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailThird, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailFourth, 3),
        ],
        [P.MetroidElectricity] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityFirst, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricitySecond, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityThird, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityFirst, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityThird, 3),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityFifth, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricitySixth, 5),
            Show(EnemyFrameId.EffectActorMetroidElectricitySeventh, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricitySixth, 1),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityEighth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityNinth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityTenth, 4),
            Show(EnemyFrameId.EffectActorEmpty, 7),
            Show(EnemyFrameId.EffectActorMetroidElectricityEleventh, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityTwelfth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityThirteenth, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityTwelfth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityThirteenth, 3),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityFourteenth, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityFifteenth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricitySixteenth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityFourteenth, 3),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricitySeventeenth, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityEighteenth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityNineteenth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityEighteenth, 2),
            Show(EnemyFrameId.EffectActorEmpty, 7),
        ],
        [P.MetroidElectricityLoop] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopFirst, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopSecond, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopThird, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopFirst, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopFifth, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopSixth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopSeventh, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopFifth, 1),
            Show(EnemyFrameId.EffectActorEmpty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopFirst, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLoopThird, 4),
            Show(EnemyFrameId.EffectActorEmpty, 32),
            Show(EnemyFrameId.EffectActorMetroidElectricityFirst, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricitySecond, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityThird, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityFourteenth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityFifteenth, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricitySixteenth, 5),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityEleventh, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityTwelfth, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityThirteenth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityEleventh, 1),
            Show(EnemyFrameId.EffectActorEmpty, 6),
            Show(EnemyFrameId.EffectActorMetroidElectricityFifth, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricitySixth, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricitySeventh, 2),
        ],
        [P.MetroidShell] =
        [
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
        ],
        [P.MetroidShellLoop] =
        [
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellFirst, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellLoopThird, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
        ],
        [P.YappingMawBaseFacingDown] =
        [
            Show(EnemyFrameId.EffectActorYappingMawBaseFacingDown, 1),
        ],
        [P.YappingMawBaseFacingUp] =
        [
            Show(EnemyFrameId.EffectActorYappingMawBaseFacingUp, 1),
        ],
        [P.EvirFacingLeft] =
        [
            Show(EnemyFrameId.EffectActorEvirFacingLeftFirst, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftSecond, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftThird, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftFourth, 10),
        ],
        [P.EvirFacingRight] =
        [
            Show(EnemyFrameId.EffectActorEvirFacingRightFirst, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightSecond, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightThird, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightFourth, 10),
        ],
        [P.EnemyHit] =
        [
            Burst(HitBurstSpriteDefinitions.Tight, 2),
            Burst(HitBurstSpriteDefinitions.Opening, 2),
            Burst(HitBurstSpriteDefinitions.Wide, 2),
            Burst(HitBurstSpriteDefinitions.Scattered, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RoomEffectActorSegment, Animation<RoomEffectActorSegment>> StartingAt { get; } = new Dictionary<RoomEffectActorSegment, Animation<RoomEffectActorSegment>>
    {
        [P.DraygonFoamingAtTheMouth] = new([new(P.DraygonFoamingAtTheMouth, 0, 8, 1)], AnimationEnd.Stops),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.DudShot] = new([new(P.DudShot, 0, 6, 1)], AnimationEnd.Stops),
        [P.SmallDudShot] = new([new(P.SmallDudShot, 0, 5, 1)], AnimationEnd.Stops),
        [P.SpacePirateLandingDustCloud] = new([new(P.SpacePirateLandingDustCloud, 0, 4, 1)], AnimationEnd.Stops),
        [P.Smoke] = new([new(P.Smoke, 0, 4, 1)], AnimationEnd.Stops),
        [P.BigExplosion] = new([new(P.BigExplosion, 0, 6, 1)], AnimationEnd.Stops),
        [P.ShortBigDustCloud] = new([new(P.ShortBigDustCloud, 0, 4, 1)], AnimationEnd.Stops),
        [P.BigDustCloud] = new([new(P.BigDustCloud, 0, 8, 1)], AnimationEnd.Stops),
        [P.ShortDraygonBreathBubbles] = new([new(P.ShortDraygonBreathBubbles, 0, 9, 1)], AnimationEnd.Stops),
        [P.PuromiBody] = new([new(P.PuromiBody, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PuromiRightExplosion] = new([new(P.PuromiRightExplosion, 0, 7, 1)], AnimationEnd.Stops),
        [P.PuromiLeftExplosion] = new([new(P.PuromiLeftExplosion, 0, 7, 1)], AnimationEnd.Stops),
        [P.PuromiSplash] = new([new(P.PuromiSplash, 0, 5, 1)], AnimationEnd.Stops),
        [P.FallingSparkTrail] = new([new(P.FallingSparkTrail, 0, 4, 1)], AnimationEnd.Stops),
        [P.MetroidElectricity] = new([new(P.MetroidElectricity, 0, 31, 1), new(P.MetroidElectricityLoop, 0, 31, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidElectricityLoop] = new([new(P.MetroidElectricityLoop, 0, 31, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MetroidShell] = new([new(P.MetroidShell, 0, 32, 1), new(P.MetroidShellLoop, 0, 30, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidShellLoop] = new([new(P.MetroidShellLoop, 0, 30, 1)], AnimationEnd.LoopsFrom(0)),
        [P.YappingMawBaseFacingDown] = new([new(P.YappingMawBaseFacingDown, 0, 1, 1)], AnimationEnd.Holds),
        [P.YappingMawBaseFacingUp] = new([new(P.YappingMawBaseFacingUp, 0, 1, 1)], AnimationEnd.Holds),
        [P.EvirFacingLeft] = new([new(P.EvirFacingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EvirFacingRight] = new([new(P.EvirFacingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EnemyHit] = new([new(P.EnemyHit, 0, 4, 1)], AnimationEnd.Stops),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RoomEffectActorSegment> StartingWith(RoomEffectActorSegment segment) => StartingAt[segment];

    /// <summary>Shows enemy-sheet frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfEnemyFrame(frame), duration);

    /// <summary>Shows Samus's hit-burst sprite <paramref name="sprite"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue> Burst(ProjectileSpriteId sprite, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfHitBurst(sprite), duration);
}
