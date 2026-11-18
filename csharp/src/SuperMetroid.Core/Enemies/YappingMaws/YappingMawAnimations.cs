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
        [P.ChewingUp] =
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
        [P.CooldownDownRight] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.DownRight)),
            Show(EnemyFrameId.YappingMawFacingDownRightJawsShut, 4),
        ],
        [P.CooldownDown] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Down)),
        ],
        [P.ChewingDown] =
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
    });

    /// <summary>
    /// The animation that starts at each segment an owner starts, each segment played whole: an attack's snap, or a
    /// cooldown's hold of Samus and then its aim's chewing, looped; no owner starts the chewing itself (#2276).
    /// </summary>
    internal static IReadOnlyDictionary<YappingMawSegment, Animation<YappingMawSegment>> StartingAt { get; } = new Dictionary<YappingMawSegment, Animation<YappingMawSegment>>
    {
        [P.AttackingUp] = new([Segments.Whole(P.AttackingUp)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingUpRight] = new([Segments.Whole(P.AttackingUpRight)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingRight] = new([Segments.Whole(P.AttackingRight)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDownRight] = new([Segments.Whole(P.AttackingDownRight)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDown] = new([Segments.Whole(P.AttackingDown)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingDownLeft] = new([Segments.Whole(P.AttackingDownLeft)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingLeft] = new([Segments.Whole(P.AttackingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.AttackingUpLeft] = new([Segments.Whole(P.AttackingUpLeft)], AnimationEnd.LoopsFrom(0)),
        [P.CooldownUpRight] = new([Segments.Whole(P.CooldownUpRight), Segments.Whole(P.CooldownUp), Segments.Whole(P.ChewingUp)], AnimationEnd.LoopsFrom(2)),
        [P.CooldownUp] = new([Segments.Whole(P.CooldownUp), Segments.Whole(P.ChewingUp)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownUpLeft] = new([Segments.Whole(P.CooldownUpLeft), Segments.Whole(P.ChewingUp)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownDownRight] = new([Segments.Whole(P.CooldownDownRight), Segments.Whole(P.CooldownDown), Segments.Whole(P.ChewingDown)], AnimationEnd.LoopsFrom(2)),
        [P.CooldownDown] = new([Segments.Whole(P.CooldownDown), Segments.Whole(P.ChewingDown)], AnimationEnd.LoopsFrom(1)),
        [P.CooldownDownLeft] = new([Segments.Whole(P.CooldownDownLeft), Segments.Whole(P.ChewingDown)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YappingMawSegment> StartingWith(YappingMawSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Cue(YappingMawCue cue) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Event(cue);
}
