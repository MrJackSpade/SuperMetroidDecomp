using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailProgram;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations as typed programs (#641). Each
/// frame shows a <see cref="MaridiaLargeSnailFrame"/> pose and each callback becomes a <see cref="MaridiaLargeSnailCue"/> with its typed operands.
/// </summary>
internal static class MaridiaLargeSnailAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Segments { get; } = new(
        new Dictionary<MaridiaLargeSnailProgram, AnimationBeat<MaridiaLargeSnailFrame, MaridiaLargeSnailCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Show(0, 1),
        ],
        [P.FacingLeftAttacking] =
        [
            Show(0, 16),
            Show(1, 16),
            Cue(new(K.PlaySplash)),
            Show(2, 16),
            Show(3, 2),
            Show(4, 3),
            Show(5, 4),
            Show(6, 2),
            Show(7, 3),
            Show(6, 1),
            Show(5, 3),
            Show(4, 2),
            Show(3, 1),
            Show(2, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingLeftRollingForwards] =
        [
            Show(2, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(8, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(9, 7),
            Show(10, 7),
            Show(11, 7),
            Show(12, 7),
            Show(13, 7),
            Show(14, 7),
        ],
        [P.FacingLeftRollingBackwards] =
        [
            Show(14, 7),
            Show(13, 7),
            Show(12, 7),
            Show(11, 7),
            Show(10, 7),
            Show(9, 7),
            Show(8, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(2, 7),
            Cue(new(K.DisallowAttackRotation)),
        ],
        [P.FacingRightIdle] =
        [
            Show(15, 1),
        ],
        [P.FacingRightAttacking] =
        [
            Show(15, 16),
            Show(16, 16),
            Show(17, 16),
            Cue(new(K.PlaySplash)),
            Show(18, 2),
            Show(19, 3),
            Show(20, 4),
            Show(21, 2),
            Show(22, 3),
            Show(21, 1),
            Show(20, 3),
            Show(19, 2),
            Show(18, 1),
            Show(17, 18),
            Cue(new(K.FinishAttack)),
        ],
        [P.FacingRightRollingForwards] =
        [
            Show(17, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(23, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(24, 7),
            Show(25, 7),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(29, 7),
        ],
        [P.FacingRightRollingBackwards] =
        [
            Show(29, 7),
            Show(28, 7),
            Show(27, 7),
            Show(26, 7),
            Show(25, 7),
            Show(24, 7),
            Show(23, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(17, 7),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Show(int pose, int duration) => new AnimationBeat<MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Cue(MaridiaLargeSnailCue cue) => new AnimationBeat<MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.Event(cue);
}
