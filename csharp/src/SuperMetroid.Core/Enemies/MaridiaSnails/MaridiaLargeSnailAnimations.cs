using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="MaridiaLargeSnailCue"/> with its typed operands.
/// </summary>
internal static class MaridiaLargeSnailAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MaridiaLargeSnailProgram, EnemyFrameId, MaridiaLargeSnailCue> Segments { get; } = new(
        new Dictionary<MaridiaLargeSnailProgram, AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Show(EnemyFrameId.OumPose0, 1),
        ],
        [P.FacingLeftAttacking] =
        [
            Show(EnemyFrameId.OumPose0, 16),
            Show(EnemyFrameId.OumPose1, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumPose2, 16),
            Show(EnemyFrameId.OumPose3, 2),
            Show(EnemyFrameId.OumPose4, 3),
            Show(EnemyFrameId.OumPose5, 4),
            Show(EnemyFrameId.OumPose6, 2),
            Show(EnemyFrameId.OumPose7, 3),
            Show(EnemyFrameId.OumPose6, 1),
            Show(EnemyFrameId.OumPose5, 3),
            Show(EnemyFrameId.OumPose4, 2),
            Show(EnemyFrameId.OumPose3, 1),
            Show(EnemyFrameId.OumPose2, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingLeftRollingForwards] =
        [
            Show(EnemyFrameId.OumPose2, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumPose8, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumPose9, 7),
            Show(EnemyFrameId.OumPose10, 7),
            Show(EnemyFrameId.OumPose11, 7),
            Show(EnemyFrameId.OumPose12, 7),
            Show(EnemyFrameId.OumPose13, 7),
            Show(EnemyFrameId.OumPose14, 7),
        ],
        [P.FacingLeftRollingBackwards] =
        [
            Show(EnemyFrameId.OumPose14, 7),
            Show(EnemyFrameId.OumPose13, 7),
            Show(EnemyFrameId.OumPose12, 7),
            Show(EnemyFrameId.OumPose11, 7),
            Show(EnemyFrameId.OumPose10, 7),
            Show(EnemyFrameId.OumPose9, 7),
            Show(EnemyFrameId.OumPose8, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumPose2, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
        [P.FacingRightIdle] =
        [
            Show(EnemyFrameId.OumPose15, 1),
        ],
        [P.FacingRightAttacking] =
        [
            Show(EnemyFrameId.OumPose15, 16),
            Show(EnemyFrameId.OumPose16, 16),
            Show(EnemyFrameId.OumPose17, 16),
            Cue(new(K.PlaySplash)),
            Show(EnemyFrameId.OumPose18, 2),
            Show(EnemyFrameId.OumPose19, 3),
            Show(EnemyFrameId.OumPose20, 4),
            Show(EnemyFrameId.OumPose21, 2),
            Show(EnemyFrameId.OumPose22, 3),
            Show(EnemyFrameId.OumPose21, 1),
            Show(EnemyFrameId.OumPose20, 3),
            Show(EnemyFrameId.OumPose19, 2),
            Show(EnemyFrameId.OumPose18, 1),
            Show(EnemyFrameId.OumPose17, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingRightRollingForwards] =
        [
            Show(EnemyFrameId.OumPose17, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumPose23, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(EnemyFrameId.OumPose24, 7),
            Show(EnemyFrameId.OumPose25, 7),
            Show(EnemyFrameId.OumPose26, 7),
            Show(EnemyFrameId.OumPose27, 7),
            Show(EnemyFrameId.OumPose28, 7),
            Show(EnemyFrameId.OumPose29, 7),
        ],
        [P.FacingRightRollingBackwards] =
        [
            Show(EnemyFrameId.OumPose29, 7),
            Show(EnemyFrameId.OumPose28, 7),
            Show(EnemyFrameId.OumPose27, 7),
            Show(EnemyFrameId.OumPose26, 7),
            Show(EnemyFrameId.OumPose25, 7),
            Show(EnemyFrameId.OumPose24, 7),
            Show(EnemyFrameId.OumPose23, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(EnemyFrameId.OumPose17, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MaridiaLargeSnailProgram, Animation<MaridiaLargeSnailProgram>> StartingAt { get; } = new Dictionary<MaridiaLargeSnailProgram, Animation<MaridiaLargeSnailProgram>>
    {
        [P.FacingLeftIdle] = new([new(P.FacingLeftIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingLeftAttacking] = new([new(P.FacingLeftAttacking, 0, 15, 1)], AnimationEnd.Holds),
        [P.FacingLeftRollingForwards] = new([new(P.FacingLeftRollingForwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingLeftRollingBackwards] = new([new(P.FacingLeftRollingBackwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightIdle] = new([new(P.FacingRightIdle, 0, 1, 1)], AnimationEnd.Holds),
        [P.FacingRightAttacking] = new([new(P.FacingRightAttacking, 0, 15, 1)], AnimationEnd.Holds),
        [P.FacingRightRollingForwards] = new([new(P.FacingRightRollingForwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FacingRightRollingBackwards] = new([new(P.FacingRightRollingBackwards, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MaridiaLargeSnailProgram> StartingWith(MaridiaLargeSnailProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue> Cue(MaridiaLargeSnailCue cue) => new AnimationBeat<EnemyFrameId, MaridiaLargeSnailCue>.Event(cue);
}
