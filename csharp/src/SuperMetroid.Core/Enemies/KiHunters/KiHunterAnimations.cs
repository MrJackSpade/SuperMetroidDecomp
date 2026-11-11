using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.KiHunters.KiHunterCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterProgram;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641). Each
/// frame shows a <see cref="KiHunterFrame"/> pose and each callback becomes a <see cref="KiHunterCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterProgram, KiHunterFrame, KiHunterCue> Segments { get; } = new(
        new Dictionary<KiHunterProgram, AnimationBeat<KiHunterFrame, KiHunterCue>[]>
    {
        [P.FlyingLeft] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopLeft] =
        [
            Show(14, 2),
            Show(15, 6),
            Show(14, 2),
            Show(13, 2),
            Show(16, 2),
            Show(17, 32),
        ],
        [P.FlyingRight] =
        [
            Show(6, 2),
            Show(7, 2),
            Show(8, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopRight] =
        [
            Show(19, 2),
            Show(20, 6),
            Show(19, 2),
            Show(18, 2),
            Show(21, 2),
            Show(22, 32),
        ],
        [P.WingsLeft] =
        [
            Show(3, 2),
            Show(4, 2),
            Show(5, 1),
        ],
        [P.WingsRight] =
        [
            Show(9, 2),
            Show(10, 2),
            Show(11, 1),
        ],
        [P.DetachedWings] =
        [
            Show(12, 1),
        ],
        [P.JumpLeft] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 11),
            Show(24, 2),
            Show(23, 2),
            Cue(new(K.StartHop)),
            Show(26, 1),
        ],
        [P.JumpRight] =
        [
            Show(27, 8),
            Show(28, 8),
            Show(29, 11),
            Show(28, 2),
            Show(27, 2),
            Cue(new(K.StartHop)),
            Show(30, 1),
        ],
        [P.LandLeft] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 11),
            Show(24, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(23, 1),
        ],
        [P.LandRight] =
        [
            Show(27, 8),
            Show(28, 8),
            Show(29, 11),
            Show(28, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(27, 1),
        ],
        [P.SpitLeft] =
        [
            Show(31, 32),
            Show(32, 6),
            Show(33, 16),
            Show(34, 2),
            Cue(new(K.SpitLeft)),
            Show(35, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(31, 1),
        ],
        [P.SpitRight] =
        [
            Show(36, 32),
            Show(37, 6),
            Show(38, 16),
            Show(39, 2),
            Cue(new(K.SpitRight)),
            Show(40, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(36, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterProgram, Animation<KiHunterProgram>> StartingAt { get; } = new Dictionary<KiHunterProgram, Animation<KiHunterProgram>>
    {
        [P.FlyingLeft] = new([new(P.FlyingLeft, 0, 4, 1), new(P.SwoopLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopLeft] = new([new(P.SwoopLeft, 0, 6, 1), new(P.FlyingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlyingRight] = new([new(P.FlyingRight, 0, 4, 1), new(P.SwoopRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopRight] = new([new(P.SwoopRight, 0, 6, 1), new(P.FlyingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsLeft] = new([new(P.WingsLeft, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsRight] = new([new(P.WingsRight, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DetachedWings] = new([new(P.DetachedWings, 0, 1, 1)], AnimationEnd.Holds),
        [P.JumpLeft] = new([new(P.JumpLeft, 0, 7, 1)], AnimationEnd.Holds),
        [P.JumpRight] = new([new(P.JumpRight, 0, 7, 1)], AnimationEnd.Holds),
        [P.LandLeft] = new([new(P.LandLeft, 0, 6, 1)], AnimationEnd.Holds),
        [P.LandRight] = new([new(P.LandRight, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpitLeft] = new([new(P.SpitLeft, 0, 8, 1)], AnimationEnd.Holds),
        [P.SpitRight] = new([new(P.SpitRight, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterProgram> StartingWith(KiHunterProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KiHunterFrame, KiHunterCue> Show(int pose, int duration) => new AnimationBeat<KiHunterFrame, KiHunterCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<KiHunterFrame, KiHunterCue> Cue(KiHunterCue cue) => new AnimationBeat<KiHunterFrame, KiHunterCue>.Event(cue);
}
