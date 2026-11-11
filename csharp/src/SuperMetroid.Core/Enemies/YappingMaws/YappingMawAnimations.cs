using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.YappingMaws.YappingMawCueKind;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawProgram;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations as typed programs (#641). Each
/// frame shows a <see cref="YappingMawFrame"/> pose and each callback becomes a <see cref="YappingMawCue"/> with its typed operands.
/// </summary>
internal static class YappingMawAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YappingMawProgram, YappingMawFrame, YappingMawCue> Segments { get; } = new(
        new Dictionary<YappingMawProgram, AnimationBeat<YappingMawFrame, YappingMawCue>[]>
    {
        [P.AttackingUp] =
        [
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
            Show(2, 80),
            Show(1, 3),
        ],
        [P.AttackingUpRight] =
        [
            Show(3, 5),
            Show(4, 3),
            Cue(new(K.PlayBiteSound)),
            Show(5, 80),
            Show(4, 3),
        ],
        [P.AttackingRight] =
        [
            Show(6, 5),
            Show(7, 3),
            Cue(new(K.PlayBiteSound)),
            Show(8, 80),
            Show(7, 3),
        ],
        [P.AttackingDownRight] =
        [
            Show(9, 5),
            Show(10, 3),
            Cue(new(K.PlayBiteSound)),
            Show(11, 80),
            Show(10, 3),
        ],
        [P.AttackingDown] =
        [
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
            Show(14, 80),
            Show(13, 3),
        ],
        [P.AttackingDownLeft] =
        [
            Show(15, 5),
            Show(16, 3),
            Cue(new(K.PlayBiteSound)),
            Show(17, 80),
            Show(16, 3),
        ],
        [P.AttackingLeft] =
        [
            Show(18, 5),
            Show(19, 3),
            Cue(new(K.PlayBiteSound)),
            Show(20, 80),
            Show(19, 3),
        ],
        [P.AttackingUpLeft] =
        [
            Show(21, 5),
            Show(22, 3),
            Cue(new(K.PlayBiteSound)),
            Show(23, 80),
            Show(22, 3),
        ],
        [P.CooldownUpRight] =
        [
            Cue(new(K.HoldSamusUpRight)),
            Show(5, 4),
        ],
        [P.CooldownUp] =
        [
            Cue(new(K.HoldSamusUp)),
        ],
        [P.CooldownUpLoop] =
        [
            Show(2, 80),
            Show(1, 3),
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownUpLeft] =
        [
            Cue(new(K.HoldSamusUpLeft)),
            Show(23, 4),
            Cue(new(K.HoldSamusUp)),
        ],
        [P.CooldownUpLeftLoop] =
        [
            Show(2, 80),
            Show(1, 3),
            Show(0, 5),
            Show(1, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownDownRight] =
        [
            Cue(new(K.HoldSamusDownRight)),
            Show(11, 4),
        ],
        [P.CooldownDown] =
        [
            Cue(new(K.HoldSamusDown)),
        ],
        [P.CooldownDownLoop] =
        [
            Show(14, 80),
            Show(13, 3),
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownDownLeft] =
        [
            Cue(new(K.HoldSamusDownLeft)),
            Show(17, 4),
            Cue(new(K.HoldSamusDown)),
        ],
        [P.CooldownDownLeftLoop] =
        [
            Show(14, 80),
            Show(13, 3),
            Show(12, 5),
            Show(13, 3),
            Cue(new(K.PlayBiteSound)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YappingMawProgram, Animation<YappingMawProgram>> StartingAt { get; } = new Dictionary<YappingMawProgram, Animation<YappingMawProgram>>
    {
        [P.AttackingUp] = new([new(P.AttackingUp, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingUpRight] = new([new(P.AttackingUpRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingRight] = new([new(P.AttackingRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDownRight] = new([new(P.AttackingDownRight, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDown] = new([new(P.AttackingDown, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDownLeft] = new([new(P.AttackingDownLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingLeft] = new([new(P.AttackingLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingUpLeft] = new([new(P.AttackingUpLeft, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CooldownUpRight] = new([new(P.CooldownUpRight, 0, 2, 1), new(P.CooldownUp, 0, 1, 1), new(P.CooldownUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(2)),
        [P.CooldownUp] = new([new(P.CooldownUp, 0, 1, 1), new(P.CooldownUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownUpLoop] = new([new(P.CooldownUpLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CooldownUpLeft] = new([new(P.CooldownUpLeft, 0, 3, 1), new(P.CooldownUpLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownUpLeftLoop] = new([new(P.CooldownUpLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CooldownDownRight] = new([new(P.CooldownDownRight, 0, 2, 1), new(P.CooldownDown, 0, 1, 1), new(P.CooldownDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(2)),
        [P.CooldownDown] = new([new(P.CooldownDown, 0, 1, 1), new(P.CooldownDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownDownLoop] = new([new(P.CooldownDownLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CooldownDownLeft] = new([new(P.CooldownDownLeft, 0, 3, 1), new(P.CooldownDownLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownDownLeftLoop] = new([new(P.CooldownDownLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YappingMawProgram> StartingWith(YappingMawProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<YappingMawFrame, YappingMawCue> Show(int pose, int duration) => new AnimationBeat<YappingMawFrame, YappingMawCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<YappingMawFrame, YappingMawCue> Cue(YappingMawCue cue) => new AnimationBeat<YappingMawFrame, YappingMawCue>.Event(cue);
}
