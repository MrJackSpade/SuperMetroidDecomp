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
            Show(EnemyProjectileFrameId.MotherBrainBlueRing00, 16),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(2, 2))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRing01, 10),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(3, 3))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRing02, 8),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(4, 4))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRing03, 7),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(5, 5))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRing04, 6),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(6, 6))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRing05, 5),
        ],
        [P.OnionRingHit] =
        [
            Cue(new MotherBrainEffectCue.Command(K.DrawFromSharedSheet)),
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact00, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact01, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact02, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact03, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact04, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingImpact05, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Bomb] =
        [
            Show(EnemyProjectileFrameId.MotherBrainBombFrame00, 6),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame01, 5),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame02, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame03, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame04, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame05, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame06, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame07, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombFrame08, 5),
        ],
        [P.RainbowBeamCharging] =
        [
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame00, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame01, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame02, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame03, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame04, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame05, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Drool] =
        [
            Show(EnemyProjectileFrameId.MotherBrainDroolAttached00, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttached01, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttached02, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttached03, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolAttached04, 10),
            Cue(new MotherBrainEffectCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.MotherBrainDroolFall)),
            Cue(new MotherBrainEffectCue.Command(K.DropDrool)),
            Show(EnemyProjectileFrameId.MotherBrainDroolReleased, 10),
        ],
        [P.DroolHitFloor] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame00, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame01, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame02, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame03, 10),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeDoorParticle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame00, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame01, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame02, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame03, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame04, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame05, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame06, 4),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame07, 4),
        ],
        [P.PurpleBreathBig] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame00, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame01, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame02, 9),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame03, 9),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame04, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame05, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame06, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame07, 11),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.PurpleBreathSmall] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame00, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame01, 8),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame02, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame03, 10),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame04, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame05, 11),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame06, 12),
            Show(EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame07, 12),
            Cue(new MotherBrainEffectCue.Command(K.EndSmallPurpleBreath)),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeSubtitle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeSubtitleFrame00, 1),
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
