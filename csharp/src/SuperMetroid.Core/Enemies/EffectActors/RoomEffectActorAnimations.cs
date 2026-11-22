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
internal static partial class RoomEffectActorAnimations
{
    /// <summary>How many times a Metroid's shell flickers at its usual size as its first pass starts.</summary>
    private const int ShellFirstFlickers = 7;

    /// <summary>How many times a Metroid's shell flickers swollen in its first pass, before it settles back once.</summary>
    private const int SwollenShellFlickers = 8;

    /// <summary>How many times a Metroid's shell flickers at its usual size in each cycle.</summary>
    private const int ShellCycleFlickers = 6;

    /// <summary>How many times a Metroid's shell flickers stretched a pixel taller in each cycle.</summary>
    private const int StretchedShellCycleFlickers = 9;

    /// <summary>
    /// Every room effect actor's runs (#2275, #2369): the run each actor starts with, the beats it shows, and a Metroid's
    /// electricity's and shell's cycles, which their first pass runs on into.
    /// </summary>
    internal static UnpausedAnimationSegments<RoomEffectActorSegment, RoomEffectActorPicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomEffectActorSegment, AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused[]>
    {
        [P.Of(K.DraygonFoam)] =
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
        [P.Of(K.SmallExplosion)] =
        [
            Show(EnemyFrameId.EffectActorSmallExplosionSpark, 4),
            Show(EnemyFrameId.EffectActorSmallExplosionFireball, 6),
            Show(EnemyFrameId.EffectActorSmallExplosionFlash, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionBurst, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionRing, 5),
            Show(EnemyFrameId.EffectActorSmallExplosionScattering, 6),
        ],
        [P.Of(K.DudShot)] =
        [
            Show(EnemyFrameId.EffectActorDudShotDot, 4),
            Show(EnemyFrameId.EffectActorDudShotSpark, 4),
            Show(EnemyFrameId.EffectActorDudShotFlash, 4),
            Show(EnemyFrameId.EffectActorDudShotRing, 4),
            Show(EnemyFrameId.EffectActorDudShotThinRing, 4),
            Show(EnemyFrameId.EffectActorDudShotFadingRing, 4),
        ],
        [P.Of(K.SmokePuff)] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotPuff, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmoke, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRising, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeRisen, 5),
            Show(EnemyFrameId.EffectActorSmallDudShotSmokeFading, 5),
        ],
        [P.Of(K.RisingDustPuff)] =
        [
            Show(EnemyFrameId.EffectActorSmallDudShotPuff, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuff, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuffRising, 3),
            Show(EnemyFrameId.EffectActorSpacePirateLandingDustPuffRisen, 3),
        ],
        [P.Of(K.Smoke)] =
        [
            Show(EnemyFrameId.EffectActorSmokeLow, 8),
            Show(EnemyFrameId.EffectActorSmokeRising, 8),
            Show(EnemyFrameId.EffectActorSmokeRisen, 8),
            Show(EnemyFrameId.EffectActorSmokeFading, 8),
        ],
        [P.Of(K.BigExplosion)] =
        [
            Show(EnemyFrameId.EffectActorBigExplosionFireball, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFlash, 5),
            Show(EnemyFrameId.EffectActorBigExplosionBurst, 5),
            Show(EnemyFrameId.EffectActorBigExplosionRing, 5),
            Show(EnemyFrameId.EffectActorBigExplosionWideRing, 5),
            Show(EnemyFrameId.EffectActorBigExplosionFadingRing, 5),
        ],
        [P.Of(K.ShortBigDustCloud)] =
        [
            Show(EnemyFrameId.EffectActorBigDustCloudPuff, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudSwelling, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudSwollen, 2),
            Show(EnemyFrameId.EffectActorBigDustCloudBillowing, 2),
        ],
        [P.Of(K.BigDustCloud)] =
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
        [P.Of(K.DraygonBreathBubbles)] =
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
        [P.Of(K.NuclearWaffleBody)] =
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
        [P.Of(K.NuclearWaffleRightExplosion)] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleRightExplosionScattered, 2),
        ],
        [P.Of(K.NuclearWaffleLeftExplosion)] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionEmber, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBall, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionSwelling, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionGlowing, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattering, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleLeftExplosionScattered, 2),
        ],
        [P.Of(K.NuclearWaffleSplash)] =
        [
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashSpurting, 4),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashStreaming, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashBursting, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplashScattered, 2),
        ],
        [P.Of(K.FallingSparkTrail)] =
        [
            Show(EnemyFrameId.EffectActorFallingSparkTrailBright, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailTwinkling, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailDimming, 3),
            Show(EnemyFrameId.EffectActorFallingSparkTrailFading, 3),
        ],
        [P.Of(K.MetroidElectricity)] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 3),
            Show(EnemyFrameId.PointAtOrigin, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkHooked, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 5),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkZigzag, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 1),
            Show(EnemyFrameId.PointAtOrigin, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcKnotTip, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityLeftArcFlat, 4),
            Show(EnemyFrameId.PointAtOrigin, 7),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 3),
            Show(EnemyFrameId.PointAtOrigin, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltBent, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltZigzag, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 3),
            Show(EnemyFrameId.PointAtOrigin, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcArched, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcFlat, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityRightArcKnotted, 2),
            Show(EnemyFrameId.PointAtOrigin, 7),
        ],
        [P.MetroidElectricityCycle] =
        [
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleKnotted, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 1),
            Show(EnemyFrameId.PointAtOrigin, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkForked, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkKinked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkSlanted, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadSparkForked, 1),
            Show(EnemyFrameId.PointAtOrigin, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleArched, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightCrackleFlat, 4),
            Show(EnemyFrameId.PointAtOrigin, 32),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleKnotted, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleFlat, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftCrackleArched, 1),
            Show(EnemyFrameId.PointAtOrigin, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltBent, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityOverheadBoltZigzag, 5),
            Show(EnemyFrameId.PointAtOrigin, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 2),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkBent, 1),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkZigzag, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperLeftForkHooked, 1),
            Show(EnemyFrameId.PointAtOrigin, 6),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkHooked, 3),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkBent, 4),
            Show(EnemyFrameId.EffectActorMetroidElectricityUpperRightForkZigzag, 2),
        ],
        [P.Of(K.MetroidShell)] =
        [
            .. Flickering(EnemyFrameId.EffectActorMetroidShellRounded, ShellFirstFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShellSwollen, SwollenShellFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShellRounded, 1),
        ],
        [P.MetroidShellCycle] =
        [
            .. Flickering(EnemyFrameId.EffectActorMetroidShellRounded, ShellCycleFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShellPeaked, StretchedShellCycleFlickers),
        ],
        [P.Of(K.YappingMawBaseFacingDown)] = [Hold(EnemyFrameId.EffectActorYappingMawBaseFacingDown)],
        [P.Of(K.YappingMawBaseFacingUp)] = [Hold(EnemyFrameId.EffectActorYappingMawBaseFacingUp)],
        [P.Of(K.EvirFacingLeft)] =
        [
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsStretched, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsLowered, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsTucked, 10),
            Show(EnemyFrameId.EffectActorEvirFacingLeftLegsLowering, 10),
        ],
        [P.Of(K.EvirFacingRight)] =
        [
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsStretched, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsLowered, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsTucked, 10),
            Show(EnemyFrameId.EffectActorEvirFacingRightLegsLowering, 10),
        ],
        [P.Of(K.HitBurst)] =
        [
            Burst(HitBurstSpriteDefinitions.Tight, 2),
            Burst(HitBurstSpriteDefinitions.Opening, 2),
            Burst(HitBurstSpriteDefinitions.Wide, 2),
            Burst(HitBurstSpriteDefinitions.Scattered, 2),
        ],
    }, RoomEffectActorSegment.All);

    /// <summary>
    /// The animation each actor plays from its spawn (#2275, #2369): its own run once and done, its own run looping or its
    /// still picture held; a Metroid's electricity and shell play a first pass and then loop their cycle.
    /// </summary>
    internal static IReadOnlyDictionary<RoomEffectActorKind, Animation<RoomEffectActorSegment>> StartingAt { get; } = new Dictionary<RoomEffectActorKind, Animation<RoomEffectActorSegment>>
    {
        [K.DraygonFoam] = Once(K.DraygonFoam),
        [K.SmallExplosion] = Once(K.SmallExplosion),
        [K.DudShot] = Once(K.DudShot),
        [K.SmokePuff] = Once(K.SmokePuff),
        [K.RisingDustPuff] = Once(K.RisingDustPuff),
        [K.Smoke] = Once(K.Smoke),
        [K.BigExplosion] = Once(K.BigExplosion),
        [K.ShortBigDustCloud] = Once(K.ShortBigDustCloud),
        [K.BigDustCloud] = Once(K.BigDustCloud),
        [K.DraygonBreathBubbles] = Once(K.DraygonBreathBubbles),
        [K.NuclearWaffleBody] = Looping(K.NuclearWaffleBody),
        [K.NuclearWaffleRightExplosion] = Once(K.NuclearWaffleRightExplosion),
        [K.NuclearWaffleLeftExplosion] = Once(K.NuclearWaffleLeftExplosion),
        [K.NuclearWaffleSplash] = Once(K.NuclearWaffleSplash),
        [K.FallingSparkTrail] = Once(K.FallingSparkTrail),
        [K.MetroidElectricity] = Animation<RoomEffectActorSegment>.LeadingInto([Segments.Whole(P.Of(K.MetroidElectricity))], [Segments.Whole(P.MetroidElectricityCycle)]),
        [K.MetroidShell] = Animation<RoomEffectActorSegment>.LeadingInto([Segments.Whole(P.Of(K.MetroidShell))], [Segments.Whole(P.MetroidShellCycle)]),
        [K.YappingMawBaseFacingDown] = Still(K.YappingMawBaseFacingDown),
        [K.YappingMawBaseFacingUp] = Still(K.YappingMawBaseFacingUp),
        [K.EvirFacingLeft] = Looping(K.EvirFacingLeft),
        [K.EvirFacingRight] = Looping(K.EvirFacingRight),
        [K.HitBurst] = Once(K.HitBurst),
    };

    /// <summary>The animation <paramref name="kind"/> plays from its spawn.</summary>
    internal static Animation<RoomEffectActorSegment> StartingWith(RoomEffectActorKind kind) => StartingAt[kind];

    /// <summary><paramref name="actor"/>'s own run, played once before the actor ends.</summary>
    private static Animation<RoomEffectActorSegment> Once(RoomEffectActorKind actor) => new([Segments.Whole(P.Of(actor))], AnimationEnd.Stops);

    /// <summary><paramref name="actor"/>'s own run, looping until its owner ends it.</summary>
    private static Animation<RoomEffectActorSegment> Looping(RoomEffectActorKind actor) => Animation<RoomEffectActorSegment>.Looping([Segments.Whole(P.Of(actor))]);

    /// <summary><paramref name="actor"/>'s still picture, held until its owner ends it.</summary>
    private static Animation<RoomEffectActorSegment> Still(RoomEffectActorKind actor) => new([Segments.Whole(P.Of(actor))], AnimationEnd.Holds);

    /// <summary>
    /// A Metroid's shell flickering <paramref name="times"/> times (#2369): <paramref name="frame"/> for an update, then
    /// nothing for an update.
    /// </summary>
    private static IEnumerable<AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused> Flickering(EnemyFrameId frame, int times) =>
        Enumerable.Repeat<AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused[]>([Show(frame, 1), Show(EnemyFrameId.PointAtOrigin, 1)], times)
            .SelectMany(flicker => flicker);

    /// <summary>A still picture (#2275): enemy-sheet frame <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused Hold(EnemyFrameId frame) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.HeldFrame(RoomEffectActorPicture.OfEnemyFrame(frame));

    /// <summary>Shows enemy-sheet frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfEnemyFrame(frame), duration);

    /// <summary>Shows Samus's hit-burst sprite <paramref name="sprite"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Unpaused Burst(ProjectileSpriteId sprite, int duration) =>
        new AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame(RoomEffectActorPicture.OfHitBurst(sprite), duration);
}
