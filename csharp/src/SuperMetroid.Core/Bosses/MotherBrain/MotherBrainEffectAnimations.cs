using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="MotherBrainEffectCue"/> with the values it acts on.
/// </summary>
internal static class MotherBrainEffectAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<MotherBrainEffectSegment, EnemyProjectileFrameId, MotherBrainEffectCue> Segments { get; } = new(
        new Dictionary<MotherBrainEffectSegment, AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Unpaused[]>
    {
        [P.OnionRing] =
        [
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(1, 1))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingForming, 16),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(2, 2))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingOpening, 10),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(3, 3))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingSpreading, 8),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(4, 4))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingGrowing, 7),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(5, 5))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingSwelling, 6),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(6, 6))),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingFullSize, 5),
        ],
        [P.OnionRingHit] =
        [
            Cue(new MotherBrainEffectCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingFireballSmall, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingFireballLarge, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingFlamingRing, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingFireballHuge, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingWideRing, 5),
            Show(EnemyProjectileFrameId.MotherBrainBlueRingWideRingFading, 5),
            Cue(new MotherBrainEffectCue.Signal(K.Delete)),
        ],
        [P.Bomb] =
        [
            Show(EnemyProjectileFrameId.MotherBrainBombGlowRed, 6),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowOrangeRight, 5),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowYellowRight, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowYellowTop, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowOrangeCenter, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowYellowBottom, 2),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowYellowLeft, 3),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowOrangeLeft, 4),
            Show(EnemyProjectileFrameId.MotherBrainBombGlowRedDim, 5),
        ],
        [P.RainbowBeamCharging] =
        [
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeSparksSpread, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeSparksClosing, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksClosing, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksNear, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksGathered, 5),
            Show(EnemyProjectileFrameId.MotherBrainRainbowChargeStreaksAndFireballs, 5),
            Cue(new MotherBrainEffectCue.Signal(K.Delete)),
        ],
        [P.Drool] =
        [
            Show(EnemyProjectileFrameId.MotherBrainDroolSpeck, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolBead, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolHanging, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolStretching, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolStrand, 10),
            Cue(new MotherBrainEffectCue.Signal(K.ReleaseDrool)),
            Cue(new MotherBrainEffectCue.Signal(K.DropDrool)),
            Show(EnemyProjectileFrameId.MotherBrainDroolDrop, 10),
        ],
        [P.DroolHitFloor] =
        [
            Cue(new MotherBrainEffectCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.MotherBrainDroolSplashRising, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolSplashScattered, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolSplashFlat, 10),
            Show(EnemyProjectileFrameId.MotherBrainDroolSplashFaint, 10),
            Cue(new MotherBrainEffectCue.Signal(K.Delete)),
        ],
        [P.EscapeDoorParticle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned0, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned0, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned90, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned90, 1),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned180, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned180, 3),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisEdgeTurned270, 4),
            Show(EnemyProjectileFrameId.MotherBrainEscapeDoorDebrisFaceTurned270, 4),
        ],
        [P.PurpleBreathBig] =
        [
            Cue(new MotherBrainEffectCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePuff, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlume, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlumeLong, 9),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargePlumeFull, 9),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeDispersing, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeThinning, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeFading, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathLargeFaint, 11),
            Cue(new MotherBrainEffectCue.Signal(K.Delete)),
        ],
        [P.PurpleBreathSmall] =
        [
            Cue(new MotherBrainEffectCue.Signal(K.StopMoving)),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallPuff, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallPuffGrowing, 8),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallCloud, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallCloudLong, 10),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallDispersing, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallThinning, 11),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallFading, 12),
            Show(EnemyProjectileFrameId.MotherBrainBreathSmallFaint, 12),
            Cue(new MotherBrainEffectCue.Signal(K.EndSmallPurpleBreath)),
            Cue(new MotherBrainEffectCue.Signal(K.Delete)),
        ],
        [P.EscapeSubtitle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainEscapeSubtitle, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>> StartingAt { get; } = new Dictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>>
    {
        [P.OnionRing] = new([Segments.Whole(P.OnionRing)], AnimationEnd.Holds),
        [P.OnionRingHit] = new([Segments.Whole(P.OnionRingHit)], AnimationEnd.Holds),
        [P.Bomb] = Animation<MotherBrainEffectSegment>.Looping([Segments.Whole(P.Bomb)]),
        [P.RainbowBeamCharging] = new([Segments.Whole(P.RainbowBeamCharging)], AnimationEnd.Holds),
        [P.Drool] = new([Segments.Whole(P.Drool)], AnimationEnd.Holds),
        [P.DroolHitFloor] = new([Segments.Whole(P.DroolHitFloor)], AnimationEnd.Holds),
        [P.EscapeDoorParticle] = Animation<MotherBrainEffectSegment>.Looping([Segments.Whole(P.EscapeDoorParticle)]),
        [P.PurpleBreathBig] = new([Segments.Whole(P.PurpleBreathBig)], AnimationEnd.Holds),
        [P.PurpleBreathSmall] = new([Segments.Whole(P.PurpleBreathSmall)], AnimationEnd.Holds),
        [P.EscapeSubtitle] = new([Segments.Whole(P.EscapeSubtitle)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MotherBrainEffectSegment> StartingWith(MotherBrainEffectSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Unpaused Cue(MotherBrainEffectCue cue) => new AnimationBeat<EnemyProjectileFrameId, MotherBrainEffectCue>.Event(cue);
}
