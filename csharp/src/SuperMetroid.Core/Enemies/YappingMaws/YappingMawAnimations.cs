using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.YappingMaws.YappingMawCueKind;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="YappingMawCue"/> with the values it acts on.
/// </summary>
internal static class YappingMawAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YappingMawSegment, EnemyFrameId, YappingMawCue> Segments { get; } = new(
        new Dictionary<YappingMawSegment, AnimationBeat<EnemyFrameId, YappingMawCue>[]>
    {
        [P.AttackingUp] =
        [
            Show(EnemyFrameId.YappingMawFacingUpJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingUpJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
        ],
        [P.AttackingUpRight] =
        [
            Show(EnemyFrameId.YappingMawFacingUpRightJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingUpRightJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingUpRightJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingUpRightJawsClosing, 3),
        ],
        [P.AttackingRight] =
        [
            Show(EnemyFrameId.YappingMawFacingRightJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingRightJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingRightJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingRightJawsClosing, 3),
        ],
        [P.AttackingDownRight] =
        [
            Show(EnemyFrameId.YappingMawFacingDownRightJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingDownRightJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingDownRightJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingDownRightJawsClosing, 3),
        ],
        [P.AttackingDown] =
        [
            Show(EnemyFrameId.YappingMawFacingDownJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingDownJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
        ],
        [P.AttackingDownLeft] =
        [
            Show(EnemyFrameId.YappingMawFacingDownLeftJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingDownLeftJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingDownLeftJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingDownLeftJawsClosing, 3),
        ],
        [P.AttackingLeft] =
        [
            Show(EnemyFrameId.YappingMawFacingLeftJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingLeftJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingLeftJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingLeftJawsClosing, 3),
        ],
        [P.AttackingUpLeft] =
        [
            Show(EnemyFrameId.YappingMawFacingUpLeftJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingUpLeftJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
            Show(EnemyFrameId.YappingMawFacingUpLeftJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingUpLeftJawsClosing, 3),
        ],
        [P.CooldownUpRight] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.UpRight)),
            Show(EnemyFrameId.YappingMawFacingUpRightJawsShut, 4),
        ],
        [P.CooldownUp] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Up)),
        ],
        [P.CooldownUpLoop] =
        [
            Show(EnemyFrameId.YappingMawFacingUpJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
            Show(EnemyFrameId.YappingMawFacingUpJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
        ],
        [P.CooldownUpLeft] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.UpLeft)),
            Show(EnemyFrameId.YappingMawFacingUpLeftJawsShut, 4),
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Up)),
        ],
        [P.CooldownUpLeftLoop] =
        [
            Show(EnemyFrameId.YappingMawFacingUpJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
            Show(EnemyFrameId.YappingMawFacingUpJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingUpJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
        ],
        [P.CooldownDownRight] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.DownRight)),
            Show(EnemyFrameId.YappingMawFacingDownRightJawsShut, 4),
        ],
        [P.CooldownDown] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Down)),
        ],
        [P.CooldownDownLoop] =
        [
            Show(EnemyFrameId.YappingMawFacingDownJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
            Show(EnemyFrameId.YappingMawFacingDownJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
        ],
        [P.CooldownDownLeft] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.DownLeft)),
            Show(EnemyFrameId.YappingMawFacingDownLeftJawsShut, 4),
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Down)),
        ],
        [P.CooldownDownLeftLoop] =
        [
            Show(EnemyFrameId.YappingMawFacingDownJawsShut, 80),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
            Show(EnemyFrameId.YappingMawFacingDownJawsWide, 5),
            Show(EnemyFrameId.YappingMawFacingDownJawsClosing, 3),
            Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
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
