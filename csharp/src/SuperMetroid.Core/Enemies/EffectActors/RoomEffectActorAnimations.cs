using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using K = SuperMetroid.Core.Enemies.EffectActors.RoomEffectActorKind;
using P = SuperMetroid.Core.Enemies.EffectActors.RoomEffectActorSegment;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// Every room effect actor's animation (#2275): each kind plays its segment once and stops, loops it, or, for a Metroid's
/// electricity and shell, runs a first pass on into a looping cycle, and a Yapping Maw's base holds a still picture. Each
/// beat shows a <see cref="RoomEffectActorPicture"/>: an enemy-sheet frame or, for the hit burst, one of Samus's burst
/// pictures (#1817).
/// </summary>
internal static class RoomEffectActorAnimations
{
    /// <summary>
    /// Every room effect actor's runs (#2275): one segment per kind, the beats that kind shows, and a Metroid's electricity's
    /// and shell's cycles, which their first pass runs on into.
    /// </summary>
    internal static AnimationSegments<RoomEffectActorSegment, RoomEffectActorPicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomEffectActorSegment, AnimationBeat<RoomEffectActorPicture, NoAnimationCue>[]>
    {
        [P.DraygonFoam] =
        [
            Show(EnemyFrameId.EffectActorDraygonFoamBubbling, 6),
            Show(EnemyFrameId.EffectActorDraygonFoamFrothing, 6),
            Show(EnemyFrameId.EffectActorDraygonFoamSwelling, 7),
            Show(EnemyFrameId.EffectActorDraygonFoamBillowing, 7),
            Show(EnemyFrameId.EffectActorDraygonFoamBreaking, 8),
            Show(EnemyFrameId.EffectActorDraygonFoamScattering, 8),
            Show(EnemyFrameId.EffectActorDraygonFoamDispersing, 9),
            Show(EnemyFrameId.EffectActorDraygonFoamVanishing, 9),
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
        [P.SmokePuff] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotPuff, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmoke, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRising, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRisen, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeFading, 5),
        ],
        [P.RisingDustPuff] =
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
        [P.DraygonBreathBubbles] =
        [
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesForming, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesSwelling, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesBillowing, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesRising, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesLifting, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesThinning, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesScattering, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesDispersing, 3),
            Show(EnemyFrameId.EffectActorDraygonBreathBubblesVanishing, 3),
        ],
        [P.NuclearWaffleBody] =
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
        [P.NuclearWaffleRightExplosion] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattered, 2),
        ],
        [P.NuclearWaffleLeftExplosion] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattered, 2),
        ],
        [P.NuclearWaffleSplash] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashSpurting, 4),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashStreaming, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashScattered, 2),
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
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 3),
            Show(EnemyFrameId.Empty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkHooked, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 5),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkZigzag, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 1),
            Show(EnemyFrameId.Empty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcKnotTip, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcFlat, 4),
            Show(EnemyFrameId.Empty, 7),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 3),
            Show(EnemyFrameId.Empty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltZigzag, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 3),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcArched, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcFlat, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcKnotted, 2),
            Show(EnemyFrameId.Empty, 7),
        ],
        [P.MetroidElectricityCycle] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 1),
            Show(EnemyFrameId.Empty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkForked, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkKinked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkSlanted, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkForked, 1),
            Show(EnemyFrameId.Empty, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 4),
            Show(EnemyFrameId.Empty, 32),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleKnotted, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 1),
            Show(EnemyFrameId.Empty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltBent, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltZigzag, 5),
            Show(EnemyFrameId.Empty, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 1),
            Show(EnemyFrameId.Empty, 6),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkZigzag, 2),
        ],
        [P.MetroidShell] =
        [
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellSwollen, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
        ],
        [P.MetroidShellCycle] =
        [
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellRounded, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.EffectActorMetroidShellPeaked, 1),
            Show(EnemyFrameId.Empty, 1),
        ],
        [P.YappingMawBaseFacingDown] = [Hold(EnemyFrameId.EffectActorYappingMawBaseFacingDown)],
        [P.YappingMawBaseFacingUp] = [Hold(EnemyFrameId.EffectActorYappingMawBaseFacingUp)],
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
        [P.HitBurst] =
        [
            Burst(HitBurstSpriteDefinitions.Tight, 2),
            Burst(HitBurstSpriteDefinitions.Opening, 2),
            Burst(HitBurstSpriteDefinitions.Wide, 2),
            Burst(HitBurstSpriteDefinitions.Scattered, 2),
        ],
    });

    /// <summary>
    /// The animation each kind plays: its segment once and done, or, for a Metroid's electricity and shell, a first pass
    /// and then its cycle looping (#2275); a Yapping Maw's base is a still picture.
    /// </summary>
    internal static IReadOnlyDictionary<RoomEffectActorKind, Animation<RoomEffectActorSegment>> StartingAt { get; } = new Dictionary<RoomEffectActorKind, Animation<RoomEffectActorSegment>>
    {
        [K.DraygonFoam] = new([Whole(P.DraygonFoam)], AnimationEnd.Stops),
        [K.SmallExplosion] = new([Whole(P.SmallExplosion)], AnimationEnd.Stops),
        [K.DudShot] = new([Whole(P.DudShot)], AnimationEnd.Stops),
        [K.SmokePuff] = new([Whole(P.SmokePuff)], AnimationEnd.Stops),
        [K.RisingDustPuff] = new([Whole(P.RisingDustPuff)], AnimationEnd.Stops),
        [K.Smoke] = new([Whole(P.Smoke)], AnimationEnd.Stops),
        [K.BigExplosion] = new([Whole(P.BigExplosion)], AnimationEnd.Stops),
        [K.ShortBigDustCloud] = new([Whole(P.ShortBigDustCloud)], AnimationEnd.Stops),
        [K.BigDustCloud] = new([Whole(P.BigDustCloud)], AnimationEnd.Stops),
        [K.DraygonBreathBubbles] = new([Whole(P.DraygonBreathBubbles)], AnimationEnd.Stops),
        [K.NuclearWaffleBody] = new([Whole(P.NuclearWaffleBody)], AnimationEnd.LoopsFrom(0)),
        [K.NuclearWaffleRightExplosion] = new([Whole(P.NuclearWaffleRightExplosion)], AnimationEnd.Stops),
        [K.NuclearWaffleLeftExplosion] = new([Whole(P.NuclearWaffleLeftExplosion)], AnimationEnd.Stops),
        [K.NuclearWaffleSplash] = new([Whole(P.NuclearWaffleSplash)], AnimationEnd.Stops),
        [K.FallingSparkTrail] = new([Whole(P.FallingSparkTrail)], AnimationEnd.Stops),
        [K.MetroidElectricity] = new([Whole(P.MetroidElectricity), Whole(P.MetroidElectricityCycle)], AnimationEnd.LoopsFrom(1)),
        [K.MetroidShell] = new([Whole(P.MetroidShell), Whole(P.MetroidShellCycle)], AnimationEnd.LoopsFrom(1)),
        [K.YappingMawBaseFacingDown] = new([Whole(P.YappingMawBaseFacingDown)], AnimationEnd.Holds),
        [K.YappingMawBaseFacingUp] = new([Whole(P.YappingMawBaseFacingUp)], AnimationEnd.Holds),
        [K.EvirFacingLeft] = new([Whole(P.EvirFacingLeft)], AnimationEnd.LoopsFrom(0)),
        [K.EvirFacingRight] = new([Whole(P.EvirFacingRight)], AnimationEnd.LoopsFrom(0)),
        [K.HitBurst] = new([Whole(P.HitBurst)], AnimationEnd.Stops),
    };

    /// <summary>The animation <paramref name="kind"/> plays from its spawn.</summary>
    internal static Animation<RoomEffectActorSegment> StartingWith(RoomEffectActorKind kind) => StartingAt[kind];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<RoomEffectActorSegment> Whole(RoomEffectActorSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): enemy-sheet frame <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue> Hold(EnemyFrameId frame) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.HeldFrame(RoomEffectActorPicture.OfEnemyFrame(frame));

    /// <summary>Shows enemy-sheet frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfEnemyFrame(frame), duration);

    /// <summary>Shows Samus's hit-burst sprite <paramref name="sprite"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue> Burst(ProjectileSpriteId sprite, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfHitBurst(sprite), duration);
}
