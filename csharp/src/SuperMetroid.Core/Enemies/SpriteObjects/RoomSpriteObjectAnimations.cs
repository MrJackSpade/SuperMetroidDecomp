using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using P = SuperMetroid.Core.Enemies.SpriteObjects.RoomSpriteObjectSegment;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Every room sprite-object animation the named object kinds reach, as a typed animation: each timed beat shows a
/// <see cref="RoomSpriteObjectPicture"/>, an enemy-sheet frame or, for the hit burst, one of Samus's burst sprites
/// (#1817); a hold keeps the last frame showing, deletion stops the animation, and a jump continues at another
/// animation.
/// </summary>
internal static class RoomSpriteObjectAnimations
{
    /// <summary>Every room sprite-object animation.</summary>
    internal static AnimationSegments<RoomSpriteObjectSegment, RoomSpriteObjectPicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomSpriteObjectSegment, AnimationBeat<RoomSpriteObjectPicture, NoAnimationCue>[]>
    {
        [P.DraygonFoamingAtTheMouth] =
        [
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthFirst, 6),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthSecond, 6),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthThird, 7),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthFourth, 7),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthFifth, 8),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthSixth, 8),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthSeventh, 9),
            Show(EnemyFrameId.SpriteObjectDraygonFoamingAtTheMouthEighth, 9),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyFrameId.SpriteObjectSmallExplosionFirst, 4),
            Show(EnemyFrameId.SpriteObjectSmallExplosionSecond, 6),
            Show(EnemyFrameId.SpriteObjectSmallExplosionThird, 5),
            Show(EnemyFrameId.SpriteObjectSmallExplosionFourth, 5),
            Show(EnemyFrameId.SpriteObjectSmallExplosionFifth, 5),
            Show(EnemyFrameId.SpriteObjectSmallExplosionSixth, 6),
        ],
        [P.DudShot] =
        [
            Show(EnemyFrameId.SpriteObjectDudShotFirst, 4),
            Show(EnemyFrameId.SpriteObjectDudShotSecond, 4),
            Show(EnemyFrameId.SpriteObjectDudShotThird, 4),
            Show(EnemyFrameId.SpriteObjectDudShotFourth, 4),
            Show(EnemyFrameId.SpriteObjectDudShotFifth, 4),
            Show(EnemyFrameId.SpriteObjectDudShotSixth, 4),
        ],
        [P.SmallDudShot] =
        [
            Show(EnemyFrameId.SpriteObjectSmallDudShotFirst, 5),
            Show(EnemyFrameId.SpriteObjectSmallDudShotSecond, 5),
            Show(EnemyFrameId.SpriteObjectSmallDudShotThird, 5),
            Show(EnemyFrameId.SpriteObjectSmallDudShotFourth, 5),
            Show(EnemyFrameId.SpriteObjectSmallDudShotFifth, 5),
        ],
        [P.SpacePirateLandingDustCloud] =
        [
            Show(EnemyFrameId.SpriteObjectSmallDudShotFirst, 3),
            Show(EnemyFrameId.SpriteObjectSpacePirateLandingDustCloudSecond, 3),
            Show(EnemyFrameId.SpriteObjectSpacePirateLandingDustCloudThird, 3),
            Show(EnemyFrameId.SpriteObjectSpacePirateLandingDustCloudFourth, 3),
        ],
        [P.Smoke] =
        [
            Show(EnemyFrameId.SpriteObjectSmokeFirst, 8),
            Show(EnemyFrameId.SpriteObjectSmokeSecond, 8),
            Show(EnemyFrameId.SpriteObjectSmokeThird, 8),
            Show(EnemyFrameId.SpriteObjectSmokeFourth, 8),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyFrameId.SpriteObjectBigExplosionFirst, 5),
            Show(EnemyFrameId.SpriteObjectBigExplosionSecond, 5),
            Show(EnemyFrameId.SpriteObjectBigExplosionThird, 5),
            Show(EnemyFrameId.SpriteObjectBigExplosionFourth, 5),
            Show(EnemyFrameId.SpriteObjectBigExplosionFifth, 5),
            Show(EnemyFrameId.SpriteObjectBigExplosionSixth, 5),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudFirst, 2),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudSecond, 2),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudThird, 2),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudFourth, 2),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudFirst, 5),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudSecond, 5),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudThird, 5),
            Show(EnemyFrameId.SpriteObjectShortBigDustCloudFourth, 5),
            Show(EnemyFrameId.SpriteObjectBigDustCloudFifth, 5),
            Show(EnemyFrameId.SpriteObjectBigDustCloudSixth, 5),
            Show(EnemyFrameId.SpriteObjectBigDustCloudSeventh, 5),
            Show(EnemyFrameId.SpriteObjectBigDustCloudEighth, 5),
        ],
        [P.ShortDraygonBreathBubbles] =
        [
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesFirst, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesSecond, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesThird, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesFourth, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesFifth, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesSixth, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesSeventh, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesEighth, 3),
            Show(EnemyFrameId.SpriteObjectShortDraygonBreathBubblesNinth, 3),
        ],
        [P.PuromiBody] =
        [
            Show(EnemyFrameId.SpriteObjectPuromiBodyFirst, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodySecond, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyThird, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyFourth, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyFifth, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodySixth, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyFirst, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodySecond, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodySeventh, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyFourth, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyFifth, 3),
            Show(EnemyFrameId.SpriteObjectPuromiBodyEighth, 3),
        ],
        [P.PuromiRightExplosion] =
        [
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionFirst, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionSecond, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionThird, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionFourth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionFifth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionSixth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiRightExplosionSeventh, 2),
        ],
        [P.PuromiLeftExplosion] =
        [
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionFirst, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionSecond, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionThird, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionFourth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionFifth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionSixth, 2),
            Show(EnemyFrameId.SpriteObjectPuromiLeftExplosionSeventh, 2),
        ],
        [P.PuromiSplash] =
        [
            Show(EnemyFrameId.SpriteObjectPuromiSplashFirst, 2),
            Show(EnemyFrameId.SpriteObjectPuromiSplashFirst, 2),
            Show(EnemyFrameId.SpriteObjectPuromiSplashSecond, 2),
            Show(EnemyFrameId.SpriteObjectPuromiSplashThird, 2),
            Show(EnemyFrameId.SpriteObjectPuromiSplashFourth, 2),
        ],
        [P.FallingSparkTrail] =
        [
            Show(EnemyFrameId.SpriteObjectFallingSparkTrailFirst, 3),
            Show(EnemyFrameId.SpriteObjectFallingSparkTrailSecond, 3),
            Show(EnemyFrameId.SpriteObjectFallingSparkTrailThird, 3),
            Show(EnemyFrameId.SpriteObjectFallingSparkTrailFourth, 3),
        ],
        [P.MetroidElectricity] =
        [
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFirst, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySecond, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThird, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFirst, 1),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThird, 3),
            Show(EnemyFrameId.SpriteObjectEmpty, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFifth, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySixth, 5),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySeventh, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySixth, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEighth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityNinth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityTenth, 4),
            Show(EnemyFrameId.SpriteObjectEmpty, 7),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEleventh, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityTwelfth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThirteenth, 1),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityTwelfth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThirteenth, 3),
            Show(EnemyFrameId.SpriteObjectEmpty, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFourteenth, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFifteenth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySixteenth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFourteenth, 3),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySeventeenth, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEighteenth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityNineteenth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEighteenth, 2),
            Show(EnemyFrameId.SpriteObjectEmpty, 7),
        ],
        [P.MetroidElectricityLoop] =
        [
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopFirst, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopSecond, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopThird, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopFirst, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopFifth, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopSixth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopSeventh, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopFifth, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopFirst, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityLoopThird, 4),
            Show(EnemyFrameId.SpriteObjectEmpty, 32),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFirst, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySecond, 1),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThird, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFourteenth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFifteenth, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySixteenth, 5),
            Show(EnemyFrameId.SpriteObjectEmpty, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEleventh, 2),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityTwelfth, 1),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityThirteenth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityEleventh, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 6),
            Show(EnemyFrameId.SpriteObjectMetroidElectricityFifth, 3),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySixth, 4),
            Show(EnemyFrameId.SpriteObjectMetroidElectricitySeventh, 2),
        ],
        [P.MetroidShell] =
        [
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
        ],
        [P.MetroidShellLoop] =
        [
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellFirst, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
            Show(EnemyFrameId.SpriteObjectMetroidShellLoopThird, 1),
            Show(EnemyFrameId.SpriteObjectEmpty, 1),
        ],
        [P.YappingMawBaseFacingDown] =
        [
            Show(EnemyFrameId.SpriteObjectYappingMawBaseFacingDown, 1),
        ],
        [P.YappingMawBaseFacingUp] =
        [
            Show(EnemyFrameId.SpriteObjectYappingMawBaseFacingUp, 1),
        ],
        [P.EvirFacingLeft] =
        [
            Show(EnemyFrameId.SpriteObjectEvirFacingLeftFirst, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingLeftSecond, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingLeftThird, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingLeftFourth, 10),
        ],
        [P.EvirFacingRight] =
        [
            Show(EnemyFrameId.SpriteObjectEvirFacingRightFirst, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingRightSecond, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingRightThird, 10),
            Show(EnemyFrameId.SpriteObjectEvirFacingRightFourth, 10),
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
    internal static IReadOnlyDictionary<RoomSpriteObjectSegment, Animation<RoomSpriteObjectSegment>> StartingAt { get; } = new Dictionary<RoomSpriteObjectSegment, Animation<RoomSpriteObjectSegment>>
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
    internal static Animation<RoomSpriteObjectSegment> StartingWith(RoomSpriteObjectSegment segment) => StartingAt[segment];

    /// <summary>Shows enemy-sheet frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomSpriteObjectPicture, NoAnimationCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<RoomSpriteObjectPicture, NoAnimationCue>.Frame(RoomSpriteObjectPicture.OfEnemyFrame(frame), duration);

    /// <summary>Shows Samus's hit-burst sprite <paramref name="sprite"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomSpriteObjectPicture, NoAnimationCue> Burst(ProjectileSpriteId sprite, int duration) =>
        new AnimationBeat<RoomSpriteObjectPicture, NoAnimationCue>.Frame(RoomSpriteObjectPicture.OfHitBurst(sprite), duration);
}
