using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Samus.Projectiles.MiscDustProjectileSegment;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The fourteen misc-dust explosion, smoke, dust and bubble animations (#641, #1686). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class MiscDustProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MiscDustProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<MiscDustProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.ElbowChargeParticle] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleSecond, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleThird, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFourth, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleFifth, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeParticleSixth, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.ElbowChargeEnergy] =
        [
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySecond, 3),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyThird, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFourth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergyFifth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySixth, 2),
            Show(EnemyProjectileFrameId.MiscDustElbowChargeEnergySeventh, 12),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionSecond, 6),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionThird, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallExplosionSixth, 6),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.RainbowBeamExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionThird, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFourth, 4),
            Show(EnemyProjectileFrameId.MiscDustRainbowBeamExplosionFifth, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.DudShot] =
        [
            Show(EnemyProjectileFrameId.MiscDustDudShotFirst, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotSecond, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotThird, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFourth, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotFifth, 4),
            Show(EnemyProjectileFrameId.MiscDustDudShotSixth, 4),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.SmallDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudThird, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustSmallDustCloudFifth, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.CorpseDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFirst, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudSecond, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudThird, 3),
            Show(EnemyProjectileFrameId.MiscDustCorpseDustCloudFourth, 3),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.EyeDoorSweatDrop] =
        [
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropThird, 5),
            Show(EnemyProjectileFrameId.MiscDustEyeDoorSweatDropFourth, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.Smoke] =
        [
            Show(EnemyProjectileFrameId.MiscDustSmokeFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeThird, 8),
            Show(EnemyProjectileFrameId.MiscDustSmokeFourth, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigExplosion] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionThird, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigExplosionSixth, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.RockParticles] =
        [
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFirst, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSecond, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesThird, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFourth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFifth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSixth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSeventh, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesEighth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesNinth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesTenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesEleventh, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesTwelfth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesThirteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFourteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesFifteenth, 2),
            Show(EnemyProjectileFrameId.MiscDustRockParticlesSixteenth, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.ShortBigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFirst, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudSecond, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudThird, 2),
            Show(EnemyProjectileFrameId.MiscDustShortBigDustCloudFourth, 2),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.BigDustCloud] =
        [
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFirst, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSecond, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudThird, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFourth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudFifth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSixth, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudSeventh, 5),
            Show(EnemyProjectileFrameId.MiscDustBigDustCloudEighth, 5),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.DraygonBreathBubbles] =
        [
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFirst, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSecond, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesThird, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFourth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesFifth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSixth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesSeventh, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesEighth, 8),
            Show(EnemyProjectileFrameId.MiscDustDraygonBreathBubblesNinth, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MiscDustProjectileSegment, Animation<MiscDustProjectileSegment>> StartingAt { get; } = new Dictionary<MiscDustProjectileSegment, Animation<MiscDustProjectileSegment>>
    {
        [P.ElbowChargeParticle] = new([new(P.ElbowChargeParticle, 0, 7, 1)], AnimationEnd.Holds),
        [P.ElbowChargeEnergy] = new([new(P.ElbowChargeEnergy, 0, 8, 1)], AnimationEnd.Holds),
        [P.SmallExplosion] = new([new(P.SmallExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.RainbowBeamExplosion] = new([new(P.RainbowBeamExplosion, 0, 6, 1)], AnimationEnd.Holds),
        [P.DudShot] = new([new(P.DudShot, 0, 7, 1)], AnimationEnd.Holds),
        [P.SmallDustCloud] = new([new(P.SmallDustCloud, 0, 6, 1)], AnimationEnd.Holds),
        [P.CorpseDustCloud] = new([new(P.CorpseDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.EyeDoorSweatDrop] = new([new(P.EyeDoorSweatDrop, 0, 5, 1)], AnimationEnd.Holds),
        [P.Smoke] = new([new(P.Smoke, 0, 5, 1)], AnimationEnd.Holds),
        [P.BigExplosion] = new([new(P.BigExplosion, 0, 7, 1)], AnimationEnd.Holds),
        [P.RockParticles] = new([new(P.RockParticles, 0, 17, 1)], AnimationEnd.Holds),
        [P.ShortBigDustCloud] = new([new(P.ShortBigDustCloud, 0, 5, 1)], AnimationEnd.Holds),
        [P.BigDustCloud] = new([new(P.BigDustCloud, 0, 9, 1)], AnimationEnd.Holds),
        [P.DraygonBreathBubbles] = new([new(P.DraygonBreathBubbles, 0, 10, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MiscDustProjectileSegment> StartingWith(MiscDustProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
