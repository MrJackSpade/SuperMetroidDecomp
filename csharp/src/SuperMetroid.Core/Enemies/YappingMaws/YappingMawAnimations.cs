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
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner starts, each segment played whole: an attack's snap, or a
    /// cooldown's hold of Samus and then its aim's chewing, looped; no owner starts the chewing itself (#2276). A cooldown
    /// aimed off to one side holds Samus there and then straight up or down, as the straight cooldown does (#2451).
    /// </summary>
    private static Dictionary<YappingMawSegment, Animation<YappingMawSegment>> StartingAt { get; } = new Dictionary<YappingMawSegment, Animation<YappingMawSegment>>
    {
        [P.AttackingUp] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingUp)]),
        [P.AttackingUpRight] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingUpRight)]),
        [P.AttackingRight] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingRight)]),
        [P.AttackingDownRight] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingDownRight)]),
        [P.AttackingDown] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingDown)]),
        [P.AttackingDownLeft] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingDownLeft)]),
        [P.AttackingLeft] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingLeft)]),
        [P.AttackingUpLeft] = Animation<YappingMawSegment>.Looping([Segments.Whole(P.AttackingUpLeft)]),
        [P.CooldownUpRight] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownUpRight), Segments.Whole(P.CooldownUp)], [Segments.Whole(P.ChewingUp)]),
        [P.CooldownUp] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownUp)], [Segments.Whole(P.ChewingUp)]),
        [P.CooldownUpLeft] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownUpLeft), Segments.Whole(P.CooldownUp)], [Segments.Whole(P.ChewingUp)]),
        [P.CooldownDownRight] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownDownRight), Segments.Whole(P.CooldownDown)], [Segments.Whole(P.ChewingDown)]),
        [P.CooldownDown] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownDown)], [Segments.Whole(P.ChewingDown)]),
        [P.CooldownDownLeft] = Animation<YappingMawSegment>.LeadingInto([Segments.Whole(P.CooldownDownLeft), Segments.Whole(P.CooldownDown)], [Segments.Whole(P.ChewingDown)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YappingMawSegment> StartingWith(YappingMawSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Cue(YappingMawCue cue) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Event(cue);
}
