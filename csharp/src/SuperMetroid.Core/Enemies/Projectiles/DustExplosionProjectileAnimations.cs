using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Projectiles.DustExplosionAnimation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The fourteen misc-dust explosion, smoke, dust and bubble animations (#641, #1686). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class DustExplosionProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<DustExplosionAnimation, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.MotherBrainElbowChargeParticle] =
        [
            Show(EnemyProjectileFrameId.MiscDustSparksGathering, 5),
            Show(EnemyProjectileFrameId.MiscDustSparksDrawingIn, 4),
            Show(EnemyProjectileFrameId.MiscDustSparksClosingIn, 3),
            Show(EnemyProjectileFrameId.MiscDustSparksConverging, 3),
            Show(EnemyProjectileFrameId.MiscDustSparksNearing, 3),
            Show(EnemyProjectileFrameId.MiscDustSparksMeeting, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.MotherBrainElbowChargeEnergy] =
        [
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedGathering, 4),
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedDrawingIn, 3),
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedClosingIn, 2),
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedConverging, 2),
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedNearing, 2),
            Show(EnemyProjectileFrameId.MiscDustSparksTwistedMeeting, 2),
            Show(EnemyProjectileFrameId.MiscDustSpark, 12),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustSpark, 4),
            Show(EnemyProjectileFrameId.MiscDustFireballSmall, 6),
            Show(EnemyProjectileFrameId.MiscDustFireballLarge, 5),
            Show(EnemyProjectileFrameId.MiscDustFlamingRing, 5),
            Show(EnemyProjectileFrameId.MiscDustStarburst, 5),
            Show(EnemyProjectileFrameId.MiscDustEmbers, 6),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.MotherBrainRainbowBeamExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustGreenOrbSmall, 3),
            Show(EnemyProjectileFrameId.MiscDustGreenOrb, 3),
            Show(EnemyProjectileFrameId.MiscDustGreenRingCored, 4),
            Show(EnemyProjectileFrameId.MiscDustGreenRingThick, 4),
            Show(EnemyProjectileFrameId.MiscDustGreenRing, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.DudShotTinyExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustFlareGlint, 4),
            Show(EnemyProjectileFrameId.MiscDustFlareGlow, 4),
            Show(EnemyProjectileFrameId.MiscDustFlareBall, 4),
            Show(EnemyProjectileFrameId.MiscDustFlareRingCored, 4),
            Show(EnemyProjectileFrameId.MiscDustFlareRing, 4),
            Show(EnemyProjectileFrameId.MiscDustFlareRingFading, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustPuff, 5),
            Show(EnemyProjectileFrameId.MiscDustCloud, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudThinning, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudThin, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudFaint, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.CorpseDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustPuff, 3),
            Show(EnemyProjectileFrameId.MiscDustPuffSmall, 3),
            Show(EnemyProjectileFrameId.MiscDustPuffThin, 3),
            Show(EnemyProjectileFrameId.MiscDustPuffFaint, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.EyeDoorSmokePuff] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmokePuffLow, 5),
            Show(EnemyProjectileFrameId.MiscDustSmokePuffRising, 5),
            Show(EnemyProjectileFrameId.MiscDustSmokePuffScattering, 5),
            Show(EnemyProjectileFrameId.MiscDustSmokePuffFading, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.Smoke] =
        [
            Show(EnemyProjectileFrameId.MiscDustFireCloud, 8),
            Show(EnemyProjectileFrameId.MiscDustFireCloudSpreading, 8),
            Show(EnemyProjectileFrameId.MiscDustFireCloudBreaking, 8),
            Show(EnemyProjectileFrameId.MiscDustFireCloudEmbers, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustFireballSmall, 5),
            Show(EnemyProjectileFrameId.MiscDustFireballLarge, 5),
            Show(EnemyProjectileFrameId.MiscDustFlamingRing, 5),
            Show(EnemyProjectileFrameId.MiscDustFireballHuge, 5),
            Show(EnemyProjectileFrameId.MiscDustWideRing, 5),
            Show(EnemyProjectileFrameId.MiscDustWideRingFading, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.RockParticles] =
        [
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing16, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing20, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing26, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing30, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing34, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesGlowing36, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesCooling40, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesCooling44, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesCooling48, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesCooling52, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen54, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen56, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen58, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen60, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen62, 2),
            Show(EnemyProjectileFrameId.MiscDustPebblesAshen64, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustPuffUpLeft, 2),
            Show(EnemyProjectileFrameId.MiscDustPuffPair, 2),
            Show(EnemyProjectileFrameId.MiscDustPuffTrio, 2),
            Show(EnemyProjectileFrameId.MiscDustCloudsDense, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustPuffUpLeft, 5),
            Show(EnemyProjectileFrameId.MiscDustPuffPair, 5),
            Show(EnemyProjectileFrameId.MiscDustPuffTrio, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudsDense, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudsThinning, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudsThin, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudsFaint, 5),
            Show(EnemyProjectileFrameId.MiscDustCloudFaintRight, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.LongDraygonBreathBubbles] =
        [
            Show(EnemyProjectileFrameId.MiscDustBubbleSmall, 8),
            Show(EnemyProjectileFrameId.MiscDustBubbles, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesRising, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesSpreading, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesScattered, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesSparse, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesFew, 8),
            Show(EnemyProjectileFrameId.MiscDustBubblesLast, 8),
            Show(EnemyProjectileFrameId.MiscDustSpeck, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>> StartingAt { get; } = new Dictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>>
    {
        [P.MotherBrainElbowChargeParticle] = new([new(P.MotherBrainElbowChargeParticle, 0, 7, 1)], AnimationEnd.Holds),
        [P.MotherBrainElbowChargeEnergy] = new([new(P.MotherBrainElbowChargeEnergy, 0, 8, 1)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.MotherBrainRainbowBeamExplosion] = new([new(P.MotherBrainRainbowBeamExplosion, 0, 6, 1)], AnimationEnd.Holds),
        [P.DudShotTinyExplosion] = new([new(P.DudShotTinyExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.SmallDustCloud] = new([new(P.SmallDustCloud, 0, 6, 1)], AnimationEnd.Holds),
        [P.CorpseDustCloud] = new([new(P.CorpseDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.EyeDoorSmokePuff] = new([new(P.EyeDoorSmokePuff, 0, 5, 1)], AnimationEnd.Holds),
        [P.Smoke] = new([new(P.Smoke, 0, 5, 1)], AnimationEnd.Holds),
        [P.BigExplosion] = new([new(P.BigExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.RockParticles] = new([new(P.RockParticles, 0, 17, 1)], AnimationEnd.Holds),
        [P.ShortBigDustCloud] = new([new(P.ShortBigDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.BigDustCloud] = new([new(P.BigDustCloud, 0, 9, 1)], AnimationEnd.Holds),
        [P.LongDraygonBreathBubbles] = new([new(P.LongDraygonBreathBubbles, 0, 10, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segment"/> is not one of the fourteen animations.</exception>
    internal static Animation<DustExplosionAnimation> StartingWith(DustExplosionAnimation segment) =>
        StartingAt.TryGetValue(segment, out Animation<DustExplosionAnimation>? animation)
            ? animation
            : throw new ArgumentOutOfRangeException(nameof(segment), segment, "Undefined misc-dust animation.");

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
