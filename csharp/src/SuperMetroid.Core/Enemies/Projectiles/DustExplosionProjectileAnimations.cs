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
            Show(EnemyProjectileFrameId.DustExplosionSparksGathering, 5),
            Show(EnemyProjectileFrameId.DustExplosionSparksDrawingIn, 4),
            Show(EnemyProjectileFrameId.DustExplosionSparksClosingIn, 3),
            Show(EnemyProjectileFrameId.DustExplosionSparksConverging, 3),
            Show(EnemyProjectileFrameId.DustExplosionSparksNearing, 3),
            Show(EnemyProjectileFrameId.DustExplosionSparksMeeting, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.MotherBrainElbowChargeEnergy] =
        [
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedGathering, 4),
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedDrawingIn, 3),
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedClosingIn, 2),
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedConverging, 2),
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedNearing, 2),
            Show(EnemyProjectileFrameId.DustExplosionSparksTwistedMeeting, 2),
            Show(EnemyProjectileFrameId.DustExplosionSpark, 12),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.DustExplosionSpark, 4),
            Show(EnemyProjectileFrameId.DustExplosionFireballSmall, 6),
            Show(EnemyProjectileFrameId.DustExplosionFireballLarge, 5),
            Show(EnemyProjectileFrameId.DustExplosionFlamingRing, 5),
            Show(EnemyProjectileFrameId.DustExplosionStarburst, 5),
            Show(EnemyProjectileFrameId.DustExplosionEmbers, 6),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.MotherBrainRainbowBeamExplosion] =
        [
            Show(EnemyProjectileFrameId.DustExplosionGreenOrbSmall, 3),
            Show(EnemyProjectileFrameId.DustExplosionGreenOrb, 3),
            Show(EnemyProjectileFrameId.DustExplosionGreenRingCored, 4),
            Show(EnemyProjectileFrameId.DustExplosionGreenRingThick, 4),
            Show(EnemyProjectileFrameId.DustExplosionGreenRing, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.DudShotTinyExplosion] =
        [
            Show(EnemyProjectileFrameId.DustExplosionFlareGlint, 4),
            Show(EnemyProjectileFrameId.DustExplosionFlareGlow, 4),
            Show(EnemyProjectileFrameId.DustExplosionFlareBall, 4),
            Show(EnemyProjectileFrameId.DustExplosionFlareRingCored, 4),
            Show(EnemyProjectileFrameId.DustExplosionFlareRing, 4),
            Show(EnemyProjectileFrameId.DustExplosionFlareRingFading, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallDustCloud] =
        [
            Show(EnemyProjectileFrameId.DustExplosionPuff, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloud, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudThinning, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudThin, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudFaint, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.CorpseDustCloud] =
        [
            Show(EnemyProjectileFrameId.DustExplosionPuff, 3),
            Show(EnemyProjectileFrameId.DustExplosionPuffSmall, 3),
            Show(EnemyProjectileFrameId.DustExplosionPuffThin, 3),
            Show(EnemyProjectileFrameId.DustExplosionPuffFaint, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.EyeDoorSmokePuff] =
        [
            Show(EnemyProjectileFrameId.DustExplosionSmokePuffLow, 5),
            Show(EnemyProjectileFrameId.DustExplosionSmokePuffRising, 5),
            Show(EnemyProjectileFrameId.DustExplosionSmokePuffScattering, 5),
            Show(EnemyProjectileFrameId.DustExplosionSmokePuffFading, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.Smoke] =
        [
            Show(EnemyProjectileFrameId.DustExplosionFireCloud, 8),
            Show(EnemyProjectileFrameId.DustExplosionFireCloudSpreading, 8),
            Show(EnemyProjectileFrameId.DustExplosionFireCloudBreaking, 8),
            Show(EnemyProjectileFrameId.DustExplosionFireCloudEmbers, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyProjectileFrameId.DustExplosionFireballSmall, 5),
            Show(EnemyProjectileFrameId.DustExplosionFireballLarge, 5),
            Show(EnemyProjectileFrameId.DustExplosionFlamingRing, 5),
            Show(EnemyProjectileFrameId.DustExplosionFireballHuge, 5),
            Show(EnemyProjectileFrameId.DustExplosionWideRing, 5),
            Show(EnemyProjectileFrameId.DustExplosionWideRingFading, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.RockParticles] =
        [
            Show(EnemyProjectileFrameId.DustExplosionPebblesBursting, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesFlyingOut, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesFlyingApart, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesClimbing, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesSlowing, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesPeaking, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesTurningOver, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesStartingDown, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesDropping, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesFalling, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesTurningAshen, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesTumbling, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesPlunging, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesPlummeting, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesScattering, 2),
            Show(EnemyProjectileFrameId.DustExplosionPebblesFallingAway, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyProjectileFrameId.DustExplosionPuffUpLeft, 2),
            Show(EnemyProjectileFrameId.DustExplosionPuffPair, 2),
            Show(EnemyProjectileFrameId.DustExplosionPuffTrio, 2),
            Show(EnemyProjectileFrameId.DustExplosionCloudsDense, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyProjectileFrameId.DustExplosionPuffUpLeft, 5),
            Show(EnemyProjectileFrameId.DustExplosionPuffPair, 5),
            Show(EnemyProjectileFrameId.DustExplosionPuffTrio, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudsDense, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudsThinning, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudsThin, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudsFaint, 5),
            Show(EnemyProjectileFrameId.DustExplosionCloudFaintRight, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.LongDraygonBreathBubbles] =
        [
            Show(EnemyProjectileFrameId.DustExplosionBubbleSmall, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubbles, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesRising, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesSpreading, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesScattered, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesSparse, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesFew, 8),
            Show(EnemyProjectileFrameId.DustExplosionBubblesLast, 8),
            Show(EnemyProjectileFrameId.DustExplosionSpeck, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>> StartingAt { get; } = new Dictionary<DustExplosionAnimation, Animation<DustExplosionAnimation>>
    {
        [P.MotherBrainElbowChargeParticle] = new([Segments.Whole(P.MotherBrainElbowChargeParticle)], AnimationEnd.Holds),
        [P.MotherBrainElbowChargeEnergy] = new([Segments.Whole(P.MotherBrainElbowChargeEnergy)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([Segments.Whole(P.SmallExplosion)], AnimationEnd.Holds),
        [P.MotherBrainRainbowBeamExplosion] = new([Segments.Whole(P.MotherBrainRainbowBeamExplosion)], AnimationEnd.Holds),
        [P.DudShotTinyExplosion] = new([Segments.Whole(P.DudShotTinyExplosion)], AnimationEnd.Holds),
        [P.SmallDustCloud] = new([Segments.Whole(P.SmallDustCloud)], AnimationEnd.Holds),
        [P.CorpseDustCloud] = new([Segments.Whole(P.CorpseDustCloud)], AnimationEnd.Holds),
        [P.EyeDoorSmokePuff] = new([Segments.Whole(P.EyeDoorSmokePuff)], AnimationEnd.Holds),
        [P.Smoke] = new([Segments.Whole(P.Smoke)], AnimationEnd.Holds),
        [P.BigExplosion] = new([Segments.Whole(P.BigExplosion)], AnimationEnd.Holds),
        [P.RockParticles] = new([Segments.Whole(P.RockParticles)], AnimationEnd.Holds),
        [P.ShortBigDustCloud] = new([Segments.Whole(P.ShortBigDustCloud)], AnimationEnd.Holds),
        [P.BigDustCloud] = new([Segments.Whole(P.BigDustCloud)], AnimationEnd.Holds),
        [P.LongDraygonBreathBubbles] = new([Segments.Whole(P.LongDraygonBreathBubbles)], AnimationEnd.Holds),
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
