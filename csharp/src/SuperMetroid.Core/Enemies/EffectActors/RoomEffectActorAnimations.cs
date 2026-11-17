using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using K = SuperMetroid.Core.Enemies.EffectActors.RoomEffectActorKind;
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
    internal static AnimationSegments<RoomEffectActorSegment, RoomEffectActorPicture, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RoomEffectActorSegment, AnimationBeat<RoomEffectActorPicture, NoAnimationCue>[]>
    {
        [P.Of(K.DraygonFoam)] =
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
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash25Wide, 4),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash29Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash39Wide, 2),
            Show(EnemyFrameId.EffectActorNuclearWaffleSplash45Wide, 2),
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
        ],
        [P.MetroidElectricityCycle] =
        [
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
        [P.Of(K.MetroidShell)] =
        [
            .. Flickering(EnemyFrameId.EffectActorMetroidShell38By27, ShellFirstFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShell44By29, SwollenShellFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShell38By27, 1),
        ],
        [P.MetroidShellCycle] =
        [
            .. Flickering(EnemyFrameId.EffectActorMetroidShell38By27, ShellCycleFlickers),
            .. Flickering(EnemyFrameId.EffectActorMetroidShell38By28, StretchedShellCycleFlickers),
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
        [K.MetroidElectricity] = new([Whole(P.Of(K.MetroidElectricity)), Whole(P.MetroidElectricityCycle)], AnimationEnd.LoopsFrom(1)),
        [K.MetroidShell] = new([Whole(P.Of(K.MetroidShell)), Whole(P.MetroidShellCycle)], AnimationEnd.LoopsFrom(1)),
        [K.YappingMawBaseFacingDown] = Still(K.YappingMawBaseFacingDown),
        [K.YappingMawBaseFacingUp] = Still(K.YappingMawBaseFacingUp),
        [K.EvirFacingLeft] = Looping(K.EvirFacingLeft),
        [K.EvirFacingRight] = Looping(K.EvirFacingRight),
        [K.HitBurst] = Once(K.HitBurst),
    };

    /// <summary>The animation <paramref name="kind"/> plays from its spawn.</summary>
    internal static Animation<RoomEffectActorSegment> StartingWith(RoomEffectActorKind kind) => StartingAt[kind];

    /// <summary><paramref name="actor"/>'s own run, played once before the actor ends.</summary>
    private static Animation<RoomEffectActorSegment> Once(RoomEffectActorKind actor) => new([Whole(P.Of(actor))], AnimationEnd.Stops);

    /// <summary><paramref name="actor"/>'s own run, looping until its owner ends it.</summary>
    private static Animation<RoomEffectActorSegment> Looping(RoomEffectActorKind actor) => new([Whole(P.Of(actor))], AnimationEnd.LoopsFrom(0));

    /// <summary><paramref name="actor"/>'s still picture, held until its owner ends it.</summary>
    private static Animation<RoomEffectActorSegment> Still(RoomEffectActorKind actor) => new([Whole(P.Of(actor))], AnimationEnd.Holds);

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<RoomEffectActorSegment> Whole(RoomEffectActorSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>
    /// A Metroid's shell flickering <paramref name="times"/> times (#2369): <paramref name="frame"/> for an update, then
    /// nothing for an update.
    /// </summary>
    private static IEnumerable<AnimationBeat<RoomEffectActorPicture, NoAnimationCue>> Flickering(EnemyFrameId frame, int times) =>
        Enumerable.Repeat<AnimationBeat<RoomEffectActorPicture, NoAnimationCue>[]>([Show(frame, 1), Show(EnemyFrameId.EffectActorEmpty, 1)], times)
            .SelectMany(flicker => flicker);

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
