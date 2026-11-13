using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="MotherBrainEffectCue"/> with its typed operands.
/// </summary>
internal static class MotherBrainEffectAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MotherBrainEffectProgram, EnemyProjectileFrameId, MotherBrainEffectCue> Segments { get; } = new(
        new Dictionary<MotherBrainEffectProgram, AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>[]>
    {
        [P.OnionRing] =
        [
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(1, 1))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFirst, 16),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(2, 2))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingSecond, 10),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(3, 3))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingThird, 8),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(4, 4))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFourth, 7),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(5, 5))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingFifth, 6),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(6, 6))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOnionRingSixth, 5),
        ],
        [P.OnionRingHit] =
        [
            Cue(new MotherBrainEffectCue.Command(K.DrawFromSharedSheet)),
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitFirst, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitSecond, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitThird, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitFourth, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitFifth, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpactOnionRingHitSixth, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Bomb] =
        [
            Show(EnemyProjectileFrameId.MotherBrainBombFirst, 6),
            Show(EnemyProjectileFrameId.MotherBrainBombSecond, 5),
            Show(EnemyProjectileFrameId.MotherBrainBombThird, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombFifth, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombSixth, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombSeventh, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombEighth, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombNinth, 5),
        ],
        [P.RainbowBeamCharging] =
        [
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFirst, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingSecond, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingThird, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFourth, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFifth, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingSixth, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Drool] =
        [
            Show(EnemyProjectileFrameId.MotherBrainDroolAttachedDroolFirst, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttachedDroolSecond, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttachedDroolThird, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttachedDroolFourth, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttachedDroolFifth, 10),
            Cue(new MotherBrainEffectCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.MotherBrainDroolFall)),
            Cue(new MotherBrainEffectCue.Command(K.DropDrool)),
            Show(EnemyProjectileFrameId.MotherBrainDroolReleased, 10),
        ],
        [P.DroolHitFloor] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFirst, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorSecond, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorThird, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFourth, 10),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeDoorParticle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFirst, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSecond, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleThird, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFourth, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFifth, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleSeventh, 4),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleEighth, 4),
        ],
        [P.PurpleBreathBig] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFirst, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigSecond, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigThird, 9),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFourth, 9),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFifth, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigSixth, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigSeventh, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigEighth, 11),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.PurpleBreathSmall] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFirst, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallSecond, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallThird, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFourth, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFifth, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallSixth, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallSeventh, 12),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallEighth, 12),
            Cue(new MotherBrainEffectCue.Command(K.EndSmallPurpleBreath)),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeSubtitle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeSubtitle, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MotherBrainEffectProgram, Animation<MotherBrainEffectProgram>> StartingAt { get; } = new Dictionary<MotherBrainEffectProgram, Animation<MotherBrainEffectProgram>>
    {
        [P.OnionRing] = new([new(P.OnionRing, 0, 12, 1)], AnimationEnd.Holds),
        [P.OnionRingHit] = new([new(P.OnionRingHit, 0, 9, 1)], AnimationEnd.Holds),
        [P.Bomb] = new([new(P.Bomb, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RainbowBeamCharging] = new([new(P.RainbowBeamCharging, 0, 7, 1)], AnimationEnd.Holds),
        [P.Drool] = new([new(P.Drool, 0, 8, 1)], AnimationEnd.Holds),
        [P.DroolHitFloor] = new([new(P.DroolHitFloor, 0, 6, 1)], AnimationEnd.Holds),
        [P.EscapeDoorParticle] = new([new(P.EscapeDoorParticle, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PurpleBreathBig] = new([new(P.PurpleBreathBig, 0, 10, 1)], AnimationEnd.Holds),
        [P.PurpleBreathSmall] = new([new(P.PurpleBreathSmall, 0, 11, 1)], AnimationEnd.Holds),
        [P.EscapeSubtitle] = new([new(P.EscapeSubtitle, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MotherBrainEffectProgram> StartingWith(MotherBrainEffectProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue> Cue(MotherBrainEffectCue cue) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Event(cue);
}
