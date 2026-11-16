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
            Show(EnemyFrameId.EffectActorDraygonFoam9By8Risen0, 6),
            Show(EnemyFrameId.EffectActorDraygonFoam13By14Risen4, 6),
            Show(EnemyFrameId.EffectActorDraygonFoam15By14Risen8, 7),
            Show(EnemyFrameId.EffectActorDraygonFoam15By16Risen12, 7),
            Show(EnemyFrameId.EffectActorDraygonFoam15By15Risen16, 8),
            Show(EnemyFrameId.EffectActorDraygonFoam15By12Risen20, 8),
            Show(EnemyFrameId.EffectActorDraygonFoam14By9Risen24, 9),
            Show(EnemyFrameId.EffectActorDraygonFoam13By9Risen28, 9),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyFrameId.EffectActorSmallExplosionSpark, 4),
            Show(EnemyFrameId.EffectActorSmallExplosionFireball, 6),
            Show(EnemyFrameId.EffectActorSmallExplosionFlash, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionBurst, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionRing, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionScattering, 6),
        ],
        [P.DudShot] =
        [
            Show(EnemyFrameId.EffectActorDudShotDot, 4),
            Show(EnemyFrameId.EffectActorDudShotSpark, 4),
            Show(EnemyFrameId.EffectActorDudShotFlash, 4),
            Show(EnemyFrameId.EffectActorDudShotRing, 4),
            Show(EnemyFrameId.EffectActorDudShotThinRing, 4),
            Show(EnemyFrameId.EffectActorDudShotFadingRing, 4),
        ],
        [P.SmallDudShot] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotPuff, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmoke, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRising, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRisen, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeFading, 5),
        ],
        [P.SpacePirateLandingDustCloud] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotPuff, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuff, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuffRising, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuffRisen, 3),
        ],
        [P.Smoke] =
        [
            Show(EnemyFrameId.EffectActorSmokeLow, 8),
            Show(EnemyFrameId.EffectActorSmokeRising, 8),
            Show(EnemyFrameId.EffectActorSmokeRisen, 8),
            Show(EnemyFrameId.EffectActorSmokeFading, 8),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyFrameId.EffectActorBigExplosionFireball, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFlash, 5),
            Show(EnemyFrameId.EffectActorBigExplosionBurst, 5),
            Show(EnemyFrameId.EffectActorBigExplosionRing, 5),
            Show(EnemyFrameId.EffectActorBigExplosionWideRing, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFadingRing, 5),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyFrameId.EffectActorBigDustCloudPuff, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudSwelling, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudSwollen, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudBillowing, 2),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyFrameId.EffectActorBigDustCloudPuff, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudSwelling, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudSwollen, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudBillowing, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudFull, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudThinning, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudFading, 5),
            Show(EnemyFrameId.EffectActorBigDustCloudWisps, 5),
        ],
        [P.ShortDraygonBreathBubbles] =
        [
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles6By7Risen0, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles12By12Risen6, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles14By13Risen8, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles12By14Risen11, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles12By14Risen14, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles10By13Risen15, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles10By12Risen16, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles8By10Risen16, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubbles4By6Risen14, 3),
        ],
        [P.PuromiBody] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned0, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned45, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned90, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned135, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned180, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned225, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned0, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned45, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned270, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned135, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned180, 3),
            Show(EnemyFrameId.EffectActorNuclearWaffleBodyTurned315, 3),
        ],
        [P.PuromiRightExplosion] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattered, 2),
        ],
        [P.PuromiLeftExplosion] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattered, 2),
        ],
        [P.PuromiSplash] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash25Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash25Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash29Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash39Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash45Wide, 2),
        ],
        [P.FallingSparkTrail] =
        [
            Show(EnemyFrameId.EffectActorFallingSparkTrailBright, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailTwinkling, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailDimming, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailFading, 3),
        ],
        [P.MetroidElectricity] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc22By22Anchored16By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored16By20, 3),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By18Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By21Anchored8By20, 5),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By22Anchored8By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By21Anchored8By20, 1),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By7Anchored16By4, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc8By8Anchored16By4, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By6Anchored16By4, 4),
            Show(EnemyFrameId.EffectActorEmpty, 7),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By18Anchored16By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By22Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc20By21Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By22Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc20By21Anchored16By20, 3),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc7By12Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc8By14Anchored8By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc7By14Anchored8By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc7By12Anchored8By20, 3),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By7Anchored0By4, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By8Anchored0By4, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By6Anchored0By4, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc16By8Anchored0By4, 2),
            Show(EnemyFrameId.EffectActorEmpty, 7),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored8By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc22By22Anchored8By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored8By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored8By20, 1),
            Show(EnemyFrameId.EffectActorEmpty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc4By6Anchored0By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc3By6Anchored0By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc4By7Anchored0By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc4By6Anchored0By20, 1),
            Show(EnemyFrameId.EffectActorEmpty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored8By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorEmpty, 32),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc22By22Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By23Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By21Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc7By12Anchored8By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc8By14Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc7By14Anchored8By20, 5),
            Show(EnemyFrameId.EffectActorEmpty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By18Anchored16By20, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By22Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc20By21Anchored16By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc19By18Anchored16By20, 1),
            Show(EnemyFrameId.EffectActorEmpty, 6),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By18Anchored8By20, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc24By21Anchored8By20, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityArc23By22Anchored8By20, 2),
        ],
        [P.MetroidShell] =
        [
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell44By29, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By27, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
            Show(EnemyFrameId.EffectActorEmpty, 1),
            Show(EnemyFrameId.EffectActorMetroidShell38By28, 1),
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
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsStretched, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsLowered, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsTucked, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsLowering, 10),
        ],
        [P.EvirFacingRight] =
        [
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsStretched, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsLowered, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsTucked, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsLowering, 10),
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
        [P.MetroidElectricity] = new([new(P.MetroidElectricity, 0, 31, 1), new(P.MetroidElectricity, 31, 31, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MetroidShell] = new([new(P.MetroidShell, 0, 32, 1), new(P.MetroidShell, 32, 30, 1)], AnimationEnd.LoopsFrom(1)),
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
