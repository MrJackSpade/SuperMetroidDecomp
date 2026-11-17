using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MaridiaLargeSnailCue"/> naming what it does.
/// </summary>
internal static class MaridiaLargeSnailAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MaridiaLargeSnailSegment, EnemyFrameId, MaridiaLargeSnailCue> Segments { get; } = new(
        new Dictionary<MaridiaLargeSnailSegment, AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Hold(EnemyFrameId.OumFacingLeftTurned0),
        ],
        [P.FacingLeftAttacking] =
        [
            Show(EnemyFrameId.OumFacingLeftTurned0, 16),
            Show(EnemyFrameId.OumFacingLeftVentOpening, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumFacingLeftVentOpen, 16),
            Show(EnemyFrameId.OumFacingLeftJetFlaring, 2),
            Show(EnemyFrameId.OumFacingLeftJetShort, 3),
            Show(EnemyFrameId.OumFacingLeftJetMedium, 4),
            Show(EnemyFrameId.OumFacingLeftJetLong, 2),
            Show(EnemyFrameId.OumFacingLeftJetLongest, 3),
            Show(EnemyFrameId.OumFacingLeftJetLong, 1),
            Show(EnemyFrameId.OumFacingLeftJetMedium, 3),
            Show(EnemyFrameId.OumFacingLeftJetShort, 2),
            Show(EnemyFrameId.OumFacingLeftJetFlaring, 1),
            Show(EnemyFrameId.OumFacingLeftVentOpen, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingLeftRollingForwards] =
        [
            Show(EnemyFrameId.OumFacingLeftVentOpen, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftTurned45, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftTurned90, 7),
            Show(EnemyFrameId.OumFacingLeftTurned135, 7),
            Show(EnemyFrameId.OumFacingLeftTurned180, 7),
            Show(EnemyFrameId.OumFacingLeftTurned225, 7),
            Show(EnemyFrameId.OumFacingLeftTurned270, 7),
            Show(EnemyFrameId.OumFacingLeftTurned315, 7),
        ],
        [P.FacingLeftRollingBackwards] =
        [
            Show(EnemyFrameId.OumFacingLeftTurned315, 7),
            Show(EnemyFrameId.OumFacingLeftTurned270, 7),
            Show(EnemyFrameId.OumFacingLeftTurned225, 7),
            Show(EnemyFrameId.OumFacingLeftTurned180, 7),
            Show(EnemyFrameId.OumFacingLeftTurned135, 7),
            Show(EnemyFrameId.OumFacingLeftTurned90, 7),
            Show(EnemyFrameId.OumFacingLeftTurned45, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingLeftVentOpen, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
        [P.FacingRightIdle] =
        [
            Hold(EnemyFrameId.OumFacingRightTurned0),
        ],
        [P.FacingRightAttacking] =
        [
            Show(EnemyFrameId.OumFacingRightTurned0, 16),
            Show(EnemyFrameId.OumFacingRightVentOpening, 16),
            Show(EnemyFrameId.OumFacingRightVentOpen, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumFacingRightJetFlaring, 2),
            Show(EnemyFrameId.OumFacingRightJetShort, 3),
            Show(EnemyFrameId.OumFacingRightJetMedium, 4),
            Show(EnemyFrameId.OumFacingRightJetLong, 2),
            Show(EnemyFrameId.OumFacingRightJetLongest, 3),
            Show(EnemyFrameId.OumFacingRightJetLong, 1),
            Show(EnemyFrameId.OumFacingRightJetMedium, 3),
            Show(EnemyFrameId.OumFacingRightJetShort, 2),
            Show(EnemyFrameId.OumFacingRightJetFlaring, 1),
            Show(EnemyFrameId.OumFacingRightVentOpen, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingRightRollingForwards] =
        [
            Show(EnemyFrameId.OumFacingRightVentOpen, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightTurned45, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightTurned90, 7),
            Show(EnemyFrameId.OumFacingRightTurned135, 7),
            Show(EnemyFrameId.OumFacingRightTurned180, 7),
            Show(EnemyFrameId.OumFacingRightTurned225, 7),
            Show(EnemyFrameId.OumFacingRightTurned270, 7),
            Show(EnemyFrameId.OumFacingRightTurned315, 7),
        ],
        [P.FacingRightRollingBackwards] =
        [
            Show(EnemyFrameId.OumFacingRightTurned315, 7),
            Show(EnemyFrameId.OumFacingRightTurned270, 7),
            Show(EnemyFrameId.OumFacingRightTurned225, 7),
            Show(EnemyFrameId.OumFacingRightTurned180, 7),
            Show(EnemyFrameId.OumFacingRightTurned135, 7),
            Show(EnemyFrameId.OumFacingRightTurned90, 7),
            Show(EnemyFrameId.OumFacingRightTurned45, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumFacingRightVentOpen, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MaridiaLargeSnailSegment, Animation<MaridiaLargeSnailSegment>> StartingAt { get; } = new Dictionary<MaridiaLargeSnailSegment, Animation<MaridiaLargeSnailSegment>>
    {
        [P.FacingLeftIdle] = new([Whole(P.FacingLeftIdle)], AnimationEnd.Holds),
        [P.FacingLeftAttacking] = new([Whole(P.FacingLeftAttacking)], AnimationEnd.Holds),
        [P.FacingLeftRollingForwards] = new([Whole(P.FacingLeftRollingForwards)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRollingBackwards] = new([Whole(P.FacingLeftRollingBackwards)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightIdle] = new([Whole(P.FacingRightIdle)], AnimationEnd.Holds),
        [P.FacingRightAttacking] = new([Whole(P.FacingRightAttacking)], AnimationEnd.Holds),
        [P.FacingRightRollingForwards] = new([Whole(P.FacingRightRollingForwards)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRollingBackwards] = new([Whole(P.FacingRightRollingBackwards)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MaridiaLargeSnailSegment> StartingWith(MaridiaLargeSnailSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<MaridiaLargeSnailSegment> Whole(MaridiaLargeSnailSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Cue(MaridiaLargeSnailCue cue) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Event(cue);
}
