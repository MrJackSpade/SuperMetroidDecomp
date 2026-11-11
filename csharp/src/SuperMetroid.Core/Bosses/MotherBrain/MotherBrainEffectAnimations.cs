using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="MotherBrainEffectFrame"/> pose and each command becomes a <see cref="MotherBrainEffectCue"/> with its typed operands.
/// </summary>
internal static class MotherBrainEffectAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Segments { get; } = new(
        new Dictionary<MotherBrainEffectProgram, AnimationBeat<MotherBrainEffectFrame, MotherBrainEffectCue>[]>
    {
        [P.OnionRing] =
        [
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(1, 1))),
            Show(0, 16),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(2, 2))),
            Show(1, 10),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(3, 3))),
            Show(2, 8),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(4, 4))),
            Show(3, 7),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(5, 5))),
            Show(4, 6),
            Cue(new MotherBrainEffectCue.SetRadius(Radius: new(6, 6))),
            Show(5, 5),
        ],
        [P.OnionRingHit] =
        [
            Cue(new MotherBrainEffectCue.Command(K.DrawFromSharedSheet)),
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Bomb] =
        [
            Show(12, 6),
            Show(13, 5),
            Show(14, 4),
            Show(15, 3),
            Show(16, 2),
            Show(17, 2),
            Show(18, 3),
            Show(19, 4),
            Show(20, 5),
        ],
        [P.RainbowBeamCharging] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(23, 5),
            Show(24, 5),
            Show(25, 5),
            Show(26, 5),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.Drool] =
        [
            Show(27, 10),
            Show(28, 10),
            Show(29, 10),
            Show(30, 10),
            Show(31, 10),
            Cue(new MotherBrainEffectCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.MotherBrainDroolFall)),
            Cue(new MotherBrainEffectCue.Command(K.DropDrool)),
            Show(32, 10),
        ],
        [P.DroolHitFloor] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(33, 10),
            Show(34, 10),
            Show(35, 10),
            Show(36, 10),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeDoorParticle] =
        [
            Show(37, 1),
            Show(38, 1),
            Show(39, 1),
            Show(40, 1),
            Show(41, 3),
            Show(42, 3),
            Show(43, 4),
            Show(44, 4),
        ],
        [P.PurpleBreathBig] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(45, 8),
            Show(46, 8),
            Show(47, 9),
            Show(48, 9),
            Show(49, 10),
            Show(50, 10),
            Show(51, 11),
            Show(52, 11),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.PurpleBreathSmall] =
        [
            Cue(new MotherBrainEffectCue.Command(K.ClearFrameBehavior)),
            Show(53, 8),
            Show(54, 8),
            Show(55, 10),
            Show(56, 10),
            Show(57, 11),
            Show(58, 11),
            Show(59, 12),
            Show(60, 12),
            Cue(new MotherBrainEffectCue.Command(K.EndSmallPurpleBreath)),
            Cue(new MotherBrainEffectCue.Command(K.Delete)),
        ],
        [P.EscapeSubtitle] =
        [
            Show(61, 1),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MotherBrainEffectFrame, MotherBrainEffectCue> Show(int pose, int duration) => new AnimationBeat<MotherBrainEffectFrame, MotherBrainEffectCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MotherBrainEffectFrame, MotherBrainEffectCue> Cue(MotherBrainEffectCue cue) => new AnimationBeat<MotherBrainEffectFrame, MotherBrainEffectCue>.Event(cue);
}
