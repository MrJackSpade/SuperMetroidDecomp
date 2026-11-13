using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.YappingMaws.YappingMawCueKind;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="YappingMawCue"/> with its typed operands.
/// </summary>
internal static class YappingMawAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YappingMawSegment, EnemyFrameId, YappingMawCue> Segments { get; } = new(
        new Dictionary<YappingMawSegment, AnimationBeat<EnemyFrameId, YappingMawCue>[]>
    {
        [P.AttackingUp] =
        [
            Show(EnemyFrameId.YappingMawAttackingUpFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingUpThird, 80),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
        ],
        [P.AttackingUpRight] =
        [
            Show(EnemyFrameId.YappingMawAttackingUpRightFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingUpRightSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingUpRightThird, 80),
            Show(EnemyFrameId.YappingMawAttackingUpRightSecond, 3),
        ],
        [P.AttackingRight] =
        [
            Show(EnemyFrameId.YappingMawAttackingRightFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingRightSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingRightThird, 80),
            Show(EnemyFrameId.YappingMawAttackingRightSecond, 3),
        ],
        [P.AttackingDownRight] =
        [
            Show(EnemyFrameId.YappingMawAttackingDownRightFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingDownRightSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingDownRightThird, 80),
            Show(EnemyFrameId.YappingMawAttackingDownRightSecond, 3),
        ],
        [P.AttackingDown] =
        [
            Show(EnemyFrameId.YappingMawAttackingDownFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingDownThird, 80),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
        ],
        [P.AttackingDownLeft] =
        [
            Show(EnemyFrameId.YappingMawAttackingDownLeftFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingDownLeftSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingDownLeftThird, 80),
            Show(EnemyFrameId.YappingMawAttackingDownLeftSecond, 3),
        ],
        [P.AttackingLeft] =
        [
            Show(EnemyFrameId.YappingMawAttackingLeftFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingLeftSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingLeftThird, 80),
            Show(EnemyFrameId.YappingMawAttackingLeftSecond, 3),
        ],
        [P.AttackingUpLeft] =
        [
            Show(EnemyFrameId.YappingMawAttackingUpLeftFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingUpLeftSecond, 3),
            Cue(new(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawAttackingUpLeftThird, 80),
            Show(EnemyFrameId.YappingMawAttackingUpLeftSecond, 3),
        ],
        [P.CooldownUpRight] =
        [
            Cue(new(K.HoldSamusUpRight)),
            Show(EnemyFrameId.YappingMawAttackingUpRightThird, 4),
        ],
        [P.CooldownUp] =
        [
            Cue(new(K.HoldSamusUp)),
        ],
        [P.CooldownUpLoop] =
        [
            Show(EnemyFrameId.YappingMawAttackingUpThird, 80),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
            Show(EnemyFrameId.YappingMawAttackingUpFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownUpLeft] =
        [
            Cue(new(K.HoldSamusUpLeft)),
            Show(EnemyFrameId.YappingMawAttackingUpLeftThird, 4),
            Cue(new(K.HoldSamusUp)),
        ],
        [P.CooldownUpLeftLoop] =
        [
            Show(EnemyFrameId.YappingMawAttackingUpThird, 80),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
            Show(EnemyFrameId.YappingMawAttackingUpFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingUpSecond, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownDownRight] =
        [
            Cue(new(K.HoldSamusDownRight)),
            Show(EnemyFrameId.YappingMawAttackingDownRightThird, 4),
        ],
        [P.CooldownDown] =
        [
            Cue(new(K.HoldSamusDown)),
        ],
        [P.CooldownDownLoop] =
        [
            Show(EnemyFrameId.YappingMawAttackingDownThird, 80),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
            Show(EnemyFrameId.YappingMawAttackingDownFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
            Cue(new(K.PlayBiteSound)),
        ],
        [P.CooldownDownLeft] =
        [
            Cue(new(K.HoldSamusDownLeft)),
            Show(EnemyFrameId.YappingMawAttackingDownLeftThird, 4),
            Cue(new(K.HoldSamusDown)),
        ],
        [P.CooldownDownLeftLoop] =
        [
            Show(EnemyFrameId.YappingMawAttackingDownThird, 80),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
            Show(EnemyFrameId.YappingMawAttackingDownFirst, 5),
            Show(EnemyFrameId.YappingMawAttackingDownSecond, 3),
            Cue(new(K.PlayBiteSound)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YappingMawSegment, Animation<YappingMawSegment>> StartingAt { get; } = new Dictionary<YappingMawSegment, Animation<YappingMawSegment>>
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
    internal static Animation<YappingMawSegment> StartingWith(YappingMawSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Cue(YappingMawCue cue) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Event(cue);
}
