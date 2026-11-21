using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.Projectiles.DustExplosionAnimation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The fourteen explosion, smoke, dust and bubble animations (#641, #1686). Each frame shows a
/// <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it
/// acts on.
/// </summary>
internal static class DustExplosionProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<DustExplosionAnimation, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<DustExplosionAnimation, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
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
    internal static IdentityTable<DustExplosionAnimation, Animation<DustExplosionAnimation>> StartingAt { get; } = new(Segments.EachEnding(AnimationEnd.Holds,
        P.MotherBrainElbowChargeParticle, P.MotherBrainElbowChargeEnergy, P.SmallExplosion,
        P.MotherBrainRainbowBeamExplosion, P.DudShotTinyExplosion, P.SmallDustCloud, P.CorpseDustCloud,
        P.EyeDoorSmokePuff, P.Smoke, P.BigExplosion, P.RockParticles, P.ShortBigDustCloud, P.BigDustCloud,
        P.LongDraygonBreathBubbles));

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="segment"/> is not one of the fourteen animations.</exception>
    internal static Animation<DustExplosionAnimation> StartingWith(DustExplosionAnimation segment) =>
        StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
