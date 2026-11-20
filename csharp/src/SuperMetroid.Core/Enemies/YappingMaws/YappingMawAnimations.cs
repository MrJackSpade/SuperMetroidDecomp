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
        [P.AttackingUp] = Bite(EnemyFrameId.YappingMawFacingUpJawsWide, EnemyFrameId.YappingMawFacingUpJawsClosing, EnemyFrameId.YappingMawFacingUpJawsShut),
        [P.AttackingUpRight] = Bite(EnemyFrameId.YappingMawFacingUpRightJawsWide, EnemyFrameId.YappingMawFacingUpRightJawsClosing, EnemyFrameId.YappingMawFacingUpRightJawsShut),
        [P.AttackingRight] = Bite(EnemyFrameId.YappingMawFacingRightJawsWide, EnemyFrameId.YappingMawFacingRightJawsClosing, EnemyFrameId.YappingMawFacingRightJawsShut),
        [P.AttackingDownRight] = Bite(EnemyFrameId.YappingMawFacingDownRightJawsWide, EnemyFrameId.YappingMawFacingDownRightJawsClosing, EnemyFrameId.YappingMawFacingDownRightJawsShut),
        [P.AttackingDown] = Bite(EnemyFrameId.YappingMawFacingDownJawsWide, EnemyFrameId.YappingMawFacingDownJawsClosing, EnemyFrameId.YappingMawFacingDownJawsShut),
        [P.AttackingDownLeft] = Bite(EnemyFrameId.YappingMawFacingDownLeftJawsWide, EnemyFrameId.YappingMawFacingDownLeftJawsClosing, EnemyFrameId.YappingMawFacingDownLeftJawsShut),
        [P.AttackingLeft] = Bite(EnemyFrameId.YappingMawFacingLeftJawsWide, EnemyFrameId.YappingMawFacingLeftJawsClosing, EnemyFrameId.YappingMawFacingLeftJawsShut),
        [P.AttackingUpLeft] = Bite(EnemyFrameId.YappingMawFacingUpLeftJawsWide, EnemyFrameId.YappingMawFacingUpLeftJawsClosing, EnemyFrameId.YappingMawFacingUpLeftJawsShut),
        [P.CooldownUpRight] = SideCooldown(YappingMawAim.UpRight, EnemyFrameId.YappingMawFacingUpRightJawsShut),
        [P.CooldownUp] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Up)),
        ],
        [P.ChewingUp] = Chewing(EnemyFrameId.YappingMawFacingUpJawsShut, EnemyFrameId.YappingMawFacingUpJawsClosing, EnemyFrameId.YappingMawFacingUpJawsWide),
        [P.CooldownUpLeft] = SideCooldown(YappingMawAim.UpLeft, EnemyFrameId.YappingMawFacingUpLeftJawsShut),
        [P.CooldownDownRight] = SideCooldown(YappingMawAim.DownRight, EnemyFrameId.YappingMawFacingDownRightJawsShut),
        [P.CooldownDown] =
        [
            Cue(new YappingMawCue.HoldSamus(YappingMawAim.Down)),
        ],
        [P.ChewingDown] = Chewing(EnemyFrameId.YappingMawFacingDownJawsShut, EnemyFrameId.YappingMawFacingDownJawsClosing, EnemyFrameId.YappingMawFacingDownJawsWide),
        [P.CooldownDownLeft] = SideCooldown(YappingMawAim.DownLeft, EnemyFrameId.YappingMawFacingDownLeftJawsShut),
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

    /// <summary>A bite over an aim's jaws, <paramref name="jawsWide"/>, <paramref name="jawsClosing"/>, <paramref name="jawsShut"/>: wide, closing, the bite's sound, shut, and closing again.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue>[] Bite(EnemyFrameId jawsWide, EnemyFrameId jawsClosing, EnemyFrameId jawsShut) =>
    [
        Show(jawsWide, 5),
        Show(jawsClosing, 3),
        Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
        Show(jawsShut, 80),
        Show(jawsClosing, 3),
    ];

    /// <summary>A side aim's cooldown: it holds Samus <paramref name="aim"/> and shows its shut jaws, <paramref name="jawsShut"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue>[] SideCooldown(YappingMawAim aim, EnemyFrameId jawsShut) =>
    [
        Cue(new YappingMawCue.HoldSamus(aim)),
        Show(jawsShut, 4),
    ];

    /// <summary>Chewing over a reach's jaws, <paramref name="jawsShut"/>, <paramref name="jawsClosing"/>, <paramref name="jawsWide"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue>[] Chewing(EnemyFrameId jawsShut, EnemyFrameId jawsClosing, EnemyFrameId jawsWide) =>
    [
        Show(jawsShut, 80),
        Show(jawsClosing, 3),
        Show(jawsWide, 5),
        Show(jawsClosing, 3),
        Cue(new YappingMawCue.Signal(K.PlayBiteSound)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YappingMawCue> Cue(YappingMawCue cue) => new AnimationBeat<EnemyFrameId, YappingMawCue>.Event(cue);
}
