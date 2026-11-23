using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.OumCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.OumSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="OumCue"/> naming what it does.
/// </summary>
internal static partial class OumAnimations
{
    /// <summary>An Oum facing left's roll: its vent open and each eighth of a turn round from it.</summary>
    private static readonly EnemyFrameId[] LeftTurns = [EnemyFrameId.OumFacingLeftVentOpen, EnemyFrameId.OumFacingLeftTurned45, EnemyFrameId.OumFacingLeftTurned90, EnemyFrameId.OumFacingLeftTurned135, EnemyFrameId.OumFacingLeftTurned180, EnemyFrameId.OumFacingLeftTurned225, EnemyFrameId.OumFacingLeftTurned270, EnemyFrameId.OumFacingLeftTurned315];

    /// <summary>An Oum facing right's roll: its vent open and each eighth of a turn round from it.</summary>
    private static readonly EnemyFrameId[] RightTurns = [EnemyFrameId.OumFacingRightVentOpen, EnemyFrameId.OumFacingRightTurned45, EnemyFrameId.OumFacingRightTurned90, EnemyFrameId.OumFacingRightTurned135, EnemyFrameId.OumFacingRightTurned180, EnemyFrameId.OumFacingRightTurned225, EnemyFrameId.OumFacingRightTurned270, EnemyFrameId.OumFacingRightTurned315];

    /// <summary>An Oum facing left's attack poses: resting, its vent opening, and its jet flaring, short, medium, long and longest.</summary>
    private static readonly EnemyFrameId[] LeftJet = [EnemyFrameId.OumFacingLeftTurned0, EnemyFrameId.OumFacingLeftVentOpening, EnemyFrameId.OumFacingLeftJetFlaring, EnemyFrameId.OumFacingLeftJetShort, EnemyFrameId.OumFacingLeftJetMedium, EnemyFrameId.OumFacingLeftJetLong, EnemyFrameId.OumFacingLeftJetLongest];

    /// <summary>An Oum facing right's attack poses: resting, its vent opening, and its jet flaring, short, medium, long and longest.</summary>
    private static readonly EnemyFrameId[] RightJet = [EnemyFrameId.OumFacingRightTurned0, EnemyFrameId.OumFacingRightVentOpening, EnemyFrameId.OumFacingRightJetFlaring, EnemyFrameId.OumFacingRightJetShort, EnemyFrameId.OumFacingRightJetMedium, EnemyFrameId.OumFacingRightJetLong, EnemyFrameId.OumFacingRightJetLongest];

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<OumSegment, EnemyFrameId, OumCue> Segments { get; } = new(
        new Dictionary<OumSegment, AnimationBeat<EnemyFrameId, OumCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Hold(EnemyFrameId.OumFacingLeftTurned0),
        ],
        [P.FacingLeftAttacking] = Attacking(LeftTurns, LeftJet, splashBeforeVentOpens: [Splash], splashAsVentOpens: []),
        [P.FacingLeftRollingForwards] = Roll(LeftTurns, windowAfter: 0),
        [P.FacingLeftRollingBackwards] = Roll([.. Enumerable.Reverse(LeftTurns)], windowAfter: LeftTurns.Length - 2),
        [P.FacingRightIdle] =
        [
            Hold(EnemyFrameId.OumFacingRightTurned0),
        ],
        [P.FacingRightAttacking] = Attacking(RightTurns, RightJet, splashBeforeVentOpens: [], splashAsVentOpens: [Splash]),
        [P.FacingRightRollingForwards] = Roll(RightTurns, windowAfter: 0),
        [P.FacingRightRollingBackwards] = Roll([.. Enumerable.Reverse(RightTurns)], windowAfter: RightTurns.Length - 2),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<OumSegment, Animation<OumSegment>> StartingAt { get; } = new Dictionary<OumSegment, Animation<OumSegment>>
    {
        [P.FacingLeftIdle] = new([Segments.Whole(P.FacingLeftIdle)], AnimationEnd.Holds),
        [P.FacingLeftAttacking] = new([Segments.Whole(P.FacingLeftAttacking)], AnimationEnd.Holds),
        [P.FacingLeftRollingForwards] = Animation<OumSegment>.Looping([Segments.Whole(P.FacingLeftRollingForwards)]),
        [P.FacingLeftRollingBackwards] = Animation<OumSegment>.Looping([Segments.Whole(P.FacingLeftRollingBackwards)]),
        [P.FacingRightIdle] = new([Segments.Whole(P.FacingRightIdle)], AnimationEnd.Holds),
        [P.FacingRightAttacking] = new([Segments.Whole(P.FacingRightAttacking)], AnimationEnd.Holds),
        [P.FacingRightRollingForwards] = Animation<OumSegment>.Looping([Segments.Whole(P.FacingRightRollingForwards)]),
        [P.FacingRightRollingBackwards] = Animation<OumSegment>.Looping([Segments.Whole(P.FacingRightRollingBackwards)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<OumSegment> StartingWith(OumSegment segment) => StartingAt[segment];

    /// <summary>
    /// A roll through <paramref name="turns"/> in play order, seven updates a pose: an attack may turn it only between the pose
    /// at <paramref name="windowAfter"/> and the next.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, OumCue>[] Roll(EnemyFrameId[] turns, int windowAfter) =>
    [
        .. turns[..(windowAfter + 1)].Select(pose => Show(pose, 7)),
        Cue(new(K.AllowAttackRotation)),
        Show(turns[windowAfter + 1], 7),
        Cue(new(K.DisallowAttackRotation)),
        .. turns[(windowAfter + 2)..].Select(pose => Show(pose, 7)),
    ];

    /// <summary>
    /// An attack over a facing's <paramref name="turns"/> and <paramref name="jet"/> poses: the Oum's vent opens, its jet flares
    /// out and back and its vent stays open before the attack finishes; its splash plays before its vent opens
    /// (<paramref name="splashBeforeVentOpens"/>) or as it opens (<paramref name="splashAsVentOpens"/>), as its facing's attack does.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, OumCue>[] Attacking(EnemyFrameId[] turns, EnemyFrameId[] jet,
        AnimationBeat<EnemyFrameId, OumCue>[] splashBeforeVentOpens, AnimationBeat<EnemyFrameId, OumCue>[] splashAsVentOpens) =>
    [
        Show(jet[0], 16),
        Show(jet[1], 16),
        .. splashBeforeVentOpens,
        Show(turns[0], 16),
        .. splashAsVentOpens,
        Show(jet[2], 2),
        Show(jet[3], 3),
        Show(jet[4], 4),
        Show(jet[5], 2),
        Show(jet[6], 3),
        Show(jet[5], 1),
        Show(jet[4], 3),
        Show(jet[3], 2),
        Show(jet[2], 1),
        Show(turns[0], 18),
        Cue(new(K.FinishAttack)),
    ];

    /// <summary>An attack's splash.</summary>
    private static AnimationBeat<EnemyFrameId, OumCue> Splash => Cue(new(K.PlaySplash));

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, OumCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, OumCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, OumCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, OumCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, OumCue> Cue(OumCue cue) => new AnimationBeat<EnemyFrameId, OumCue>.Event(cue);
}
